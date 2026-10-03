using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class BattlewornDummyEventV3Encounter : BattlewornDummyEventEncounter
{
	private const string _settingKey = "Setting";

	public override RoomType RoomType => RoomType.Monster;

	public override bool ShouldGiveRewards => false;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<BattleFriendV3>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<BattleFriendV3>().ToMutable(), null));
	}
}
