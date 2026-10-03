using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class HeftyTablet : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(3));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Injury>();

	public override async Task AfterObtained()
	{
		CardCreationOptions options2 = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(base.Owner.Character.CardPool), CardCreationSource.Other, CardRarityOddsType.Uniform, (CardModel c) => c.Rarity == CardRarity.Rare).WithFlags(CardCreationFlags.NoUpgradeRoll);
		List<CardModel> options = (from r in CardFactory.CreateForReward(base.Owner, base.DynamicVars.Cards.IntValue, options2)
			select r.Card).ToList();
		CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), options, base.Owner, canSkip: true);
		List<CardModel> list = new List<CardModel>(1) { base.Owner.RunState.CreateCard<Injury>(base.Owner) };
		if (chosenCard != null)
		{
			list.Insert(0, chosenCard);
		}
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(list, PileType.Deck));
		foreach (CardModel item in options)
		{
			if (item != chosenCard)
			{
				base.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(base.Owner.NetId).CardChoices.Add(new CardChoiceHistoryEntry(item, wasPicked: false));
			}
		}
	}
}
