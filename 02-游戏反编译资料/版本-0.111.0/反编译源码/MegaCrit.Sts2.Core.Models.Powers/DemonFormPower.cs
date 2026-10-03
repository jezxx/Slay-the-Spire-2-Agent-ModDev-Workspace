using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx.Forms;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class DemonFormPower : PowerModel
{
	private NDemonFormVfx? _vfx;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	private NDemonFormVfx? Vfx
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
		Vfx = NDemonFormVfx.Create(base.Owner);
		return Task.CompletedTask;
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		Vfx?.SetActive(isActive: false);
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (participants.Contains(base.Owner))
		{
			Flash();
			Vfx?.OnEffectTriggered();
			await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Owner, base.Amount, base.Owner, null);
		}
	}
}
