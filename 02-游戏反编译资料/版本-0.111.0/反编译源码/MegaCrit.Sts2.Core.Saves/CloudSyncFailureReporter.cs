using System;
using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Rate limits how many cloud-to-local sync failures are reported to Sentry per game session.
///
/// A single machine with a corrupt or oversized cloud save directory can fail to sync hundreds of files in one
/// launch (PRG-7044): the startup sync iterates every run-history file, and each failure was previously captured
/// as its own Sentry event. Because identical failures fingerprint to the same issue, reporting every one adds
/// quota cost without diagnostic value. The first few events (with their surrounding breadcrumbs) already describe
/// the pattern, so we cap reporting per session and let local logs keep the full per-file detail.
///
/// The cap is applied per exception type, not globally. A corrupt directory floods the session with one repeated
/// exception type, but a qualitatively different failure later in the same session (for example an SEHException
/// from the native Steam DLL versus a managed SteamRemoteSaveStoreException) is a distinct diagnostic signal and
/// must still reach Sentry. Bucketing by type suppresses volume within a fingerprint without hiding unrelated
/// failures behind it.
/// </summary>
public class CloudSyncFailureReporter
{
	/// <summary>
	/// The number of sync failures of a given exception type reported to Sentry before the rest of that type's
	/// failures are suppressed for the session.
	/// </summary>
	public const int maxReportsPerSessionPerType = 5;

	private readonly Dictionary<Type, int> _reportedCountsByType = new Dictionary<Type, int>();

	/// <summary>
	/// Records a sync failure and returns true if it should be reported to Sentry, or false if it should be
	/// suppressed because the session cap for this exception's type has been reached. Suppressed failures should
	/// still be logged locally.
	/// </summary>
	public bool ShouldReport(Exception exception)
	{
		Type type = exception.GetType();
		int valueOrDefault = _reportedCountsByType.GetValueOrDefault(type);
		if (valueOrDefault >= 5)
		{
			return false;
		}
		_reportedCountsByType[type] = valueOrDefault + 1;
		return true;
	}
}
