using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class CacophonyPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => base.DynamicVars.Cards.IntValue;

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(33));

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		base.DynamicVars.Cards.BaseValue--;
		InvokeDisplayAmountChanged();
		if (base.DynamicVars.Cards.IntValue <= 0)
		{
			Creature enemy = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(base.CombatState.HittableEnemies);
			base.DynamicVars.Cards.BaseValue = 33m;
			InvokeDisplayAmountChanged();
			await Cmd.Wait(0.5f);
			if (enemy != null)
			{
				await CreatureCmd.Damage(choiceContext, enemy, base.Amount, ValueProp.Unpowered, base.Owner);
			}
		}
	}
}
