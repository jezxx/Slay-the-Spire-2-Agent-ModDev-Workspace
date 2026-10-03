using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace MegaCrit.Sts2.Core.Models.Characters;

public sealed class Deprived : CharacterModel
{
	private MockCardPool? _mockCardPool;

	public override bool IsMock => true;

	public override bool IsPlayable => false;

	public override Color NameColor => StsColors.gold;

	public override CharacterGender Gender => CharacterGender.Neutral;

	protected override CharacterModel? UnlocksAfterRunAs => null;

	public override int StartingHp => 1000;

	public override int StartingGold => 99;

	public override int MaxEnergy => 100;

	public override CardPoolModel CardPool
	{
		get
		{
			if (_mockCardPool != null)
			{
				return _mockCardPool;
			}
			return ModelDb.CardPool<DeprivedCardPool>();
		}
	}

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<IroncladRelicPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<IroncladPotionPool>();

	public override IEnumerable<CardModel> StartingDeck => Array.Empty<CardModel>();

	public override IReadOnlyList<RelicModel> StartingRelics => Array.Empty<RelicModel>();

	public override float AttackAnimDelay => 0f;

	public override float CastAnimDelay => 0f;

	public override Color MapDrawingColor => new Color("462996");

	public override List<string> GetArchitectAttackVfx()
	{
		return new List<string>();
	}

	public void ResetMockCardPool()
	{
		_mockCardPool = null;
	}

	public void SetMockCardPool(IEnumerable<CardModel> cards)
	{
		if (_mockCardPool != null)
		{
			throw new InvalidOperationException("Mock card pool already initialized");
		}
		_mockCardPool = (MockCardPool)ModelDb.CardPool<MockCardPool>().ToMutable();
		foreach (CardModel card in cards)
		{
			_mockCardPool.Add(card);
		}
	}
}
