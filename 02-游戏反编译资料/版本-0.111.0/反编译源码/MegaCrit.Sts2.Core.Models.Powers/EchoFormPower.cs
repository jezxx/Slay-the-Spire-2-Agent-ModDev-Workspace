using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx.Forms;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class EchoFormPower : PowerModel
{
	private NEchoFormVfx? _vfx;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	private NEchoFormVfx? Vfx
	{
		get
		{
			if (_vfx == null)
			{
				return _vfx;
			}
			if (!_vfx.IsValid())
			{
				return null;
			}
			return _vfx;
		}
		set
		{
			AssertMutable();
			_vfx = value;
		}
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		Vfx = NEchoFormVfx.Create(base.Owner);
		return Task.CompletedTask;
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		Vfx?.SetActive(isActive: false);
		return Task.CompletedTask;
	}

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (card.Owner.Creature != base.Owner)
		{
			return playCount;
		}
		int num = CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry e) => e.Actor == base.Owner && e.CardPlay.IsFirstInSeries && e.HappenedThisTurn(base.CombatState));
		if (num >= base.Amount)
		{
			return playCount;
		}
		return playCount + 1;
	}

	public override Task AfterModifyingCardPlayCount(CardModel card)
	{
		Flash();
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int num = CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry e) => e.Actor == base.Owner && e.CardPlay.IsFirstInSeries && e.HappenedThisTurn(base.CombatState));
		if (num >= base.Amount)
		{
			Vfx?.SetActive(isActive: false);
		}
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner))
		{
			return Task.CompletedTask;
		}
		Vfx?.SetActive(isActive: true);
		return Task.CompletedTask;
	}
}
