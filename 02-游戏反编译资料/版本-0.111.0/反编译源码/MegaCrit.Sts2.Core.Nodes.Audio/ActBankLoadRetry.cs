using System;
using System.Threading;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Logging;
using Sentry;

namespace MegaCrit.Sts2.Core.Nodes.Audio;

/// <summary>
/// Retries an act-music bank load that fails intermittently. The FMOD GDExtension reads bank files
/// through Godot's FileAccess on a background thread with no retry, collapsing any open or
/// short-read failure into a hard loadBankFile error (SLAYTHESPIRE2-6GF). The failures look like
/// transient external interference (antivirus scans, cloud sync, file indexers touching the .pck),
/// the same class that affects save writes, but the exact FMOD_RESULT is not captured, so the cause
/// is inferred rather than confirmed. When a load fails the act bank stays unloaded; the production
/// errors show the global "Progress" parameter then cannot be set, which indicates it is served
/// from the act bank, so combat music freezes on its last value ("music inertia"). This mirrors the
/// retry already applied to save writes in <see cref="T:MegaCrit.Sts2.Core.Saves.FileWriteRetry" />.
///
/// The load is reported as a success/failure bool rather than via an exception, so this retries on
/// a false result instead of a caught throw. The bank loader MUST fully unload the failed loader
/// between attempts (the proxy does this) so a retry issues a fresh loadBankFile rather than reusing
/// the failed load.
///
/// Each non-healthy outcome is reported to Sentry (a recovery that needed more than one attempt, or
/// an exhausted failure) so we can measure in production whether the retry window is sized right:
/// the recovered-vs-failed ratio and the attempt distribution tell us if a wider window or a
/// different approach (keeping banks resident) is warranted. First-try successes, expected to be the
/// common case, are not reported.
/// </summary>
public static class ActBankLoadRetry
{
	/// <summary>The result of a load: whether it ultimately loaded, and how many attempts it took.</summary>
	public readonly record struct LoadOutcome(bool Loaded, int Attempts);

	public const int maxAttempts = 3;

	public const int retryDelayMs = 50;

	/// <summary>
	/// Runs <paramref name="loadAttempt" />, retrying while it reports failure (false). Returns true
	/// once a load succeeds, or false if every attempt fails. <paramref name="sleep" /> and
	/// <paramref name="report" /> are injectable for tests; production passes Thread.Sleep and the
	/// Sentry reporter.
	/// </summary>
	public static bool Run(string bankPath, Func<bool> loadAttempt, Action<int>? sleep = null, Action<string, LoadOutcome>? report = null)
	{
		if (sleep == null)
		{
			sleep = Thread.Sleep;
		}
		if (report == null)
		{
			report = ReportToSentry;
		}
		for (int i = 1; i <= 3; i++)
		{
			if (loadAttempt())
			{
				report(bankPath, new LoadOutcome(Loaded: true, i));
				return true;
			}
			if (i < 3)
			{
				Log.Warn($"Act music bank load failed (attempt {i}/{3}), retrying. path={bankPath}");
				sleep(50);
			}
		}
		Log.Error($"Act music bank failed to load after {3} attempts; music will not follow combat state. path={bankPath}");
		report(bankPath, new LoadOutcome(Loaded: false, 3));
		return false;
	}

	private static void ReportToSentry(string bankPath, LoadOutcome outcome)
	{
		string bankPath2 = bankPath;
		if (!outcome.Loaded || outcome.Attempts != 1)
		{
			string result = (outcome.Loaded ? "recovered" : "failed");
			SentryLevel level = (outcome.Loaded ? SentryLevel.Info : SentryLevel.Warning);
			SentryService.CaptureMessage("Act music bank load needed retries or failed", level, delegate(Scope scope)
			{
				scope.SetTag("act_bank_load", result);
				scope.SetTag("act_bank_load_attempts", outcome.Attempts.ToString());
				scope.SetTag("act_bank_path", bankPath2);
				scope.SetFingerprint("act-bank-load", result);
			});
		}
	}
}
