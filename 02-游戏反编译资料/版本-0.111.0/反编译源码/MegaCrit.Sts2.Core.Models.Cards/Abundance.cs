using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Abundance : CardModel
{
	public override bool CanBeGeneratedInCombat => false;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public Abundance()
		: base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
	{
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		List<CardModel> list = CardFactory.GetDistinctForCombat(base.Owner, from c in base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
			where c.Type == CardType.Power
			select c, 3, base.Owner.RunState.Rng.CombatCardGeneration).ToList();
		foreach (CardModel item in list)
		{
			CardCmd.Upgrade(item);
		}
		CardModel cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, list, base.Owner);
		if (cardModel != null)
		{
			cardModel.SetToFreeThisTurn();
			await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, base.Owner);
		}
	}
}
