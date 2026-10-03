using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class TankPower : PowerModel
{
	private const string _damageIncreaseKey = "DamageIncrease";

	private const string _damageDecreaseKey = "DamageDecrease";

	public const decimal damageIncrease = 1.5m;

	public const decimal damageDecrease = 0.5m;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("DamageIncrease", 1.5m),
		new DynamicVar("DamageDecrease", 0.5m)
	});

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		foreach (Creature item in base.CombatState.GetTeammatesOf(base.Owner))
		{
			if (item.IsAlive && item.IsPlayer && item != base.Owner)
			{
				await PowerCmd.Apply<GuardedPower>(new ThrowingPlayerChoiceContext(), item, base.Amount, base.Owner, null);
			}
		}
	}

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
		return base.DynamicVars["DamageIncrease"].BaseValue;
	}
}
