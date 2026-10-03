using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves.Migrations.Shared;

namespace MegaCrit.Sts2.Core.Saves.Migrations.SerializableRuns;

/// <summary>
/// Migrates renamed ModelIds:
/// - CARD.SCARE -&gt; CARD.SIDESTEP (class Scare reworked and renamed to Sidestep)
/// </summary>
[Migration(typeof(SerializableRun), 19, 20)]
public class SerializableRunV19ToV20 : MigrationBase<SerializableRun>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		Log.Info("SerializableRun migration v19 -> v20: Migrating renamed ModelIds");
		SharedMigrationHelper.ReplaceModelIds(saveData.GetRawNode(), SharedMigrationHelper.v110Renames);
	}
}
