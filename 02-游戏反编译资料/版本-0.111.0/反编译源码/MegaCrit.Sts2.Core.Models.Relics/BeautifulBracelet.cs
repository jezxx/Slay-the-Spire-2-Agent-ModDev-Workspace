using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BeautifulBracelet : RelicModel
{
	private const string _swiftKey = "Swift";

	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(4),
		new DynamicVar("Swift", 2m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Swift>(base.DynamicVars["Swift"].IntValue);

	public override Task AfterObtained()
	{
		EnchantmentModel swift = ModelDb.Enchantment<Swift>();
		List<CardModel> list = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => swift.CanEnchant(c)).TakeRandom(base.DynamicVars.Cards.IntValue, base.Owner.RunState.Rng.Niche).ToList();
		foreach (CardModel item in list)
		{
			CardCmd.Enchant<Swift>(item, base.DynamicVars["Swift"].IntValue);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(item);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
		return Task.CompletedTask;
	}
}
