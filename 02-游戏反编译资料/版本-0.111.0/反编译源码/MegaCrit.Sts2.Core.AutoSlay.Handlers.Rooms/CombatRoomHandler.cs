using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms;

/// <summary>
/// Handles combat rooms (Monster, Elite, Boss).
/// Applies massive defensive buffs and plays all cards each turn.
/// </summary>
public class CombatRoomHandler : IRoomHandler, IHandler
{
	public RoomType[] HandledTypes => new RoomType[3]
	{
		RoomType.Monster,
		RoomType.Elite,
		RoomType.Boss
	};

	public TimeSpan Timeout => TimeSpan.FromMinutes(5L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for combat to start");
		await WaitHelper.Until(() => CombatManager.Instance.IsInProgress, ct, AutoSlayConfig.nodeWaitTimeout, "Combat not started");
		AutoSlayLog.Action("Combat started, applying defensive buffs");
		Player player = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState());
		Creature playerCreature = player.Creature;
		await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), playerCreature, 999m, playerCreature, null);
		await PowerCmd.Apply<RegenPower>(new ThrowingPlayerChoiceContext(), playerCreature, 999m, playerCreature, null);
		int turnCount = 0;
		int lastPlayedTurnNumber = 0;
		while (CombatManager.Instance.IsInProgress && turnCount < 100)
		{
			ct.ThrowIfCancellationRequested();
			turnCount++;
			AutoSlayer.CurrentWatchdog?.Reset($"Waiting for a playable turn after turn {lastPlayedTurnNumber}");
			await WaitHelper.Until(() => IsPlayableTurnAfter(player, lastPlayedTurnNumber) || !CombatManager.Instance.IsInProgress, ct, TimeSpan.FromSeconds(28L), () => $"No playable turn after turn {lastPlayedTurnNumber} (phase={player.PlayerCombatState?.Phase}, turnNumber={player.PlayerCombatState?.TurnNumber})");
			if (!CombatManager.Instance.IsInProgress)
			{
				break;
			}
			int playingTurnNumber = (lastPlayedTurnNumber = player.PlayerCombatState.TurnNumber);
			AutoSlayer.CurrentWatchdog?.Reset($"Combat turn {playingTurnNumber}");
			if (playingTurnNumber >= 3)
			{
				await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), playerCreature, 200m, playerCreature, null);
			}
			AutoSlayLog.Action($"Turn {playingTurnNumber}: playing cards");
			await UseAllPotions(player, random, ct);
			int cardsPlayed = 0;
			HashSet<CardModel> attemptedCards = new HashSet<CardModel>();
			while (cardsPlayed < 50)
			{
				PlayerCombatState? playerCombatState = player.PlayerCombatState;
				if (playerCombatState == null || playerCombatState.Phase != PlayerTurnPhase.Play || player.PlayerCombatState.TurnNumber != playingTurnNumber)
				{
					break;
				}
				ct.ThrowIfCancellationRequested();
				if (cardsPlayed > 0 && cardsPlayed % 10 == 0)
				{
					AutoSlayer.CurrentWatchdog?.Reset($"Combat turn {playingTurnNumber}, played {cardsPlayed} cards");
				}
				CardPile pile = PileType.Hand.GetPile(player);
				UnplayableReason reason;
				AbstractModel preventer;
				List<CardModel> list = pile.Cards.Where((CardModel c) => c.CanPlay(out reason, out preventer) && !attemptedCards.Contains(c)).ToList();
				if (list.Count == 0)
				{
					AutoSlayLog.Action("No more playable cards, ending turn");
					break;
				}
				CardModel cardModel = random.NextItem(list);
				Creature randomTarget = GetRandomTarget(cardModel, random);
				attemptedCards.Add(cardModel);
				AutoSlayLog.Info("Playing " + cardModel.Id.Entry);
				await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), cardModel, randomTarget);
				cardsPlayed++;
				await Task.Delay(100, ct);
			}
			if (!CombatManager.Instance.IsInProgress)
			{
				break;
			}
			PlayerCombatState? playerCombatState2 = player.PlayerCombatState;
			if (playerCombatState2 != null && playerCombatState2.Phase == PlayerTurnPhase.Play && player.PlayerCombatState.TurnNumber == playingTurnNumber)
			{
				PlayerCmd.EndTurn(player, canBackOut: false);
				continue;
			}
			AutoSlayLog.Action($"Turn {playingTurnNumber} was ended by the game, not by AutoSlay");
		}
		AutoSlayer.CurrentWatchdog?.Reset("Waiting for combat to end");
		await WaitHelper.Until(() => !CombatManager.Instance.IsInProgress, ct, TimeSpan.FromSeconds(28L), () => $"Combat did not end after {turnCount} turns (phase={player.PlayerCombatState?.Phase}, turnNumber={player.PlayerCombatState?.TurnNumber})");
		AutoSlayLog.Action("Combat finished");
	}

	/// <summary>True when the game is ready for AutoSlay to act in a turn it has not played yet.</summary>
	/// <remarks>Greater-than, not not-equal: TurnNumber restarts at 1 each combat (Player.ResetCombatState).</remarks>
	private static bool IsPlayableTurnAfter(Player player, int lastPlayedTurnNumber)
	{
		PlayerCombatState playerCombatState = player.PlayerCombatState;
		if (playerCombatState != null && playerCombatState.Phase == PlayerTurnPhase.Play)
		{
			return playerCombatState.TurnNumber > lastPlayedTurnNumber;
		}
		return false;
	}

	private static Creature? GetRandomTarget(CardModel card, Rng random)
	{
		if (card.TargetType != TargetType.AnyEnemy)
		{
			return null;
		}
		ICombatState combatState = card.CombatState;
		if (combatState == null)
		{
			return null;
		}
		List<Creature> list = combatState.HittableEnemies.ToList();
		if (list.Count == 0)
		{
			return null;
		}
		return random.NextItem(list);
	}

	private static async Task UseAllPotions(Player player, Rng random, CancellationToken ct)
	{
		List<PotionModel> list = player.Potions.ToList();
		if (list.Count == 0)
		{
			return;
		}
		AutoSlayLog.Action($"Using {list.Count} potion(s)");
		ICombatState combatState = player.Creature.CombatState;
		foreach (PotionModel item in list)
		{
			ct.ThrowIfCancellationRequested();
			PlayerCombatState? playerCombatState = player.PlayerCombatState;
			if (playerCombatState != null && playerCombatState.Phase == PlayerTurnPhase.Play && CombatManager.Instance.IsInProgress)
			{
				Creature potionTarget = GetPotionTarget(item, combatState, random);
				if (potionTarget == null && item.TargetType.IsSingleTarget())
				{
					AutoSlayLog.Warn("Skipping potion " + item.Id.Entry + ": no valid target");
					continue;
				}
				AutoSlayLog.Info("Using potion: " + item.Id.Entry);
				item.EnqueueManualUse(potionTarget);
				await Task.Delay(300, ct);
				continue;
			}
			break;
		}
	}

	private static Creature? GetPotionTarget(PotionModel potion, ICombatState? combatState, Rng random)
	{
		if (combatState == null)
		{
			return null;
		}
		return potion.TargetType switch
		{
			TargetType.AnyEnemy => random.NextItem(combatState.HittableEnemies.ToList()), 
			TargetType.AnyAlly => combatState.PlayerCreatures.FirstOrDefault((Creature c) => c.IsAlive), 
			TargetType.AnyPlayer => combatState.PlayerCreatures.FirstOrDefault((Creature c) => c.IsAlive), 
			TargetType.Self => combatState.PlayerCreatures.FirstOrDefault((Creature c) => c.IsAlive), 
			_ => null, 
		};
	}
}
