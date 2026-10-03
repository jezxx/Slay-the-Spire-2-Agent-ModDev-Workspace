using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Handlers;
using MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms;
using MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.Core.Nodes.TopBar;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.AutoSlay;

/// <summary>
/// Main orchestrator for AutoSlay. Runs the game automatically for smoke testing.
/// </summary>
public class AutoSlayer
{
	private readonly Dictionary<RoomType, IRoomHandler> _roomHandlers;

	private readonly Dictionary<Type, IScreenHandler> _screenHandlers;

	private readonly MapScreenHandler _mapHandler;

	private CancellationTokenSource? _cts;

	private string? _seed;

	private Rng? _random;

	private Watchdog? _watchdog;

	private IDisposable? _cardSelectorScope;

	private static int _exitCode;

	/// <summary>How often to re-check a pending task while draining the screens it opens.</summary>
	private const int _drainPollIntervalMs = 50;

	/// <summary>Static flag indicating if AutoSlay is currently running.</summary>
	public static bool IsActive { get; private set; }

	/// <summary>Gets the current watchdog instance (for WaitHelper integration).</summary>
	public static Watchdog? CurrentWatchdog { get; private set; }

	static AutoSlayer()
	{
		NonInteractiveMode.AutoSlayerCheck = () => IsActive;
	}

	public AutoSlayer()
	{
		CombatRoomHandler value = new CombatRoomHandler();
		_roomHandlers = new Dictionary<RoomType, IRoomHandler>
		{
			[RoomType.Monster] = value,
			[RoomType.Elite] = value,
			[RoomType.Boss] = value,
			[RoomType.Event] = new EventRoomHandler(),
			[RoomType.Shop] = new ShopRoomHandler(DrainOverlayScreensUntilAsync),
			[RoomType.Treasure] = new TreasureRoomHandler(),
			[RoomType.RestSite] = new RestSiteRoomHandler()
		};
		_mapHandler = new MapScreenHandler();
		_screenHandlers = new Dictionary<Type, IScreenHandler>
		{
			[typeof(NRewardsScreen)] = new RewardsScreenHandler(),
			[typeof(NCardRewardSelectionScreen)] = new CardRewardScreenHandler(),
			[typeof(NDeckUpgradeSelectScreen)] = new DeckUpgradeScreenHandler(),
			[typeof(NDeckTransformSelectScreen)] = new DeckTransformScreenHandler(),
			[typeof(NDeckEnchantSelectScreen)] = new DeckEnchantScreenHandler(),
			[typeof(NDeckCardSelectScreen)] = new DeckCardSelectScreenHandler(),
			[typeof(NSimpleCardSelectScreen)] = new SimpleCardSelectScreenHandler(),
			[typeof(NChooseACardSelectionScreen)] = new ChooseACardScreenHandler(),
			[typeof(NChooseABundleSelectionScreen)] = new ChooseABundleScreenHandler(),
			[typeof(NChooseARelicSelection)] = new ChooseARelicScreenHandler(),
			[typeof(NGameOverScreen)] = new GameOverScreenHandler(),
			[typeof(NCrystalSphereScreen)] = new CrystalSphereScreenHandler()
		};
	}

	/// <summary>Starts an AutoSlay run with the given seed.</summary>
	public void Start(string seed, string? logFile = null)
	{
		if (logFile != null)
		{
			AutoSlayLog.OpenLogFile(logFile);
		}
		SentryService.SetTag("autoslay", "true");
		SentryService.SetTag("autoslay.seed", seed);
		IsActive = true;
		_cts = new CancellationTokenSource();
		Task task = RunAsync(seed, _cts.Token);
		TaskHelper.RunSafely(task);
	}

	/// <summary>Stops the current AutoSlay run.</summary>
	public void Stop()
	{
		IsActive = false;
		_cts?.Cancel();
		_cts = null;
	}

	/// <summary>Gets the current overlay screen cast to the expected type.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	/// The top of the overlay stack is not a <typeparamref name="T" />, or there is no stack to peek.
	/// </exception>
	public static T GetCurrentScreen<T>() where T : Node
	{
		IOverlayScreen overlayScreen = NOverlayStack.Instance?.Peek();
		if (!(overlayScreen is T result))
		{
			throw new InvalidOperationException($"Expected {typeof(T).Name} on top of the overlay stack, found {overlayScreen?.GetType().Name ?? "nothing"}.");
		}
		return result;
	}

	private async Task RunAsync(string seed, CancellationToken ct)
	{
		string seed2 = seed;
		AutoSlayLog.RunStarted(seed2);
		try
		{
			await WaitHelper.WithTimeout((CancellationToken token) => PlayRunAsync(seed2, token), AutoSlayConfig.runTimeout, ct);
			AutoSlayLog.RunCompleted(seed2);
		}
		catch (Exception ex)
		{
			_exitCode = 1;
			AutoSlayLog.RunFailed(seed2, ex);
			throw;
		}
		finally
		{
			IsActive = false;
			CurrentWatchdog = null;
			_watchdog = null;
			_cardSelectorScope?.Dispose();
			_cardSelectorScope = null;
			MemoryProfiler.Reset();
			AutoSlayLog.CloseLogFile();
			QuitGame(_exitCode);
		}
	}

	private async Task PlayRunAsync(string seed, CancellationToken ct)
	{
		await WaitHelper.Until(() => NGame.Instance != null, ct, AutoSlayConfig.gameInitTimeout, "Game instance not initialized");
		_seed = seed;
		NGame.Instance.DebugSeedOverride = seed;
		SaveManager.Instance.PrefsSave.FastMode = FastModeType.Fast;
		SaveManager.Instance.SetFtuesEnabled(enabled: false);
		SaveManager.Instance.MarkFtueAsComplete("ascension_singleplayer_ftue");
		SaveManager.Instance.ObtainEpochOverride(EpochModel.GetId<Silent1Epoch>(), EpochState.Revealed);
		SaveManager.Instance.ObtainEpochOverride(EpochModel.GetId<Regent1Epoch>(), EpochState.Revealed);
		SaveManager.Instance.ObtainEpochOverride(EpochModel.GetId<Defect1Epoch>(), EpochState.Revealed);
		SaveManager.Instance.ObtainEpochOverride(EpochModel.GetId<Necrobinder1Epoch>(), EpochState.Revealed);
		_random = new Rng(StringHelper.GetDeterministicHashCode(seed));
		_cardSelectorScope = CardSelectCmd.UseSelector(new AutoSlayCardSelector(_random));
		_watchdog = new Watchdog();
		CurrentWatchdog = _watchdog;
		_watchdog.Reset("Playing main menu");
		await PlayMainMenuAsync(ct);
		await WaitHelper.Until(() => RunManager.Instance.DebugOnlyGetState() != null, ct, AutoSlayConfig.runStateTimeout, "Run state not initialized");
		RunState runState = RunManager.Instance.DebugOnlyGetState();
		MemoryProfiler.SetBaseline();
		await WaitHelper.Until(() => runState.CurrentRoom != null && runState.CurrentRoom.RoomType != RoomType.Unassigned, ct, AutoSlayConfig.nodeWaitTimeout, "Room type not assigned");
		while (runState.TotalFloor < 49)
		{
			ct.ThrowIfCancellationRequested();
			RoomType roomType = runState.CurrentRoom.RoomType;
			_watchdog.Reset($"Entering {roomType} room (Act {runState.CurrentActIndex + 1}, Floor {runState.ActFloor})");
			AutoSlayLog.EnterRoom(roomType, runState.CurrentActIndex, runState.ActFloor);
			MemoryProfiler.LogSnapshot($"pre-room:{roomType}:Act{runState.CurrentActIndex + 1}:F{runState.ActFloor}");
			await HandleRoomAsync(roomType, ct);
			if ((uint)(roomType - 1) > 2u)
			{
				await Task.Delay(500, ct);
			}
			else
			{
				await WaitForRewardsScreenAsync(ct);
			}
			await DrainOverlayScreensAsync(ct);
			if (roomType == RoomType.RestSite)
			{
				await ClickRestSiteProceedIfNeeded(ct);
			}
			if (roomType == RoomType.Event)
			{
				await ClickEventProceedIfNeeded(ct);
			}
			MemoryProfiler.LogSnapshot($"post-room:{roomType}:Act{runState.CurrentActIndex + 1}:F{runState.ActFloor}");
			bool flag = roomType == RoomType.Boss && runState.Map.SecondBossMapPoint != null && runState.CurrentMapCoord == runState.Map.BossMapPoint.coord;
			if (roomType == RoomType.Boss && !flag)
			{
				_watchdog.Reset("Waiting for act transition after boss");
				RoomType postBossRoomType = RoomType.Boss;
				await WaitHelper.Until(delegate
				{
					AbstractRoom currentRoom = runState.CurrentRoom;
					if (currentRoom == null)
					{
						return false;
					}
					postBossRoomType = currentRoom.RoomType;
					return postBossRoomType != RoomType.Boss;
				}, ct, TimeSpan.FromSeconds(10L), "Act transition did not start after boss");
				AutoSlayLog.Info($"Post-boss transition: room type is now {postBossRoomType}");
				if (postBossRoomType == RoomType.Event && runState.CurrentActIndex >= runState.Acts.Count - 1)
				{
					_watchdog.Reset($"Entering {postBossRoomType} room (Act {runState.CurrentActIndex + 1}, Floor {runState.ActFloor})");
					AutoSlayLog.EnterRoom(postBossRoomType, runState.CurrentActIndex, runState.ActFloor);
					await HandleRoomAsync(postBossRoomType, ct);
					await WaitForGameOverScreenAsync(ct);
					await DrainOverlayScreensAsync(ct);
					_watchdog.Reset("Waiting for main menu after victory");
					await WaitForMainMenuAsync(ct);
					AutoSlayLog.Action("Victory! Run completed and returned to main menu");
					return;
				}
				await WaitHelper.Until(() => runState.VisitedMapCoords.Count == 0, ct, TimeSpan.FromSeconds(5L), "Act transition did not complete (VisitedMapCoords not cleared)");
				MemoryProfiler.LogSnapshot($"act-transition:Act{runState.CurrentActIndex + 1}");
			}
			_watchdog.Reset("Navigating map");
			await _mapHandler.HandleAsync(_random, ct);
		}
		AutoSlayLog.Action("Run completed (max floor reached). Abandoning");
		await AbandonRunAsync(ct);
	}

	private async Task HandleRoomAsync(RoomType roomType, CancellationToken ct)
	{
		if (!_roomHandlers.TryGetValue(roomType, out IRoomHandler handler))
		{
			AutoSlayLog.Warn($"No handler for room type: {roomType}");
		}
		else
		{
			await WaitHelper.WithTimeout((CancellationToken token) => handler.HandleAsync(_random, token), handler.Timeout, ct);
			AutoSlayLog.ExitRoom(roomType);
		}
	}

	/// <summary>
	/// Drains overlay screens until <paramref name="pending" /> completes.
	/// </summary>
	/// <remarks>
	/// A room handler that awaits something which opens a screen cannot rely on the drain
	/// between rooms: the run loop does not reach it until the room finishes, and the room is
	/// blocked on that task. Buying Orrery or Cauldron does exactly this, since both await
	/// <c>RewardsCmd.OfferCustom</c> when obtained. Driving the drain alongside the task breaks
	/// the cycle. The drain runs in fail-when-stuck mode here, because a screen it cannot close
	/// will never be closed by anyone else from this call site.
	/// </remarks>
	private async Task DrainOverlayScreensUntilAsync(Task pending, CancellationToken ct)
	{
		_ = 2;
		try
		{
			while (!pending.IsCompleted)
			{
				ct.ThrowIfCancellationRequested();
				NOverlayStack? instance = NOverlayStack.Instance;
				if (instance != null && instance.ScreenCount > 0)
				{
					await DrainOverlayScreensAsync(ct, failWhenStuck: true);
				}
				await Task.Delay(50, ct);
			}
			await pending;
		}
		finally
		{
			if (!pending.IsCompleted)
			{
				pending.ContinueWith((Task t) => t.Exception, TaskContinuationOptions.OnlyOnFaulted);
			}
		}
	}

	/// <param name="ct">Cancels the drain.</param>
	/// <param name="failWhenStuck">
	/// Throw instead of returning when the drain cannot close the screen on top. Callers that
	/// are waiting on that screen have no way to recover, so returning would spin them until an
	/// outer timeout fires with the real reason buried.
	/// </param>
	private async Task DrainOverlayScreensAsync(CancellationToken ct, bool failWhenStuck = false)
	{
		if (NOverlayStack.Instance == null)
		{
			await WaitHelper.Until(() => NOverlayStack.Instance != null, ct, AutoSlayConfig.nodeWaitTimeout, "Overlay stack not initialized");
		}
		int consecutiveNoProgress = 0;
		while (true)
		{
			NOverlayStack instance = NOverlayStack.Instance;
			if (instance == null || instance.ScreenCount == 0)
			{
				break;
			}
			ct.ThrowIfCancellationRequested();
			IOverlayScreen currentOverlay = instance.Peek();
			if (currentOverlay == null)
			{
				break;
			}
			Node node = (Node)currentOverlay;
			Type screenType = node.GetType();
			if (!_screenHandlers.TryGetValue(screenType, out IScreenHandler handler))
			{
				AutoSlayLog.Warn("No handler for screen type: " + screenType.Name);
				if (failWhenStuck)
				{
					throw new InvalidOperationException("No handler for screen " + screenType.Name + ", which is blocking a pending action");
				}
				break;
			}
			_watchdog.Reset("Handling screen: " + screenType.Name);
			int screenCountBefore = instance.ScreenCount;
			await WaitHelper.WithTimeout((CancellationToken token) => handler.HandleAsync(_random, token), handler.Timeout, ct);
			if (currentOverlay is NRewardsScreen && (NMapScreen.Instance?.IsOpen ?? false))
			{
				AutoSlayLog.Info("Rewards screen handled and map is open, exiting drain loop");
				if (failWhenStuck)
				{
					throw new InvalidOperationException("Map opened while a pending action was still waiting on the rewards screen");
				}
				break;
			}
			NOverlayStack instance2 = NOverlayStack.Instance;
			if (instance2 == null || instance2.ScreenCount != screenCountBefore || instance2.Peek() != currentOverlay)
			{
				consecutiveNoProgress = 0;
			}
			else
			{
				consecutiveNoProgress++;
				if (consecutiveNoProgress >= 3)
				{
					AutoSlayLog.Error($"Infinite loop detected: screen {screenType.Name} left the overlay stack unchanged after {3} handler attempts");
					throw new InvalidOperationException("Screen " + screenType.Name + " not closing after being handled");
				}
				AutoSlayLog.Warn($"Screen {screenType.Name} left the overlay stack unchanged after handling (attempt {consecutiveNoProgress})");
			}
			await Task.Delay(100, ct);
		}
	}

	private async Task ClickRestSiteProceedIfNeeded(CancellationToken ct)
	{
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		NProceedButton nodeOrNull = root.GetNodeOrNull<NProceedButton>("/root/Game/RootSceneContainer/Run/RoomContainer/RestSiteRoom/ProceedButton");
		if (nodeOrNull != null && nodeOrNull.IsEnabled)
		{
			AutoSlayLog.Action("Clicking rest site proceed button");
			await UiHelper.Click(nodeOrNull);
		}
	}

	private async Task ClickEventProceedIfNeeded(CancellationToken ct)
	{
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		Node eventRoom = root.GetNodeOrNull("/root/Game/RootSceneContainer/Run/RoomContainer/EventRoom");
		if (eventRoom == null)
		{
			AutoSlayLog.Info("Event room not found for proceed check");
			return;
		}
		NEventOptionButton proceedOption = null;
		await WaitHelper.Until(delegate
		{
			NMapScreen? instance = NMapScreen.Instance;
			if (instance != null && instance.IsOpen)
			{
				return true;
			}
			List<NEventOptionButton> list = (from o in UiHelper.FindAll<NEventOptionButton>(eventRoom)
				where !o.Option.IsLocked && o.Option.IsProceed
				select o).ToList();
			if (list.Count > 0)
			{
				proceedOption = list[0];
				return true;
			}
			return false;
		}, ct, TimeSpan.FromSeconds(5L), "Event proceed option or map did not appear");
		if (proceedOption != null)
		{
			AutoSlayLog.Action("Clicking event proceed option");
			await UiHelper.Click(proceedOption);
		}
		else
		{
			AutoSlayLog.Info("Map already open, no proceed needed");
		}
	}

	private async Task WaitForRewardsScreenAsync(CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for rewards screen");
		await WaitHelper.Until(() => NOverlayStack.Instance?.Peek() is NRewardsScreen || (NMapScreen.Instance?.IsOpen ?? false), ct, TimeSpan.FromSeconds(10L), "Rewards screen did not appear after combat");
	}

	private async Task WaitForGameOverScreenAsync(CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for game over screen");
		await WaitHelper.Until(() => NOverlayStack.Instance?.Peek() is NGameOverScreen, ct, TimeSpan.FromSeconds(10L), "Game over screen did not appear");
	}

	private async Task WaitForMainMenuAsync(CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for main menu");
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		await WaitHelper.Until(() => root.GetNodeOrNull<Control>("/root/Game/RootSceneContainer/MainMenu")?.IsVisibleInTree() ?? false, ct, TimeSpan.FromSeconds(30L), "Main menu did not appear after game over");
		AutoSlayLog.Action("Main menu appeared");
	}

	private async Task PlayMainMenuAsync(CancellationToken ct)
	{
		AutoSlayLog.Action("Playing main menu");
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		Control mainMenu = await WaitHelper.ForNode<Control>(root, "/root/Game/RootSceneContainer/MainMenu", ct, TimeSpan.FromSeconds(30L));
		NButton node = mainMenu.GetNode<NButton>("MainMenuTextButtons/AbandonRunButton");
		if (node.Visible)
		{
			AutoSlayLog.Action("Abandoning existing run");
			await UiHelper.Click(node);
			await WaitHelper.Until(() => NModalContainer.Instance?.OpenModal != null, ct, AutoSlayConfig.nodeWaitTimeout, "Abandon run confirmation popup did not appear");
			Node node2 = (Node)NModalContainer.Instance.OpenModal;
			NButton node3 = node2.GetNode<NButton>("VerticalPopup/YesButton");
			AutoSlayLog.Action("Confirming abandon");
			await UiHelper.Click(node3);
			await WaitHelper.Until(() => NModalContainer.Instance.OpenModal == null, ct, AutoSlayConfig.nodeWaitTimeout, "Abandon run confirmation popup did not close");
		}
		NButton node4 = mainMenu.GetNode<NButton>("MainMenuTextButtons/SingleplayerButton");
		AutoSlayLog.Action("Clicking singleplayer");
		await UiHelper.Click(node4);
		Control charSelectScreen = mainMenu.GetNodeOrNull<Control>("Submenus/CharacterSelectScreen");
		NButton standardButton = mainMenu.GetNodeOrNull<NButton>("Submenus/SingleplayerSubmenu/StandardButton");
		await WaitHelper.Until(delegate
		{
			charSelectScreen = mainMenu.GetNodeOrNull<Control>("Submenus/CharacterSelectScreen");
			standardButton = mainMenu.GetNodeOrNull<NButton>("Submenus/SingleplayerSubmenu/StandardButton");
			bool flag = charSelectScreen?.Visible ?? false;
			bool flag2 = standardButton?.Visible ?? false;
			return flag || flag2;
		}, ct, AutoSlayConfig.nodeWaitTimeout, "Neither CharacterSelectScreen nor SingleplayerSubmenu became visible");
		if (standardButton?.Visible ?? false)
		{
			Control control = charSelectScreen;
			if (control == null || !control.Visible)
			{
				AutoSlayLog.Action("Clicking standard run");
				await UiHelper.Click(standardButton);
				await WaitHelper.Until(() => mainMenu.GetNodeOrNull<Control>("Submenus/CharacterSelectScreen")?.Visible ?? false, ct, AutoSlayConfig.nodeWaitTimeout, "CharacterSelectScreen did not become visible");
				charSelectScreen = mainMenu.GetNode<Control>("Submenus/CharacterSelectScreen");
				goto IL_05e6;
			}
		}
		AutoSlayLog.Action("Skipping submenu (first run)");
		goto IL_05e6;
		IL_05e6:
		Node node5 = charSelectScreen.GetNode("CharSelectButtons/ButtonContainer");
		List<NCharacterSelectButton> list = UiHelper.FindAll<NCharacterSelectButton>(node5);
		foreach (NCharacterSelectButton item in list)
		{
			item.UnlockIfPossible();
		}
		List<NCharacterSelectButton> items = list.Where((NCharacterSelectButton b) => !b.IsLocked).ToList();
		NCharacterSelectButton nCharacterSelectButton = _random.NextItem(items);
		AutoSlayLog.Action($"Selecting character: {nCharacterSelectButton.Character.Id}");
		nCharacterSelectButton.Select();
		await Task.Delay(100, ct);
		NButton button = await WaitHelper.ForNode<NButton>(mainMenu, "Submenus/CharacterSelectScreen/ConfirmButton", ct);
		NGame.Instance.DebugSeedOverride = _seed;
		AutoSlayLog.Action("Confirming character");
		await UiHelper.Click(button);
	}

	private async Task AbandonRunAsync(CancellationToken ct)
	{
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		await Task.Delay(1000, ct);
		await UiHelper.Click(await WaitHelper.ForNode<NTopBarPauseButton>(root, "/root/Game/RootSceneContainer/Run/GlobalUi/TopBar/RightAlignedStuff/PauseButton", ct));
		NPauseMenu pauseMenu = null;
		await WaitHelper.Until(() => (pauseMenu = UiHelper.FindFirst<NPauseMenu>(root)) != null && pauseMenu.IsVisibleInTree(), ct, null, "Pause menu did not open");
		NPauseMenuButton node = pauseMenu.GetNode<Control>("%ButtonContainer").GetNode<NPauseMenuButton>("GiveUp");
		await UiHelper.Click(node);
		NAbandonRunConfirmPopup confirmPopup = null;
		await WaitHelper.Until(() => (confirmPopup = UiHelper.FindFirst<NAbandonRunConfirmPopup>(root)) != null, ct, null, "Abandon confirm popup did not appear");
		NVerticalPopup node2 = confirmPopup.GetNode<NVerticalPopup>("VerticalPopup");
		await UiHelper.Click(node2.YesButton);
		await WaitForGameOverScreenAsync(ct);
		await DrainOverlayScreensAsync(ct);
		await WaitForMainMenuAsync(ct);
	}

	private static void QuitGame(int exitCode)
	{
		AutoSlayLog.Action($"Quitting game with exit code {exitCode}");
		MegaLabel.DisposeCachedParagraph();
		MegaRichTextLabel.DisposeCachedParagraph();
		FontManager.ClearCache();
		NGame.Instance?.GetTree().Quit(exitCode);
	}
}
