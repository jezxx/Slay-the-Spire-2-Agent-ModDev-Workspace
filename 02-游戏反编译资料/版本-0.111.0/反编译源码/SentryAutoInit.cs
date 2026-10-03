using System.Runtime.CompilerServices;
using Sentry.Godot.Internal;

internal static class SentryAutoInit
{
	[ModuleInitializer]
	internal static void Init()
	{
		SentryGodotInitializer.AutoInit();
	}
}
