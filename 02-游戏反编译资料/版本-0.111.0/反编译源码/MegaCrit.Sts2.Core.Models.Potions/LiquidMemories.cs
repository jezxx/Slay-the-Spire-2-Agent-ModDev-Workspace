using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class LiquidMemories : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.AnyPlayer;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		Player player = target.Player;
		NCombatRoom.Instance?.PlaySplashVfx(target, new Color(Colors.Blue));
		CardModel cardModel = (await CardSelectCmd.FromCombatPile(choiceContext, PileType.Discard.GetPile(player), player, new CardSelectorPrefs(base.SelectionScreenPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			cardModel.SetToFreeThisTurn();
			await CardPileCmd.Add(cardModel, PileType.Hand);
		}
	}
}
