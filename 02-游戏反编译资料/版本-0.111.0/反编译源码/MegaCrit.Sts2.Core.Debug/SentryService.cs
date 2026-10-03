using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using Sentry;
using Sentry.Godot;

namespace MegaCrit.Sts2.Core.Debug;

/// <summary>
/// Manages Sentry initialization and error reporting.
/// Uses the sentry-godot 2.x unified integration: a single <see cref="M:Sentry.Godot.SentrySdk.Init(System.Action{Sentry.Godot.SentryGodotOptions})" />
/// call brings up both the managed (.NET) and native layers, and scope data (tags, breadcrumbs, user) set here
/// syncs to both automatically, so no manual bridge into the GDExtension singleton is needed. Contexts and
/// attachments do NOT sync, so anything that must be visible on native events is set as a tag.
/// </summary>
public static class SentryService
{
	private const string _dsnSettingPath = "sentry/config/dsn";

	private static float _sampleRate = 1f;

	private const float _earlyBootNativeSampleRate = 0.1f;

	private static bool _platformBranchResolved;

	private static bool _isHeadless;

	private static string? _nativeEnvironment;

	private static bool _isGameInitialized;

	private static volatile bool _suppressAllEvents;

	private static readonly string _sessionId = Guid.NewGuid().ToString();

	public static bool IsEnabled { get; private set; }

	public static bool SampleForNonSteamBranches { get; private set; }

	public static bool IsForcedOn { get; private set; }

	public static string SessionId => _sessionId;

	/// <summary>
	/// Suppresses all Sentry event capture when mods are detected. Called right after
	/// ModManager.Initialize so mod errors during the rest of startup are never reported,
	/// before AfterGameInit shuts things down.
	///
	/// Both layers get the same treatment through one flag: it is read as the first line of both the
	/// managed (FilterEvent) and native (FilterNativeEvent) before-send callbacks. That flag is a plain
	/// bool, so unlike the sampler it cannot throw during early startup and fail open (Sentry sends the
	/// event when a before-send throws). Native crashes bypass before-send entirely and are always
	/// captured, but AfterGameInit shuts the SDK down for modded sessions so those are not sent either.
	/// </summary>
	public static void DisableSentryIfModded()
	{
		if (ModManager.IsRunningModded())
		{
			_suppressAllEvents = true;
		}
	}

	/// <summary>
	/// Initializes Sentry. Should be called early in game startup (from the SentryBootstrap autoload).
	/// Disabled in editor (unless headless/forced). Uses "unknown" environment until the real Steam branch
	/// is known in SetPlatformBranch.
	/// </summary>
	public static void Initialize()
	{
		bool flag = OS.HasFeature("editor");
		_isHeadless = DisplayServer.GetName().Equals("headless", StringComparison.OrdinalIgnoreCase);
		bool isForcedOn = CommandLineHelper.HasArg("force-sentry");
		if (flag && !_isHeadless && !isForcedOn)
		{
			Log.Info("[Sentry.NET] Disabled in editor");
			return;
		}
		if (!isForcedOn && TestMode.IsTestRunFromCmdline())
		{
			Log.Info("[Sentry.NET] Disabled in test run");
			return;
		}
		SampleForNonSteamBranches = _isHeadless || isForcedOn;
		IsForcedOn = isForcedOn;
		string dsn = GetDsn();
		if (string.IsNullOrEmpty(dsn))
		{
			Log.Info("[Sentry.NET] Disabled: no DSN configured in project settings");
			return;
		}
		ReleaseInfo releaseInfo = ReleaseInfoManager.Instance.ReleaseInfo;
		string environment = "unknown";
		string release = releaseInfo?.Version ?? "dev";
		Sentry.Godot.SentrySdk.Init(delegate(SentryGodotOptions options)
		{
			options.Dsn = dsn;
			options.Environment = environment;
			options.Release = release;
			options.Debug = isForcedOn;
			options.AutoSessionTracking = false;
			options.IsGlobalModeEnabled = true;
			options.SendDefaultPii = false;
			options.SetBeforeSend((SentryEvent sentryEvent, SentryHint hint) => FilterEvent(sentryEvent));
			options.Native.SetBeforeSend(FilterNativeEvent);
		});
		IsEnabled = Sentry.Godot.SentrySdk.IsEnabled;
		if (!IsEnabled)
		{
			Log.Warn("[Sentry.NET] SDK initialization failed");
			return;
		}
		Sentry.Godot.SentrySdk.ConfigureScope(delegate(Scope scope)
		{
			scope.SetTag("sdk", "dotnet");
			scope.SetTag("session_id", _sessionId);
			scope.SetTag("os", OS.GetName());
			scope.SetTag("godot_version", Engine.GetVersionInfo()["string"].AsString());
			scope.SetTag("assembly.main_hash", AssemblyHasher.GetMainAssemblyHash().ToString());
			if (releaseInfo != null)
			{
				scope.SetTag("branch", releaseInfo.Branch);
				scope.SetExtra("build.commit", releaseInfo.Commit);
				scope.SetExtra("build.main_hash", releaseInfo.MainAssemblyHash);
				scope.SetExtra("build.date", releaseInfo.Date.ToString("o"));
			}
			AddAutoslayTags(scope);
		});
		Log.LogCallback += OnLogCallback;
		Log.Info("[Sentry.NET] Initialized: env=" + environment + ", release=" + release);
	}

	public static void AfterGameInit(string? platformBranch, string uniqueId)
	{
		string uniqueId2 = uniqueId;
		if (!IsEnabled)
		{
			return;
		}
		if (!ShouldStayAliveAfterInit(shouldLog: true))
		{
			Log.Info("[Sentry.NET] Shutting down because event reporting is disabled.");
			Shutdown();
		}
		else
		{
			Sentry.Godot.SentrySdk.ConfigureScope(delegate(Scope scope)
			{
				scope.User = new SentryUser
				{
					Id = uniqueId2
				};
			});
			Log.Debug("[Sentry.NET] User context set");
			SetPlatformBranch(platformBranch);
		}
		_isGameInitialized = true;
	}

	private static void OnLogCallback(LogLevel level, string message, int skipFrames)
	{
		if (IsEnabled)
		{
			switch (level)
			{
			case LogLevel.Error:
				Sentry.Godot.SentrySdk.AddBreadcrumb(message, "log", null, null, BreadcrumbLevel.Error);
				break;
			case LogLevel.Warn:
				Sentry.Godot.SentrySdk.AddBreadcrumb(message, "log", null, null, BreadcrumbLevel.Warning);
				break;
			}
		}
	}

	/// <summary>
	/// Tags autoslay runs so their events can be filtered out in Sentry. Re-homed from the old SentryInit.gd,
	/// which owned these tags before the GDScript autoload was removed.
	/// </summary>
	private static void AddAutoslayTags(Scope scope)
	{
		if (!CommandLineHelper.HasArg("autoslay"))
		{
			return;
		}
		scope.SetTag("autoslay", "true");
		string[] cmdlineArgs = OS.GetCmdlineArgs();
		foreach (string text in cmdlineArgs)
		{
			if (text.StartsWith("--seed=", StringComparison.Ordinal))
			{
				string text2 = text;
				int length = "--seed=".Length;
				scope.SetTag("autoslay.seed", text2.Substring(length, text2.Length - length));
				break;
			}
		}
	}

	/// <summary>
	/// Before-send filter for native/engine events (non-fatal GDScript/GDExtension/engine errors). Drops modded
	/// sessions, transient build artifacts, and, in headless mode, hardware errors with no display/audio/GPU
	/// context, then stamps the native environment and applies sampling + consent. C# exceptions never reach here
	/// (the SDK forwards them to the managed layer). Native crashes bypass this callback entirely and are always
	/// captured. The event wrapper is valid only for the duration of this call and must not be retained.
	/// </summary>
	private static SentryNativeEvent? FilterNativeEvent(SentryNativeEvent nativeEvent)
	{
		if (_suppressAllEvents)
		{
			return null;
		}
		string exceptionValue = nativeEvent.GetExceptionValue(0);
		if (!string.IsNullOrEmpty(exceptionValue))
		{
			if (exceptionValue.Contains("/build/modules/mono/glue/") || exceptionValue.Contains("res://.godot/mono/temp/"))
			{
				return null;
			}
			if (_isHeadless)
			{
				if (exceptionValue.Contains("fmod", StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}
				if (exceptionValue.Contains("Failed to set animation mix"))
				{
					return null;
				}
				if (exceptionValue.Contains("custom_samplers.has"))
				{
					return null;
				}
				if (exceptionValue.Contains("Parameter \"mem\" is null"))
				{
					return null;
				}
				if (exceptionValue.Contains("Attempting to initialize the wrong RID"))
				{
					return null;
				}
				if (exceptionValue.Contains("Initializing already initialized RID"))
				{
					return null;
				}
			}
		}
		if (_nativeEnvironment != null)
		{
			nativeEvent.Environment = _nativeEnvironment;
		}
		float sampleRate = (_platformBranchResolved ? _sampleRate : 0.1f);
		if (!ShouldSampleEvent(sampleRate))
		{
			return null;
		}
		return nativeEvent;
	}

	/// <summary>
	/// Configures sampling rate based on the Steam branch. Call after Steam initializes.
	/// Shuts down Sentry entirely for non-Steam builds (null branch).
	/// </summary>
	private static void SetPlatformBranch(string? branch)
	{
		string branch2 = branch;
		_sampleRate = branch2 switch
		{
			"public" => 0.1f, 
			"private-beta" => 1f, 
			"public-beta" => 0.2f, 
			_ => (branch2 != null) ? 0.1f : (SampleForNonSteamBranches ? 1f : 0f), 
		};
		_platformBranchResolved = true;
		if (IsEnabled)
		{
			if (_sampleRate == 0f)
			{
				Log.Info("[Sentry.NET] Disabled: no platform branch (non-Steam build)");
				Shutdown();
				return;
			}
			if (branch2 != null)
			{
				_nativeEnvironment = branch2;
				Sentry.Godot.SentrySdk.ConfigureScope(delegate(Scope scope)
				{
					scope.SetTag("platform.branch", branch2);
					scope.Environment = branch2;
				});
			}
		}
		Log.Info($"[Sentry.NET] Platform branch: {branch2}, sample rate: {_sampleRate:P0}");
	}

	/// <summary>
	/// Adds a breadcrumb for tracking user actions leading up to an error.
	/// </summary>
	public static void AddBreadcrumb(string message, string category = "app", BreadcrumbLevel level = BreadcrumbLevel.Info)
	{
		if (IsEnabled)
		{
			Sentry.Godot.SentrySdk.AddBreadcrumb(message, category, null, null, level);
		}
	}

	/// <summary>
	/// Captures an exception and sends it to Sentry.
	/// Attaches current game state context for debugging.
	/// Respects user consent settings.
	/// </summary>
	public static void CaptureException(Exception ex)
	{
		if (IsEnabled)
		{
			Sentry.Godot.SentrySdk.CaptureException(ex, delegate(Scope scope)
			{
				AttachGameState(scope);
			});
		}
	}

	/// <summary>
	/// Captures an exception with additional scope configuration.
	/// AttachGameState is called first, then the caller's configureScope action.
	/// </summary>
	public static void CaptureException(Exception ex, Action<Scope> configureScope)
	{
		Action<Scope> configureScope2 = configureScope;
		if (IsEnabled)
		{
			Sentry.Godot.SentrySdk.CaptureException(ex, delegate(Scope scope)
			{
				AttachGameState(scope);
				configureScope2(scope);
			});
		}
	}

	/// <summary>
	/// Captures a message and sends it to Sentry.
	/// Respects user consent settings.
	/// </summary>
	public static void CaptureMessage(string message, SentryLevel level = SentryLevel.Info, Action<Scope>? configureScope = null)
	{
		Action<Scope> configureScope2 = configureScope;
		if (IsEnabled)
		{
			SentryEvent evt = new SentryEvent
			{
				Message = message,
				Level = level
			};
			Sentry.Godot.SentrySdk.CaptureEvent(evt, delegate(Scope scope)
			{
				AttachGameState(scope);
				configureScope2?.Invoke(scope);
			});
		}
	}

	/// <summary>
	/// Sets a tag on the current scope for filtering in Sentry.
	/// </summary>
	public static void SetTag(string key, string value)
	{
		string key2 = key;
		string value2 = value;
		if (IsEnabled)
		{
			Sentry.Godot.SentrySdk.ConfigureScope(delegate(Scope scope)
			{
				scope.SetTag(key2, value2);
			});
		}
	}

	/// <summary>
	/// Sets extra context data on the current scope.
	/// </summary>
	public static void SetExtra(string key, object value)
	{
		string key2 = key;
		object value2 = value;
		if (IsEnabled)
		{
			Sentry.Godot.SentrySdk.ConfigureScope(delegate(Scope scope)
			{
				scope.SetExtra(key2, value2);
			});
		}
	}

	/// <summary>
	/// Initializes Sentry for testing purposes, bypassing editor and release checks.
	/// Only use from dev console commands.
	/// </summary>
	public static void InitializeForTesting()
	{
		if (IsEnabled)
		{
			return;
		}
		string dsn = GetDsn();
		if (string.IsNullOrEmpty(dsn))
		{
			Log.Warn("[Sentry.NET] Cannot initialize for testing: no DSN configured");
			return;
		}
		Sentry.Godot.SentrySdk.Init(delegate(SentryGodotOptions options)
		{
			options.Dsn = dsn;
			options.Environment = "development";
			options.Release = ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? "dev-console-test";
			options.Debug = false;
			options.AutoSessionTracking = false;
			options.IsGlobalModeEnabled = true;
			options.SendDefaultPii = false;
		});
		IsEnabled = Sentry.Godot.SentrySdk.IsEnabled;
		if (!IsEnabled)
		{
			Log.Warn("[Sentry.NET] SDK initialization failed for testing");
			return;
		}
		Sentry.Godot.SentrySdk.ConfigureScope(delegate(Scope scope)
		{
			scope.SetTag("sdk", "dotnet");
			scope.SetTag("session_id", _sessionId);
			scope.SetTag("source", "dev-console-test");
		});
		Log.Info("[Sentry.NET] Initialized for testing via dev console");
	}

	/// <summary>
	/// Shuts down the Sentry SDK gracefully, closing both the .NET and native layers.
	/// Should be called when the game exits.
	/// </summary>
	public static void Shutdown()
	{
		if (IsEnabled)
		{
			Log.LogCallback -= OnLogCallback;
			Log.Info("[Sentry.NET] Shutting down");
			Sentry.Godot.SentrySdk.Close();
			IsEnabled = false;
		}
	}

	private static string GetDsn()
	{
		return ProjectSettings.GetSetting("sentry/config/dsn", "").AsString();
	}

	/// <summary>
	/// Attaches current game state to the Sentry scope for debugging context.
	/// Collects scene, run info, and combat state when available.
	/// </summary>
	private static void AttachGameState(Scope scope)
	{
		try
		{
			scope.SetExtra("loc.language", LocManager.Instance.Language);
			string currentSceneName = GetCurrentSceneName();
			if (currentSceneName != null)
			{
				scope.SetTag("game.scene", currentSceneName);
			}
			RunState runState = RunManager.Instance.DebugOnlyGetState();
			if (RunManager.Instance.IsInProgress && runState != null)
			{
				scope.SetTag("game.in_run", "true");
				scope.SetExtra("game.seed", runState.Rng.StringSeed);
				scope.SetExtra("game.ascension", runState.AscensionLevel);
				scope.SetExtra("game.act", runState.CurrentActIndex + 1);
				scope.SetExtra("game.act_name", runState.Act.Id.ToString());
				scope.SetExtra("game.floor", runState.TotalFloor);
				scope.SetExtra("game.mode", runState.GameMode);
				AbstractRoom currentRoom = runState.CurrentRoom;
				scope.SetExtra("game.room_type", currentRoom?.GetType().Name);
				if (currentRoom is EventRoom eventRoom)
				{
					scope.SetExtra("game.event", eventRoom.CanonicalEvent.Id.Entry);
				}
				IReadOnlyList<Player> players = runState.Players;
				if (players.Count > 0)
				{
					scope.SetExtra("game.characters", string.Join(", ", players.Select((Player p) => p.Character.Id)));
					scope.SetExtra("game.player_count", players.Count);
				}
			}
			else
			{
				scope.SetTag("game.in_run", "false");
			}
			CombatState combatState = CombatManager.Instance.DebugOnlyGetState();
			if (combatState != null)
			{
				scope.SetExtra("combat.encounter", combatState.Encounter?.Id.Entry);
				scope.SetExtra("combat.round", combatState.RoundNumber);
				scope.SetExtra("combat.enemy_count", combatState.Enemies.Count);
				scope.SetExtra("combat.enemies", string.Join(", ", combatState.Enemies.Select((Creature e) => e.Monster?.Id.ToString() ?? "unknown")));
				List<string> list = combatState.Players.Select((Player p) => $"{p.Creature.CurrentHp}/{p.Creature.MaxHp}").ToList();
				if (list.Count > 0)
				{
					scope.SetExtra("combat.player_hp", string.Join(", ", list));
				}
			}
		}
		catch
		{
		}
	}

	private static string? GetCurrentSceneName()
	{
		try
		{
			NGame instance = NGame.Instance;
			if (instance == null)
			{
				return null;
			}
			if (instance.MainMenu != null)
			{
				return "MainMenu";
			}
			if (instance.CurrentRunNode != null)
			{
				NRun currentRunNode = instance.CurrentRunNode;
				if (currentRunNode.CombatRoom != null)
				{
					return "CombatRoom";
				}
				if (currentRunNode.MapRoom != null)
				{
					return "MapRoom";
				}
				if (currentRunNode.EventRoom != null)
				{
					return "EventRoom";
				}
				if (currentRunNode.RestSiteRoom != null)
				{
					return "RestSiteRoom";
				}
				if (currentRunNode.MerchantRoom != null)
				{
					return "MerchantRoom";
				}
				if (currentRunNode.TreasureRoom != null)
				{
					return "TreasureRoom";
				}
				return "Run";
			}
			if (instance.LogoAnimation != null)
			{
				return "LogoAnimation";
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// The managed BeforeSend filter. Suppression (set for modded sessions) is checked first and is a
	/// plain bool read, so it cannot throw and cannot be defeated by an exception in the sampler
	/// below it (Sentry sends the event if BeforeSend throws).
	/// </summary>
	private static SentryEvent? FilterEvent(SentryEvent sentryEvent)
	{
		if (_suppressAllEvents)
		{
			return null;
		}
		if (sentryEvent.Exception is AutoSlayTimeoutException)
		{
			return null;
		}
		if (!ShouldSampleEvent())
		{
			return null;
		}
		return sentryEvent;
	}

	private static bool ShouldSampleEvent()
	{
		return ShouldSampleEvent(_sampleRate);
	}

	private static bool ShouldSampleEvent(float sampleRate)
	{
		if (System.Random.Shared.NextDouble() >= (double)sampleRate)
		{
			return false;
		}
		if (SaveManager.Instance.IsPrefsLoaded && !SaveManager.Instance.PrefsSave.UploadData)
		{
			return false;
		}
		if (!_isGameInitialized && !ShouldStayAliveAfterInit(shouldLog: false))
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Returns true if the Sentry service should shutdown after we've finished initializing the game.
	/// This should only return false for things that don't change during the runtime of the game.
	/// We shutdown the service rather than filtering events so that crashes (which can't get filtered) don't get sent
	/// for modded games.
	/// </summary>
	private static bool ShouldStayAliveAfterInit(bool shouldLog)
	{
		if (IsForcedOn)
		{
			if (shouldLog)
			{
				Log.Info("[Sentry.NET] Staying alive because we're forced on");
			}
			return true;
		}
		if (!SteamInitializer.Initialized)
		{
			if (shouldLog)
			{
				Log.Info("[Sentry.NET] Steam not initialized");
			}
			return false;
		}
		try
		{
			if (SaveManager.Instance.SettingsSave.FullConsole)
			{
				if (shouldLog)
				{
					Log.Info("[Sentry.NET] Full console is on");
				}
				return false;
			}
		}
		catch
		{
			if (shouldLog)
			{
				Log.Info("[Sentry.NET] Exception while checking UploadData or FullConsole");
			}
			return false;
		}
		if (ModManager.IsRunningModded())
		{
			if (shouldLog)
			{
				Log.Info("[Sentry.NET] Is running modded");
			}
			return false;
		}
		LocManager instance = LocManager.Instance;
		if (instance != null && instance.OverridesActive)
		{
			if (shouldLog)
			{
				Log.Info("[Sentry.NET] Loc overrides are active");
			}
			return false;
		}
		if (ModManager.HasHarmonyPatches())
		{
			if (shouldLog)
			{
				Log.Info("[Sentry.NET] Harmony patches active");
			}
			return false;
		}
		return true;
	}
}
