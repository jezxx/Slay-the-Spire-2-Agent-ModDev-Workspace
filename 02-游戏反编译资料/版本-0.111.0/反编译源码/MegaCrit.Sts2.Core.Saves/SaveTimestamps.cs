using System;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Logging;
using Sentry;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Helpers for turning raw filesystem / cloud last-modified timestamps into <see cref="T:System.DateTimeOffset" />s.
/// </summary>
public static class SaveTimestamps
{
	private static readonly long _minUnixSeconds = DateTimeOffset.MinValue.ToUnixTimeSeconds();

	private static readonly long _maxUnixSeconds = DateTimeOffset.MaxValue.ToUnixTimeSeconds();

	/// <summary>
	/// Converts a Unix-seconds last-modified timestamp into a <see cref="T:System.DateTimeOffset" />, returning
	/// <see cref="F:System.DateTimeOffset.UnixEpoch" /> for values outside the representable range instead of throwing
	/// <see cref="T:System.ArgumentOutOfRangeException" />.
	///
	/// Steam Cloud (<c>SteamRemoteStorage.GetFileTimestamp</c>) and the OS (<c>FileAccess.GetModifiedTime</c>) can
	/// report corrupt timestamps, for example during cloud-conflict resolution. Such a value used to crash the
	/// startup cloud sync (PRG-7045). Degrading a bad mtime to the epoch makes the file look "very old", so the sync
	/// treats it as stale and re-copies the valid cloud/local content instead of aborting the whole file's sync.
	/// </summary>
	public static DateTimeOffset FromUnixTimeSecondsOrEpoch(long seconds, string path)
	{
		string path2 = path;
		if (seconds < _minUnixSeconds || seconds > _maxUnixSeconds)
		{
			Log.Warn($"Last-modified timestamp {seconds} for {path2} is outside the representable DateTimeOffset range; treating it as the Unix epoch so the file re-syncs. (PRG-7045)");
			SentryService.CaptureMessage("Save-store last-modified timestamp out of DateTimeOffset range", SentryLevel.Warning, delegate(Scope scope)
			{
				scope.SetExtra("save.timestamp.raw_seconds", seconds);
				scope.SetExtra("save.timestamp.path", path2);
			});
			return DateTimeOffset.UnixEpoch;
		}
		return DateTimeOffset.FromUnixTimeSeconds(seconds);
	}
}
