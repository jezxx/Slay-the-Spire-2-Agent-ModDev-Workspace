using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class EntropicBrew : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;

	public override PotionUsage Usage => PotionUsage.AnyTime;

	public override TargetType TargetType => TargetType.AnyPlayer;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		Player targetPlayer = target.Player;
		while (targetPlayer.HasOpenPotionSlots)
		{
			PotionModel potion = PotionFactory.CreateRandomPotionOutOfCombat(targetPlayer, targetPlayer.RunState.Rng.CombatPotionGeneration).ToMutable();
			if (!(await PotionCmd.TryToProcure(potion, targetPlayer)).success)
			{
				break;
			}
		}
	}
}
