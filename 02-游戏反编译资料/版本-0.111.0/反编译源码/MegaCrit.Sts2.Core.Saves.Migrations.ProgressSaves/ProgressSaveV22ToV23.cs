using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves.Migrations.Shared;

namespace MegaCrit.Sts2.Core.Saves.Migrations.ProgressSaves;

/// <summary>
/// Migrates renamed ModelIds in progress saves.
/// Same renames as SerializableRunV19ToV20, applied to the SerializableProgress schema so that
/// discovered_cards and card_stats keep resolving to a live model after a card rename.
/// </summary>
[Migration(typeof(SerializableProgress), 22, 23)]
public class ProgressSaveV22ToV23 : MigrationBase<SerializableProgress>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		Log.Info("Progress save migration v22 -> v23: Migrating renamed ModelIds");
		SharedMigrationHelper.ReplaceModelIds(saveData.GetRawNode(), SharedMigrationHelper.v110Renames);
	}
}
