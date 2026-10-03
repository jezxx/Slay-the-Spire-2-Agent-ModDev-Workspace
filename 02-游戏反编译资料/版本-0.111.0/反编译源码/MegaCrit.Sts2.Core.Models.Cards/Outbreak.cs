using System.Collections.Generic;
using System.Threading.Tasks;
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

public sealed class Outbreak : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<PoisonPower>());

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<PoisonPower>(9m));

	public Outbreak()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
		{
			NPoisonImpactVfx child = NPoisonImpactVfx.Create(hittableEnemy);
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(child);
			await PowerCmd.Apply<PoisonPower>(choiceContext, hittableEnemy, base.DynamicVars.Poison.BaseValue, base.Owner.Creature, this);
		}
		foreach (Creature hittableEnemy2 in base.CombatState.HittableEnemies)
		{
			PoisonPower power = hittableEnemy2.GetPower<PoisonPower>();
			if (power != null)
			{
				await power.Trigger();
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Poison.UpgradeValueBy(3m);
	}
}
