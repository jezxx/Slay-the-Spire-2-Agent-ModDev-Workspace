using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Haze : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<PoisonPower>(4m),
		new PowerVar<WeakPower>(1m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<PoisonPower>(),
		HoverTipFactory.FromPower<WeakPower>()
	});

	public Haze()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		SpawnVfx();
		await Cmd.CustomScaledWait(0.2f, 0.4f);
		await PowerCmd.Apply<PoisonPower>(choiceContext, base.CombatState?.HittableEnemies, base.DynamicVars.Poison.BaseValue, base.Owner.Creature, this);
		await PowerCmd.Apply<WeakPower>(choiceContext, base.CombatState?.HittableEnemies, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
	}

	private void SpawnVfx()
	{
		Node node = NCombatRoom.Instance?.CombatVfxContainer;
		if (node == null)
		{
			return;
		}
		NSmokyVignetteVfx child = NSmokyVignetteVfx.Create(new Color(0.8f, 0.8f, 0.3f, 0.66f), new Color(0f, 4f, 0f, 0.33f));
		node.AddChildSafely(child);
		foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
		{
			node.AddChildSafely(NSmokePuffVfx.Create(hittableEnemy, NSmokePuffVfx.SmokePuffColor.Green));
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Poison.UpgradeValueBy(2m);
		base.DynamicVars.Weak.UpgradeValueBy(1m);
	}
}
