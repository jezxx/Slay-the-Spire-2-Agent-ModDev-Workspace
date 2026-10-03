using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.RelicPools;

public sealed class RegentRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "regent";

	public override Color LabOutlineColor => StsColors.orange;

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		return new global::_003C_003Ez__ReadOnlyArray<RelicModel>(new RelicModel[8]
		{
			ModelDb.Relic<DivineRight>(),
			ModelDb.Relic<FencingManual>(),
			ModelDb.Relic<GalacticDust>(),
			ModelDb.Relic<LunarPastry>(),
			ModelDb.Relic<MiniRegent>(),
			ModelDb.Relic<OrangeDough>(),
			ModelDb.Relic<Regalite>(),
			ModelDb.Relic<VitruvianMinion>()
		});
	}

	public override IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
	{
		List<RelicModel> list = base.AllRelics.ToList();
		if (!unlockState.IsEpochRevealed<Regent3Epoch>())
		{
			list.RemoveAll(delegate(RelicModel r)
			{
				RelicModel r3 = r;
				return Regent3Epoch.Relics.Any((RelicModel relic) => relic.Id == r3.Id);
			});
		}
		if (!unlockState.IsEpochRevealed<Regent6Epoch>())
		{
			list.RemoveAll(delegate(RelicModel r)
			{
				RelicModel r2 = r;
				return Regent6Epoch.Relics.Any((RelicModel relic) => relic.Id == r2.Id);
			});
		}
		return list;
	}
}
