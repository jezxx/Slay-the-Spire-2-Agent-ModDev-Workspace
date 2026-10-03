using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LastingCandy : RelicModel
{
	private bool _isActivating;

	private int _combatRewardsSeen;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return CombatRewardsSeen % 2;
			}
			return 2;
		}
	}

	private bool IsActivating
	{
		get
		{
			return _isActivating;
		}
		set
		{
			AssertMutable();
			_isActivating = value;
			InvokeDisplayAmountChanged();
		}
	}

	[SavedProperty]
	public int CombatRewardsSeen
	{
		get
		{
			return _combatRewardsSeen;
		}
		set
		{
			AssertMutable();
			_combatRewardsSeen = value;
		}
	}

	private bool IsInTriggeringCombat
	{
		get
		{
			if (CombatRewardsSeen > 0)
			{
				return CombatRewardsSeen % 2 == 1;
			}
			return false;
		}
	}

	public override bool IsAllowed(IRunState runState)
	{
		if (runState.Players.Any(delegate(Player p)
		{
			if (p != null && p.Character is Ironclad)
			{
				UnlockState unlockState = p.UnlockState;
				if (unlockState != null)
				{
					return unlockState.NumberOfRuns == 0;
				}
			}
			return false;
		}))
		{
			return false;
		}
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

	public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> rewardOptions, CardCreationOptions creationOptions)
	{
		List<CardCreationResult> rewardOptions2 = rewardOptions;
		CardCreationOptions creationOptions2 = creationOptions;
		if (base.Owner != player)
		{
			return false;
		}
		if (creationOptions2.Source != CardCreationSource.Encounter)
		{
			return false;
		}
		if (!IsInTriggeringCombat)
		{
			return false;
		}
		if (!creationOptions2.Flags.HasFlag(CardCreationFlags.IsCardReward))
		{
			return false;
		}
		if (!creationOptions2.Flags.HasFlag(CardCreationFlags.IsFromCombat))
		{
			return false;
		}
		bool allowDupes = false;
		List<CardModel> source = creationOptions2.GetPossibleCards(player).ToList();
		IEnumerable<CardModel> source2 = source.Where((CardModel c) => CardPoolFilter(c, rewardOptions2, allowDupes: false));
		if (!source2.Any())
		{
			allowDupes = true;
			source2 = source.Where((CardModel c) => CardPoolFilter(c, rewardOptions2, allowDupes: true));
		}
		if (!source2.Any())
		{
			return false;
		}
		CardCreationOptions options = new CardCreationOptions(creationOptions2.CardPools, CardCreationSource.Other, creationOptions2.RarityOdds, delegate(CardModel c)
		{
			Func<CardModel, bool>? cardPoolFilter = creationOptions2.CardPoolFilter;
			return (cardPoolFilter == null || cardPoolFilter(c)) && CardPoolFilter(c, rewardOptions2, allowDupes);
		}).WithFlags(CardCreationFlags.NoModifyHooks | CardCreationFlags.NoCardPoolModifications);
		CardModel cardModel = CardFactory.CreateForReward(base.Owner, 1, options).FirstOrDefault()?.Card;
		if (cardModel != null)
		{
			CardCreationResult cardCreationResult = new CardCreationResult(cardModel);
			cardCreationResult.ModifyCard(cardModel, this);
			rewardOptions2.Add(cardCreationResult);
		}
		return cardModel != null;
	}

	public override Task BeforeCombatRewardOffered(RewardsSet rewards, CombatRoom room)
	{
		if (rewards.Player != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (rewards.Rewards.All((Reward r) => !(r is CardReward)))
		{
			return Task.CompletedTask;
		}
		if (IsInTriggeringCombat)
		{
			TaskHelper.RunSafely(DoActivateVisuals());
		}
		CombatRewardsSeen++;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}

	private static bool CardPoolFilter(CardModel card, List<CardCreationResult> rewardOptions, bool allowDupes)
	{
		CardModel card2 = card;
		if (card2.Type == CardType.Power)
		{
			if (!allowDupes)
			{
				return rewardOptions.TrueForAll((CardCreationResult o) => o.originalCard.Id != card2.Id);
			}
			return true;
		}
		return false;
	}
}
