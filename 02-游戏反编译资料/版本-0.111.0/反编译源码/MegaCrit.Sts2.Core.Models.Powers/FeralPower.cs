using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class FeralPower : PowerModel
{
	private class Data
	{
		public int zeroCostAttacksPlayed;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => Math.Max(0, base.Amount - GetInternalData<Data>().zeroCostAttacksPlayed);

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		SetZeroCostAttacksPlayed(CombatManager.Instance.History.Entries.OfType<CardPlayStartedEntry>().Count((CardPlayStartedEntry e) => e.CardPlay.Card.Type == CardType.Attack && e.CardPlay.Player == base.Owner.Player && e.CardPlay.Resources.EnergyValue == 0 && e.HappenedThisTurn(base.CombatState)));
		return Task.CompletedTask;
	}

	public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation location)
	{
		if (card.Owner.Creature != base.Owner)
		{
			return location;
		}
		if (card.Type != CardType.Attack)
		{
			return location;
		}
		if (resources.EnergyValue > 0)
		{
			return location;
		}
		if (card.IsDupe)
		{
			return location;
		}
		if (GetInternalData<Data>().zeroCostAttacksPlayed >= base.Amount)
		{
			return location;
		}
		location.pileType = PileType.Hand;
		location.position = CardPilePosition.Top;
		location.player = base.Owner.Player;
		return location;
	}

	public override Task AfterModifyingCardPlayResultLocation(CardModel card, CardLocation location)
	{
		Flash();
		SetZeroCostAttacksPlayed(GetInternalData<Data>().zeroCostAttacksPlayed + 1);
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner))
		{
			return Task.CompletedTask;
		}
		SetZeroCostAttacksPlayed(0);
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	private void SetZeroCostAttacksPlayed(int value)
	{
		GetInternalData<Data>().zeroCostAttacksPlayed = value;
		InvokeDisplayAmountChanged();
	}
}
