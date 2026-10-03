using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Commands;

public static class OstyCmd
{
	/// <summary>
	/// Summon Osty with the specified number of HP. If the specified creature already owns an instance of Osty, raise
	/// Osty's max HP by the specified number instead.
	/// </summary>
	/// <param name="choiceContext">The context with which to handle player choices.</param>
	/// <param name="summoner">The player who is summoning.</param>
	/// <param name="amount">
	/// The number of HP that Osty should be summoned with (or that should be added to the existing Osty instance).
	/// </param>
	/// <param name="source">
	/// The model that this Summon came from. For example, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Bodyguard" /> and <see cref="T:MegaCrit.Sts2.Core.Models.Relics.BoundPhylactery" />
	/// pass themselves here.
	/// Null if the Summon did not come from any model (generally only relevant in tests).
	/// </param>
	/// <returns>The result of the summon.</returns>
	public static async Task<SummonResult> Summon(PlayerChoiceContext choiceContext, Player summoner, decimal amount, AbstractModel? source)
	{
		Player summoner2 = summoner;
		ICombatState combatState = summoner2.Creature.CombatState;
		amount = Hook.ModifySummonAmount(combatState, summoner2, amount, source);
		if (amount == 0m)
		{
			return new SummonResult(summoner2.Osty, 0m);
		}
		if (CombatManager.Instance.IsInProgress)
		{
			SfxCmd.Play("event:/sfx/characters/necrobinder/necrobinder_summon");
		}
		Creature osty = combatState.Allies.FirstOrDefault((Creature c) => c.Monster is Osty && c.PetOwner == summoner2);
		if (summoner2.IsOstyAlive)
		{
			await CreatureCmd.GainMaxHp(summoner2.Osty, amount);
		}
		else
		{
			bool isReviving = osty != null;
			if (isReviving)
			{
				if (osty.IsAlive)
				{
					throw new InvalidOperationException("We shouldn't make it here if Osty is still alive!");
				}
				summoner2.PlayerCombatState.AddPetInternal(osty);
			}
			else
			{
				osty = await PlayerCmd.AddPet<Osty>(summoner2);
				NCreature ostyNode = NCombatRoom.Instance?.GetCreatureNode(osty);
				if (ostyNode != null && source is CardModel)
				{
					ostyNode.Modulate = Colors.Transparent;
					Tween tween = ostyNode.CreateTween();
					tween.TweenProperty(ostyNode, "modulate", Colors.White, 0.3499999940395355).SetDelay(0.10000000149011612);
					ostyNode.StartReviveAnim();
				}
				await PowerCmd.Apply<DieForYouPower>(choiceContext, osty, 1m, null, null);
				ostyNode?.TrackBlockStatus(summoner2.Creature);
			}
			await CreatureCmd.SetMaxHp(osty, amount);
			await CreatureCmd.Heal(osty, amount, isReviving);
			if (isReviving)
			{
				await Hook.AfterOstyRevived(combatState, osty);
			}
		}
		if (osty != null)
		{
			NCombatRoom.Instance?.GetCreatureNode(osty)?.OstyScaleToSize(osty.MaxHp, 0.75);
		}
		CombatManager.Instance.History.Summoned(combatState, (int)amount, summoner2);
		await Hook.AfterSummon(combatState, choiceContext, summoner2, amount);
		return new SummonResult(summoner2.Osty, amount);
	}
}
