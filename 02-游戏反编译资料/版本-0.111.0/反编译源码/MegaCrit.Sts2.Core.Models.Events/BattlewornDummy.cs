using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class BattlewornDummy : EventModel
{
	private const string _setting1HpKey = "Setting1Hp";

	private const string _setting2HpKey = "Setting2Hp";

	private const string _setting3HpKey = "Setting3Hp";

	public override bool IsShared => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DynamicVar("Setting1Hp", ModelDb.Monster<BattleFriendV1>().MinInitialHp),
		new DynamicVar("Setting2Hp", ModelDb.Monster<BattleFriendV2>().MinInitialHp),
		new DynamicVar("Setting3Hp", ModelDb.Monster<BattleFriendV3>().MinInitialHp)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		int playerCount = base.Owner?.RunState.Players.Count ?? 1;
		int actIndex = base.Owner?.RunState.CurrentActIndex ?? 0;
		base.DynamicVars["Setting1Hp"].BaseValue = Creature.ScaleHpForMultiplayer(ModelDb.Monster<BattleFriendV1>().MinInitialHp, ModelDb.Encounter<BattlewornDummyEventV1Encounter>(), playerCount, actIndex);
		base.DynamicVars["Setting2Hp"].BaseValue = Creature.ScaleHpForMultiplayer(ModelDb.Monster<BattleFriendV2>().MinInitialHp, ModelDb.Encounter<BattlewornDummyEventV1Encounter>(), playerCount, actIndex);
		base.DynamicVars["Setting3Hp"].BaseValue = Creature.ScaleHpForMultiplayer(ModelDb.Monster<BattleFriendV3>().MinInitialHp, ModelDb.Encounter<BattlewornDummyEventV1Encounter>(), playerCount, actIndex);
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[3]
		{
			new EventOption(this, Setting1, "BATTLEWORN_DUMMY.pages.INITIAL.options.SETTING_1"),
			new EventOption(this, Setting2, "BATTLEWORN_DUMMY.pages.INITIAL.options.SETTING_2"),
			new EventOption(this, Setting3, "BATTLEWORN_DUMMY.pages.INITIAL.options.SETTING_3")
		});
	}

	private Task Setting1()
	{
		EnterCombatWithoutExitingEvent(ModelDb.Encounter<BattlewornDummyEventV1Encounter>(), Array.Empty<Reward>(), shouldResumeAfterCombat: true);
		return Task.CompletedTask;
	}

	private Task Setting2()
	{
		EnterCombatWithoutExitingEvent(ModelDb.Encounter<BattlewornDummyEventV2Encounter>(), Array.Empty<Reward>(), shouldResumeAfterCombat: true);
		return Task.CompletedTask;
	}

	private Task Setting3()
	{
		EnterCombatWithoutExitingEvent(ModelDb.Encounter<BattlewornDummyEventV3Encounter>(), Array.Empty<Reward>(), shouldResumeAfterCombat: true);
		return Task.CompletedTask;
	}

	public override async Task Resume(AbstractRoom room)
	{
		CombatRoom combatRoom = (CombatRoom)room;
		BattlewornDummyEventEncounter battlewornDummyEventEncounter = (BattlewornDummyEventEncounter)combatRoom.Encounter;
		if (battlewornDummyEventEncounter.RanOutOfTime)
		{
			SetEventFinished(L10NLookup("BATTLEWORN_DUMMY.pages.DEFEAT.description"));
			return;
		}
		SetEventFinished(L10NLookup("BATTLEWORN_DUMMY.pages.VICTORY.description"));
		List<Reward> list = new List<Reward>();
		if (!(battlewornDummyEventEncounter is BattlewornDummyEventV1Encounter))
		{
			if (!(battlewornDummyEventEncounter is BattlewornDummyEventV2Encounter))
			{
				if (battlewornDummyEventEncounter is BattlewornDummyEventV3Encounter)
				{
					RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable();
					list.Add(new RelicReward(relic, base.Owner));
				}
			}
			else
			{
				IEnumerable<CardModel> enumerable = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => c?.IsUpgradable ?? false).ToList().StableShuffle(base.Rng)
					.Take(2);
				foreach (CardModel item in enumerable)
				{
					CardCmd.Upgrade(item);
				}
			}
		}
		else
		{
			IEnumerable<PotionModel> items = base.Owner.Character.PotionPool.GetUnlockedPotions(base.Owner.UnlockState).Concat(ModelDb.PotionPool<SharedPotionPool>().GetUnlockedPotions(base.Owner.UnlockState));
			PotionModel potionModel = base.Owner.PlayerRng.Rewards.NextItem(items);
			if (potionModel != null)
			{
				list.Add(new PotionReward(potionModel.ToMutable(), base.Owner));
			}
		}
		if (combatRoom.ExtraRewards.ContainsKey(base.Owner))
		{
			list = list.Concat(combatRoom.ExtraRewards[base.Owner]).ToList();
		}
		if (list.Count > 0)
		{
			await RewardsCmd.OfferCustom(base.Owner, list);
		}
	}
}
