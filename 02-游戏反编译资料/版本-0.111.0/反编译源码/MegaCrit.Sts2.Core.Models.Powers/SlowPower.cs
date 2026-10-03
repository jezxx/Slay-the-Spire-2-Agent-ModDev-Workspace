using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SlowPower : PowerModel
{
	private const string _slowAmountKey = "SlowAmount";

	private const string _displayAmountKey = "DisplayAmount";

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => base.DynamicVars["SlowAmount"].IntValue * 10;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("SlowAmount", 0m),
		new DynamicVar("DisplayAmount", 0m)
	});

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		base.DynamicVars["SlowAmount"].BaseValue++;
		base.DynamicVars["DisplayAmount"].BaseValue = base.DynamicVars["SlowAmount"].BaseValue * 10m;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
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
		return 1m + 0.1m * base.DynamicVars["SlowAmount"].BaseValue;
	}

	public override Task AfterModifyingDamageAmount(CardModel? cardSource)
	{
		Flash();
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner))
		{
			return Task.CompletedTask;
		}
		base.DynamicVars["SlowAmount"].BaseValue = 0m;
		base.DynamicVars["DisplayAmount"].BaseValue = 0m;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}
}
