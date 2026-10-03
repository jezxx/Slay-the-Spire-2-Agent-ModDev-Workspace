using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class OrobicAcid : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.AnyPlayer;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		Player player = target.Player;
		List<CardModel> list = new List<CardModel>();
		list.AddRange(CardFactory.GetDistinctForCombat(player, from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
			where c.Type == CardType.Attack
			select c, 1, player.RunState.Rng.CombatCardGeneration));
		list.AddRange(CardFactory.GetDistinctForCombat(player, from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
			where c.Type == CardType.Skill
			select c, 1, player.RunState.Rng.CombatCardGeneration));
		list.AddRange(CardFactory.GetDistinctForCombat(player, from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
			where c.Type == CardType.Power
			select c, 1, player.RunState.Rng.CombatCardGeneration));
		foreach (CardModel item in list)
		{
			item.SetToFreeThisTurn();
		}
		await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Hand, base.Owner);
	}
}
