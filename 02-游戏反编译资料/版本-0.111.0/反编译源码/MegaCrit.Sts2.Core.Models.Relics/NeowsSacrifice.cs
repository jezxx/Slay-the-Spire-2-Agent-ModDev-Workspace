using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class NeowsSacrifice : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			List<IHoverTip> list = new List<IHoverTip>();
			list.Add(HoverTipFactory.FromPotion<Ambergris>());
			list.AddRange(HoverTipFactory.FromCardWithCardHoverTips<Guilty>());
			return new _003C_003Ez__ReadOnlyList<IHoverTip>(list);
		}
	}

	public override bool HasUponPickupEffect => true;

	public override async Task AfterObtained()
	{
		await PotionCmd.TryToProcure<Ambergris>(base.Owner);
		CardModel card = base.Owner.RunState.CreateCard<Guilty>(base.Owner);
		CardCmd.PreviewCardPileAdd(new global::_003C_003Ez__ReadOnlySingleElementList<CardPileAddResult>(await CardPileCmd.Add(card, PileType.Deck)), 2f);
	}
}
