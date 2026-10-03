using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class VulnerablePower : PowerModel
{
	private const string _damageIncrease = "DamageIncrease";

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("DamageIncrease", 1.5m));

	/// <summary>
	/// Multiplies damage received by 1.5x.
	/// </summary>
	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (target != base.Owner)
		{
			return 1m;
		}
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		decimal num = base.DynamicVars["DamageIncrease"].BaseValue;
		if (dealer != null)
		{
			PaperPhrog paperPhrog = dealer.Player?.GetRelic<PaperPhrog>();
			if (paperPhrog != null)
			{
				num = paperPhrog.ModifyVulnerableMultiplier(target, num, props, dealer, cardSource);
			}
			CrueltyPower crueltyPower = dealer.GetPower<CrueltyPower>() ?? dealer.PetOwner?.Creature.GetPower<CrueltyPower>();
			if (crueltyPower != null)
			{
				num = crueltyPower.ModifyVulnerableMultiplier(target, num, props, dealer, cardSource);
			}
		}
		DebilitatePower power = target.GetPower<DebilitatePower>();
		if (power != null)
		{
			num = power.ModifyVulnerableMultiplier(target, num, props, dealer, cardSource);
		}
		return num;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side == CombatSide.Enemy)
		{
			await PowerCmd.TickDownDuration(this);
		}
	}
}
