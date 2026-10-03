using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class DeprivedCardPool : CardPoolModel
{
	public override bool IsMock => true;

	public override string Title => "test";

	public override string EnergyColorName => "colorless";

	public override string CardFrameMaterialPath => "card_frame_colorless";

	public override Color DeckEntryCardColor => Colors.White;

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[13]
		{
			MockCard<MockAttackCard>(CardRarity.Common),
			MockCard<MockAttackCard>(CardRarity.Uncommon),
			MockCard<MockAttackCard>(CardRarity.Rare),
			MockCard<MockPowerCard>(CardRarity.Common),
			MockCard<MockPowerCard>(CardRarity.Uncommon),
			MockCard<MockPowerCard>(CardRarity.Rare),
			MockCard<MockSkillCard>(CardRarity.Common),
			MockCard<MockSkillCard>(CardRarity.Uncommon),
			MockCard<MockSkillCard>(CardRarity.Rare),
			MockCard<MockQuestCard>(CardRarity.Quest),
			MockCard<MockCurseCard>(CardRarity.Curse),
			MockCard<MockStatusCard>(CardRarity.Status),
			MockCard<MockTurnEndInHandRecorderCard>(CardRarity.Common)
		};
	}

	private static MockCardModel MockCard<T>(CardRarity rarity) where T : MockCardModel
	{
		return ((MockCardModel)ModelDb.Card<T>().ToMutable()).MockRarity(rarity).MockCanonical();
	}
}
