using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Exceptions;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Retries a file write that fails due to transient external interference on Windows
/// (antivirus on-access scans, cloud sync locks, file indexers). Sentry data (PRG-7042) shows
/// save writes intermittently fail with FileCantWrite in bursts across the whole save directory,
/// then recover moments later. This mirrors the retry already applied to
/// <see cref="M:MegaCrit.Sts2.Core.Saves.GodotFileIo.RenameFile(System.String,System.String)" /> in PRG-6241.
///
/// The write attempt MUST fully reopen the target file on each call. Godot's FileAccess reports
/// write errors via the sticky C stdio ferror() flag (drivers/windows/file_access_windows.cpp),
/// which is only cleared by reopening the handle, so retrying on the same handle could keep
/// reporting a stale error even after a later write succeeds.
///
/// On the final attempt the failure exception propagates unchanged, preserving the original Sentry
/// grouping and deferring error reporting until every retry is exhausted.
/// </summary>
public static class FileWriteRetry
{
	public const int maxAttempts = 4;

	public const int retryDelayMs = 50;

	/// <summary>
	/// Runs <paramref name="writeAttempt" />, retrying on transient write failures. The
	/// <paramref name="sleep" /> parameter is injectable for tests; production passes Thread.Sleep.
	/// </summary>
	public static void Run(string path, Action writeAttempt, Action<int>? sleep = null)
	{
		if (sleep == null)
		{
			sleep = Thread.Sleep;
		}
		for (int i = 1; i <= 4; i++)
		{
			try
			{
				writeAttempt();
				break;
			}
			catch (Exception ex) when (IsTransient(ex) && i < 4)
			{
				Log.Warn($"File write failed (attempt {i}/{4}), retrying. path={path} error={ex.Message}");
				sleep(50);
			}
		}
	}

	/// <summary>
	/// Async counterpart to <see cref="M:MegaCrit.Sts2.Core.Saves.FileWriteRetry.Run(System.String,System.Action,System.Action{System.Int32})" />. The <paramref name="delay" /> parameter is injectable
	/// for tests; production passes Task.Delay.
	/// </summary>
	public static async Task RunAsync(string path, Func<Task> writeAttempt, Func<int, Task>? delay = null)
	{
		if (delay == null)
		{
			delay = (int ms) => Task.Delay(ms);
		}
		for (int attempt = 1; attempt <= 4; attempt++)
		{
			try
			{
				await writeAttempt();
				break;
			}
			catch (Exception ex) when (IsTransient(ex) && attempt < 4)
			{
				Log.Warn($"File write failed (attempt {attempt}/{4}), retrying. path={path} error={ex.Message}");
				await delay(50);
			}
		}
	}

	private static bool IsTransient(Exception e)
	{
		if (e is SaveException || e is IOException)
		{
			return true;
		}
		return false;
	}
}
