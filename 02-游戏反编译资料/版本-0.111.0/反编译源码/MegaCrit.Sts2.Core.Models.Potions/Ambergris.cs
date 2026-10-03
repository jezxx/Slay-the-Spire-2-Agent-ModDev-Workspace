using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class Ambergris : PotionModel
{
	private const string _healPercentKey = "HealPercent";

	public override PotionRarity Rarity => PotionRarity.Event;

	public override PotionUsage Usage => PotionUsage.AnyTime;

	public override TargetType TargetType => TargetType.AnyPlayer;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("HealPercent", 50m));

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		NCombatRoom.Instance?.PlaySplashVfx(target, Colors.Red);
		await CreatureCmd.Heal(target, (decimal)target.MaxHp * base.DynamicVars["HealPercent"].BaseValue / 100m);
		if (CombatManager.Instance.IsInProgress)
		{
			await PowerCmd.Apply<AmbergrisPower>(choiceContext, target, 1m, base.Owner.Creature, null);
		}
	}
}
