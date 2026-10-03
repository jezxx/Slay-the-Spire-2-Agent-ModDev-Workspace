namespace MegaCrit.Sts2.Core.Platform;

public static class SupportedWindowModeExtensions
{
	public static bool ShouldForceFullscreen(this SupportedWindowMode mode)
	{
		if (mode != SupportedWindowMode.FullscreenOnly)
		{
			return mode == SupportedWindowMode.FullscreenOnlyDisplayToggle;
		}
		return true;
	}

	/// <summary>
	/// Resolves whether the window should open in fullscreen, from the saved setting and the platform's constraint.
	/// </summary>
	/// <param name="mode">What the current platform supports.</param>
	/// <param name="savedFullscreen">The player's saved fullscreen setting.</param>
	/// <param name="forceWindowed">
	/// Dev override that wins over both, so that launching from a dev shell never takes over the display.
	/// </param>
	public static bool ResolveFullscreen(this SupportedWindowMode mode, bool savedFullscreen, bool forceWindowed)
	{
		if (forceWindowed)
		{
			return false;
		}
		if (!savedFullscreen)
		{
			return mode.ShouldForceFullscreen();
		}
		return true;
	}
}
