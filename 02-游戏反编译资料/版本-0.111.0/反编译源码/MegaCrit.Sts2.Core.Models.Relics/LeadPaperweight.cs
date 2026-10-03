using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LeadPaperweight : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override async Task AfterObtained()
	{
		CardCreationOptions options2 = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(ModelDb.CardPool<ColorlessCardPool>()), CardCreationSource.Other, CardRarityOddsType.RegularEncounter);
		List<CardModel> options = (from c in CardFactory.CreateForReward(base.Owner, 2, options2)
			select c.Card).ToList();
		CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), options, base.Owner, canSkip: true);
		if (chosenCard != null)
		{
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(chosenCard, PileType.Deck));
		}
		foreach (CardModel item in options)
		{
			if (item != chosenCard)
			{
				base.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(base.Owner.NetId).CardChoices.Add(new CardChoiceHistoryEntry(item, wasPicked: false));
			}
		}
	}
}
