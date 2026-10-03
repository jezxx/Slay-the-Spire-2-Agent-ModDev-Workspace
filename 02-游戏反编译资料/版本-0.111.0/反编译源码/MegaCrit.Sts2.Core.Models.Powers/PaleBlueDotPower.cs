using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class PaleBlueDotPower : PowerModel
{
	private class Data
	{
		public bool alreadyActivatedThisTurn;
	}

	public const string cardPlayThresholdKey = "CardPlay";

	public const int cardPlayThresholdValue = 5;

	public override int DisplayAmount => Math.Max(0, 5 - AttacksPlayedThisTurn);

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("CardPlay", 5m));

	private int AttacksPlayedThisTurn => CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry c) => c.HappenedThisTurn(base.Owner.CombatState) && c.CardPlay.Player == base.Owner.Player);

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner.Player)
		{
			return;
		}
		Data internalData = GetInternalData<Data>();
		if (!internalData.alreadyActivatedThisTurn)
		{
			InvokeDisplayAmountChanged();
			if (AttacksPlayedThisTurn >= 5)
			{
				internalData.alreadyActivatedThisTurn = true;
				await Cmd.Wait(0.5f);
				await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, base.Owner, base.Amount, base.Owner, null);
			}
		}
	}

	public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(base.Owner))
		{
			return Task.CompletedTask;
		}
		Data internalData = GetInternalData<Data>();
		internalData.alreadyActivatedThisTurn = false;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}
}
