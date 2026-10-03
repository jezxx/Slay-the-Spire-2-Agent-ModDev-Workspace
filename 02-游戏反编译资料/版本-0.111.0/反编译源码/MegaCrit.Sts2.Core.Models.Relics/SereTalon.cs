using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class SereTalon : RelicModel
{
	public const int maxHpLoss = 9;

	private const string _wishesKey = "Wishes";

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new HpLossVar(9m),
		new DynamicVar("Wishes", 3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Wish>();

	public override async Task AfterObtained()
	{
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.HpLoss.BaseValue, isFromCard: false);
		List<CardPileAddResult> wishResults = new List<CardPileAddResult>();
		for (int i = 0; i < base.DynamicVars["Wishes"].IntValue; i++)
		{
			CardModel card = base.Owner.RunState.CreateCard(ModelDb.Card<Wish>(), base.Owner);
			wishResults.Add(await CardPileCmd.Add(card, PileType.Deck));
		}
		CardCmd.PreviewCardPileAdd(wishResults, 2f);
		await Cmd.Wait(0.75f);
	}
}
