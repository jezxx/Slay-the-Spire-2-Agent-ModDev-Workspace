using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class BigGameHunter : ModifierModel
{
	public override ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
	{
		Rng mapRng = new Rng(runState.Rng.Seed, $"act_{runState.CurrentActIndex + 1}_map");
		MapPointTypeCounts mapPointTypeCountsOverride = new MapPointTypeCounts(map)
		{
			NumOfElites = (int)Math.Round((float)map.GetAllMapPoints().Count((MapPoint p) => p.PointType == MapPointType.Elite) * 2.5f),
			PointTypesThatIgnoreRules = new HashSet<MapPointType> { MapPointType.Elite }
		};
		if (map is StandardActMap)
		{
			bool shouldReplaceTreasureWithElites = map is StandardActMap standardActMap && standardActMap.ShouldReplaceTreasureWithElites;
			return new StandardActMap(mapRng, runState.Act, runState.Players.Count > 1, shouldReplaceTreasureWithElites, runState.Act.HasSecondBoss, mapPointTypeCountsOverride);
		}
		if (map is SpoilsActMap)
		{
			return new SpoilsActMap(runState, mapPointTypeCountsOverride);
		}
		return map;
	}

	public override CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
	{
		if (options.Source != CardCreationSource.Encounter)
		{
			return options;
		}
		if (options.RarityOdds != CardRarityOddsType.EliteEncounter)
		{
			return options;
		}
		if (options.Flags.HasFlag(CardCreationFlags.NoCardPoolModifications))
		{
			return options;
		}
		if (options.Flags.HasFlag(CardCreationFlags.NoRarityModification))
		{
			return options;
		}
		options = options.WithRarityOdds(CardRarityOddsType.Uniform).WithFilter((CardModel c) => c.Rarity == CardRarity.Rare);
		List<CardModel> list = options.GetPossibleCards(player).ToList();
		if (list.Count <= 0)
		{
			options = options.WithCardPools(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(player.Character.CardPool));
		}
		return options;
	}
}
