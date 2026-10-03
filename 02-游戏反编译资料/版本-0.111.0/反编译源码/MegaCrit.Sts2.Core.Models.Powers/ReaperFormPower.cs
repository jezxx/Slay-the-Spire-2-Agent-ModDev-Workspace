using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx.Forms;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ReaperFormPower : PowerModel
{
	private NReaperFormVfx? _vfx;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DoomPower>());

	private NReaperFormVfx? Vfx
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
		Vfx = NReaperFormVfx.Create(base.Owner);
		return Task.CompletedTask;
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		Vfx?.SetActive(isActive: false);
		return Task.CompletedTask;
	}

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (dealer != null && (dealer == base.Owner || dealer.PetOwner?.Creature == base.Owner) && props.IsPoweredAttack() && result.TotalDamage > 0)
		{
			Vfx?.OnEffectTriggered();
			await PowerCmd.Apply<DoomPower>(choiceContext, target, result.TotalDamage * base.Amount, base.Owner, null);
		}
	}
}
