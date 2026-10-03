using Godot;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Saves;

public static class UserDataPathProvider
{
	public static string SavesDir => "saves";

	public static bool IsRunningModded { get; set; }

	public static string GetProfileScopedPath(int profileId, string dataType, PlatformType? platformOverride = null, ulong? userIdOverride = null)
	{
		PlatformType platformType = platformOverride ?? PlatformUtil.PrimaryPlatform;
		ulong value = userIdOverride ?? PlatformUtil.GetLocalPlayerId(platformType);
		string platformDirectoryName = GetPlatformDirectoryName(platformType);
		return $"user://{platformDirectoryName}/{value}/{GetProfileDir(profileId)}/{dataType}";
	}

	public static string GetProfileScopedBasePath(int profileId, PlatformType? platformOverride = null, ulong? userIdOverride = null)
	{
		PlatformType platformType = platformOverride ?? PlatformUtil.PrimaryPlatform;
		ulong value = userIdOverride ?? PlatformUtil.GetLocalPlayerId(platformType);
		string platformDirectoryName = GetPlatformDirectoryName(platformType);
		return $"user://{platformDirectoryName}/{value}/{GetProfileDir(profileId)}";
	}

	public static string GetAccountScopedBasePath(string? dataType, PlatformType? platformOverride = null, ulong? userIdOverride = null)
	{
		PlatformType platformType = platformOverride ?? PlatformUtil.PrimaryPlatform;
		ulong value = userIdOverride ?? PlatformUtil.GetLocalPlayerId(platformType);
		string platformDirectoryName = GetPlatformDirectoryName(platformType);
		string text = $"user://{platformDirectoryName}/{value}";
		if (dataType != null)
		{
			text = text.PathJoin(dataType);
		}
		return text;
	}

	public static string GetAccountDir(bool? forceModState = null)
	{
		if (!(forceModState ?? IsRunningModded))
		{
			return "";
		}
		return "modded";
	}

	public static string GetProfileDir(int profileId)
	{
		return GetProfileDir(profileId, null);
	}

	public static string GetProfileDir(int profileId, bool? forceModState)
	{
		return GetAccountDir(forceModState).PathJoin($"profile{profileId}");
	}

	public static string GetLegacyPreAccountPath(string dataType)
	{
		return "user://" + dataType;
	}

	public static string GetPlatformDirectoryName(PlatformType platform)
	{
		if (platform == PlatformType.Steam)
		{
			return "steam";
		}
		return OS.HasFeature("editor") ? "editor" : "default";
	}

	public static bool IsLegacyPath(string path)
	{
		if (!path.Contains("/steam/") && !path.Contains("/default/"))
		{
			return !path.Contains("/editor/");
		}
		return false;
	}
}
