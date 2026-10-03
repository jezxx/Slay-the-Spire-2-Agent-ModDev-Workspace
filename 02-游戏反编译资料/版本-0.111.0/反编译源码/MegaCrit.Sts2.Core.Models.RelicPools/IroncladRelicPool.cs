using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.RelicPools;

public sealed class IroncladRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "ironclad";

	public override Color LabOutlineColor => StsColors.red;

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		return new global::_003C_003Ez__ReadOnlyArray<RelicModel>(new RelicModel[8]
		{
			ModelDb.Relic<Brimstone>(),
			ModelDb.Relic<BurningBlood>(),
			ModelDb.Relic<CharonsAshes>(),
			ModelDb.Relic<DemonTongue>(),
			ModelDb.Relic<PaperPhrog>(),
			ModelDb.Relic<RedSkull>(),
			ModelDb.Relic<RuinedHelmet>(),
			ModelDb.Relic<SelfFormingClay>()
		});
	}

	public override IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
	{
		List<RelicModel> list = base.AllRelics.ToList();
		if (!unlockState.IsEpochRevealed<Ironclad3Epoch>())
		{
			list.RemoveAll(delegate(RelicModel r)
			{
				RelicModel r3 = r;
				return Ironclad3Epoch.Relics.Any((RelicModel relic) => relic.Id == r3.Id);
			});
		}
		if (!unlockState.IsEpochRevealed<Ironclad6Epoch>())
		{
			list.RemoveAll(delegate(RelicModel r)
			{
				RelicModel r2 = r;
				return Ironclad6Epoch.Relics.Any((RelicModel relic) => relic.Id == r2.Id);
			});
		}
		return list;
	}
}
