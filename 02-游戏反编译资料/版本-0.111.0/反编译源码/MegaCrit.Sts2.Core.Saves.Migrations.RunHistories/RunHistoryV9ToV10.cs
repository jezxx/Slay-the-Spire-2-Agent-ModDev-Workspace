using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Migrations.Shared;

namespace MegaCrit.Sts2.Core.Saves.Migrations.RunHistories;

/// <summary>
/// Migrates renamed ModelIds in completed run history saves.
/// Same renames as SerializableRunV19ToV20, applied to the RunHistory schema.
/// </summary>
[Migration(typeof(RunHistory), 9, 10)]
public class RunHistoryV9ToV10 : MigrationBase<RunHistory>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		Log.Info("RunHistory migration v9 -> v10: Migrating renamed ModelIds");
		SharedMigrationHelper.ReplaceModelIds(saveData.GetRawNode(), SharedMigrationHelper.v110Renames);
	}
}
