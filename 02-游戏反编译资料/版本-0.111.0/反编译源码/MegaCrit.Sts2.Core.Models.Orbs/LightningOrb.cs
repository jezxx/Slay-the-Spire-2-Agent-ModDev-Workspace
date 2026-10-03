using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Orbs;

public class LightningOrb : OrbModel
{
	protected override string PassiveSfx => "event:/sfx/characters/defect/defect_lightning_passive";

	protected override string EvokeSfx => "event:/sfx/characters/defect/defect_lightning_evoke";

	protected override string ChannelSfx => "event:/sfx/characters/defect/defect_lightning_channel";

	public override Color DarkenedColor => new Color("796606");

	public override decimal PassiveVal => ModifyOrbValue(3m);

	public override decimal EvokeVal => ModifyOrbValue(8m);

	public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
	{
		await TriggerPassive(choiceContext, null);
	}

	public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
	{
		ActivatePassive();
		await ApplyLightningDamage(PassiveVal, target, choiceContext, isEvoke: false);
	}

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
	{
		return await ApplyLightningDamage(EvokeVal, null, playerChoiceContext, isEvoke: true);
	}

	private async Task<IEnumerable<Creature>> ApplyLightningDamage(decimal value, Creature? target, PlayerChoiceContext choiceContext, bool isEvoke)
	{
		List<Creature> list = (from e in base.CombatState.GetOpponentsOf(base.Owner.Creature)
			where e.IsHittable
			select e).ToList();
		if (list.Count == 0)
		{
			return Array.Empty<Creature>();
		}
		IReadOnlyList<Creature> targets = ((target == null) ? new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(base.Owner.RunState.Rng.CombatTargets.NextItem(list)) : new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(target));
		if (isEvoke)
		{
			ActivateEvoke(targets.ToArray());
		}
		foreach (Creature item in targets)
		{
			VfxCmd.PlayOnCreature(item, "vfx/vfx_attack_lightning");
		}
		PlayEvokeSfx();
		await CreatureCmd.Damage(choiceContext, targets, value, ValueProp.Unpowered, base.Owner.Creature);
		return targets;
	}
}
