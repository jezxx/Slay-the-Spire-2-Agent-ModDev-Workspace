using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx.Forms;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SerpentFormPower : PowerModel
{
	private class Data
	{
		/// <summary>
		/// Keep track of the cards we've seen played and the power amount at the time they were played.
		/// This lets Serpent Form avoid triggering on cards that started play before it was applied, and avoid
		/// dealing extra damage on multiple plays of Serpent Form.
		/// </summary>
		public readonly Dictionary<CardModel, int> amountsForPlayedCards = new Dictionary<CardModel, int>();
	}

	private NSerpentFormVfx? _vfx;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	private NSerpentFormVfx? Vfx
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

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		Vfx = NSerpentFormVfx.Create(base.Owner);
		return Task.CompletedTask;
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		Vfx?.SetActive(isActive: false);
		return Task.CompletedTask;
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner.Player)
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().amountsForPlayedCards.Add(cardPlay.Card, base.Amount);
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner.Player && GetInternalData<Data>().amountsForPlayedCards.Remove(cardPlay.Card, out var damage) && damage > 0)
		{
			await Cmd.CustomScaledWait(0.1f, 0.2f);
			Creature creature = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(base.Owner.CombatState?.HittableEnemies ?? Array.Empty<Creature>());
			if (creature != null)
			{
				Vfx?.OnEffectTriggered();
				VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_attack_blunt");
				await CreatureCmd.Damage(choiceContext, creature, damage, ValueProp.Unpowered, base.Owner);
			}
		}
	}
}
