using System;

namespace MegaCrit.Sts2.Core.Debug;

public static class DebugSettings
{
	/// <summary>
	/// Skip intro logo and timeline requirement for faster dev iteration.
	/// Set via STS2_DEV_SKIP environment variable.
	/// </summary>
	public static bool DevSkip { get; } = Environment.GetEnvironmentVariable("STS2_DEV_SKIP") != null;


	/// <summary>
	/// Open windowed regardless of the saved display setting, so a dev launch does not take over the display.
	/// Set via STS2_DEV_WINDOWED environment variable, which the nix dev shell exports.
	/// </summary>
	public static bool DevWindowed { get; } = Environment.GetEnvironmentVariable("STS2_DEV_WINDOWED") != null;


	/// <summary>
	/// Should packed images be ignored in favor of the original image version?
	/// Set this to true to enable, but make sure to set it back to false before committing, or else you will get a test
	/// failure.
	/// </summary>
	public static bool IgnorePackedImages => false;
}
