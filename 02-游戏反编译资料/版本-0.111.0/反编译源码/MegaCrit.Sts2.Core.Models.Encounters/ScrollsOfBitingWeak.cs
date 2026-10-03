using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class ScrollsOfBitingWeak : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override bool IsWeak => true;

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Scrolls);

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<ScrollOfBiting>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		ScrollOfBiting scrollOfBiting = (ScrollOfBiting)ModelDb.Monster<ScrollOfBiting>().ToMutable();
		ScrollOfBiting scrollOfBiting2 = (ScrollOfBiting)ModelDb.Monster<ScrollOfBiting>().ToMutable();
		ScrollOfBiting scrollOfBiting3 = (ScrollOfBiting)ModelDb.Monster<ScrollOfBiting>().ToMutable();
		int num2 = (scrollOfBiting.StarterMoveIdx = base.Rng.NextInt(3));
		scrollOfBiting2.StarterMoveIdx = (num2 + 1) % 3;
		scrollOfBiting3.StarterMoveIdx = (num2 + 2) % 3;
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(scrollOfBiting, null),
			(scrollOfBiting2, null),
			(scrollOfBiting3, null)
		});
	}
}
