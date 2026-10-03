using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class OneForAllPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		if (cardSource == null)
		{
			return 0m;
		}
		if (cardSource.Owner.Creature != base.Owner)
		{
			return 0m;
		}
		if (cardPlay?.Card.EnergyCost.CostsX ?? cardSource.EnergyCost.CostsX)
		{
			return 0m;
		}
		if (cardPlay != null && cardPlay.Resources.EnergySpent != 0)
		{
			return 0m;
		}
		if (cardPlay == null && cardSource.EnergyCost.GetWithModifiers(CostModifiers.All) != 0)
		{
			return 0m;
		}
		return base.Amount;
	}
}
