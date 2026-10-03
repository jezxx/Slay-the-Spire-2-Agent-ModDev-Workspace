using System;
using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class MockCardPool : CardPoolModel
{
	private List<CardModel>? _customCards;

	public override bool IsMock => true;

	public override string Title => "test";

	public override string EnergyColorName => "colorless";

	public override string CardFrameMaterialPath => "card_frame_colorless";

	public override Color DeckEntryCardColor => Colors.White;

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return _customCards?.ToArray() ?? Array.Empty<CardModel>();
	}

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_customCards = new List<CardModel>();
	}

	public void Add(CardModel card)
	{
		AssertMutable();
		card.AssertCanonical();
		_customCards.Add(card);
		InvalidateCardCache();
	}

	public static MockCardPool Create(params CardModel[] cards)
	{
		MockCardPool mockCardPool = (MockCardPool)ModelDb.CardPool<MockCardPool>().ToMutable();
		foreach (CardModel card in cards)
		{
			mockCardPool.Add(card);
		}
		return mockCardPool;
	}
}
