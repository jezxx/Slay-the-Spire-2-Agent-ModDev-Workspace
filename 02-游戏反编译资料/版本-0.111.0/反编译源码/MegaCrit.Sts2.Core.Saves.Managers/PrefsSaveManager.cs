using Godot;
using MegaCrit.Sts2.Core.Saves.Migrations;

namespace MegaCrit.Sts2.Core.Saves.Managers;

public class PrefsSaveManager
{
	public const string fileName = "prefs.save";

	private readonly ISaveStore _saveStore;

	private readonly MigrationManager _migrationManager;

	private readonly IProfileIdProvider _profileIdProvider;

	public PrefsSave Prefs { get; set; }

	/// <summary>
	/// Whether <see cref="P:MegaCrit.Sts2.Core.Saves.Managers.PrefsSaveManager.Prefs" /> has been read. It is null until <see cref="M:MegaCrit.Sts2.Core.Saves.Managers.PrefsSaveManager.LoadPrefs" /> runs despite
	/// the non-nullable annotation, so early-boot callers have to check this first.
	/// </summary>
	public bool IsLoaded { get; private set; }

	public PrefsSaveManager(int profileId, ISaveStore saveStore, MigrationManager migrationManager)
		: this(saveStore, migrationManager, new StaticProfileIdProvider(profileId))
	{
	}

	public PrefsSaveManager(ISaveStore saveStore, MigrationManager migrationManager, IProfileIdProvider profileIdProvider)
	{
		_saveStore = saveStore;
		_migrationManager = migrationManager;
		_profileIdProvider = profileIdProvider;
	}

	public static string GetPrefsPath(int profileId, bool? forceModState = null)
	{
		return UserDataPathProvider.GetProfileDir(profileId, forceModState).PathJoin(UserDataPathProvider.SavesDir).PathJoin("prefs.save");
	}

	public void SavePrefs()
	{
		Prefs.SchemaVersion = _migrationManager.GetLatestVersion<PrefsSave>();
		string content = JsonSerializationUtility.ToJson(Prefs);
		_saveStore.WriteFile(GetPrefsPath(_profileIdProvider.CurrentProfileId), content);
	}

	public ReadSaveResult<PrefsSave> LoadPrefs()
	{
		ReadSaveResult<PrefsSave> readSaveResult = _migrationManager.LoadSave<PrefsSave>(GetPrefsPath(_profileIdProvider.CurrentProfileId));
		if (!readSaveResult.Success || readSaveResult.SaveData == null)
		{
			Prefs = _migrationManager.CreateNewSave<PrefsSave>();
			SavePrefs();
		}
		else
		{
			Prefs = readSaveResult.SaveData;
		}
		IsLoaded = true;
		return readSaveResult;
	}
}
