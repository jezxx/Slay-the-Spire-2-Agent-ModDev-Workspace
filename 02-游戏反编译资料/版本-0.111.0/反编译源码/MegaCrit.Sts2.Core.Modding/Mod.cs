using System.Collections.Generic;
using System.Reflection;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Modding;

/// <summary>
/// Information about a loaded mod.
/// </summary>
public class Mod
{
	/// <summary>
	/// Where the mod originated from.
	/// </summary>
	public ModSource modSource;

	/// <summary>
	/// Path where the mod files are located.
	/// </summary>
	public required string path;

	/// <summary>
	/// Whether the mod was loaded, and if it was not loaded, why.
	/// Since there's no way to unload mods while the game is running, this value cannot change after the initial mod
	/// initialization occurs.
	/// Even if the mod is set to disabled in SettingsSave, this value is the true source of whether or not the mod was
	/// loaded into the game.
	/// </summary>
	public ModLoadState state;

	/// <summary>
	/// The mod manifest.
	/// </summary>
	public ModManifest? manifest;

	/// <summary>
	/// The version parsed from the mod manifest.
	/// Null if the version is not present or is not a valid semantic version.
	/// </summary>
	public SemanticVersion? version;

	/// <summary>
	/// The C# assemblies loaded with the mod, if any.
	/// There are usually only zero or one in the list. Mods can register more assemblies, e.g. in the situation where
	/// they create dynamic assemblies.
	/// </summary>
	public List<Assembly> assemblies = new List<Assembly>();

	/// <summary>
	/// If null, then no errors occurred while loading the mod.
	/// If this is set, then there was an error loading the mod that should be displayed. That doesn't necessarily mean
	/// the mod failed to load; see <see cref="F:MegaCrit.Sts2.Core.Modding.Mod.state" /> for that.
	/// </summary>
	public List<LocString>? errors;

	/// <summary>
	/// The ID of the workshop item, if the mod source is Steam Workshop.
	/// This can be used as the value of a PublishedFileId_t, but we use ulong here so that we don't have to guard this
	/// with DISABLESTEAMWORKS.
	/// </summary>
	public ulong? workshopId;
}
