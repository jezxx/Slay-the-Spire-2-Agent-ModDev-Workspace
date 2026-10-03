using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using Sentry;

namespace MegaCrit.Sts2.Core.Combat;

public class CombatManager
{
	public const int baseHandDrawCount = 5;

	/// <summary>
	/// The current combat's turn state: every piece of combat-scoped state (token, ready sets, signals, phase flags)
	/// lives on it, created in <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.SetUpCombat(MegaCrit.Sts2.Core.Combat.CombatState)" /> and dropped wholesale in <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.Reset(System.Boolean)" />. The turn loop
	/// threads its own turn state through the turn flow; everything else (deliveries, public getters) reads this
	/// field for the current combat.
	/// </summary>
	private CombatTurnState? _turnState;

	/// <summary>
	/// Set to true when the player should not be able to interact with their hand or any potions.
	/// </summary>
	private bool _playerActionsDisabled;

	private readonly Dictionary<Player, int> _cardOrPotionEffectDepth = new Dictionary<Player, int>();

	/// <summary>
	/// The most recently launched turn loop task, kept across <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.Reset(System.Boolean)" /> so the next combat's turn loop can
	/// wait for it to finish dying. This is deliberately manager-scoped: it is the one piece of state whose job
	/// is to bridge two combats.
	/// </summary>
	private Task? _turnLoopTask;

	/// <summary>
	/// Installed by <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.DebugOnlyWhenNextTurnLoopWaitsForPrevious" />, completed when a turn loop suspends inside
	/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.WaitForPreviousTurnLoopToFinish(System.Threading.Tasks.Task,MegaCrit.Sts2.Core.Combat.CombatTurnState)" />.
	/// </summary>
	private TaskCompletionSource? _turnLoopWaitingForPreviousSource;

	/// <summary>
	/// Watchdog for the previous turn loop wait in <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.RunTurnLoopAfter(System.Threading.Tasks.Task)" />. Generous: it only elapses when the
	/// previous turn loop is genuinely stuck on a suspension that teardown could not cancel, in which case we log
	/// loudly and proceed, degrading to the unsequenced behavior instead of introducing a new hang class.
	/// </summary>
	private static readonly TimeSpan _previousTurnLoopTimeout = TimeSpan.FromSeconds(10L);

	/// <summary>
	/// Whether the previous turn loop watchdog has already reported to Sentry this session. The watchdog can fire on
	/// every combat start, so it reports once and lets the log carry the rest.
	/// </summary>
	private static bool _turnLoopWaitTimeoutReported;

	/// <summary>
	/// Whether the stale player-turn-end transition in <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.AfterAllPlayersReadyToEndTurn(MegaCrit.Sts2.Core.Combat.CombatTurnState,MegaCrit.Sts2.Core.Combat.EndTurnSignal)" /> has already
	/// reported to Sentry this session. Reported once for the same reason as
	/// <see cref="F:MegaCrit.Sts2.Core.Combat.CombatManager._turnLoopWaitTimeoutReported" />.
	/// </summary>
	private static bool _staleTurnEndReported;

	public static CombatManager Instance { get; } = new CombatManager();


	/// <summary>
	/// WARNING: ONLY USE THIS IN TESTS!
	/// See <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.DebugForceTopCardOnNextShuffle(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public CardModel? DebugForcedTopCardOnNextShuffle { get; private set; }

	public bool IsPaused { get; private set; }

	public bool PlayerActionsDisabled
	{
		get
		{
			return _playerActionsDisabled;
		}
		private set
		{
			if (_playerActionsDisabled != value)
			{
				_playerActionsDisabled = value;
				this.PlayerActionsDisabledChanged?.Invoke(_turnState.State);
			}
		}
	}

	/// <summary>
	/// The list of players in the current turn that are taking an extra turn.
	/// Normally empty; only non-empty if there are players that used extra-turn-taking effects like
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Relics.PaelsEye" />.
	/// Returns a snapshot copy for thread safety.
	/// </summary>
	public IReadOnlyList<Player> PlayersTakingExtraTurn
	{
		get
		{
			CombatTurnState turnState = _turnState;
			if (turnState == null)
			{
				return Array.Empty<Player>();
			}
			using (turnState.ReadyLock.EnterScope())
			{
				return turnState.PlayersTakingExtraTurn.ToList();
			}
		}
	}

	/// <summary>
	/// True when the enemy turn has started (TurnStarted has fired for the enemy side).
	/// Set right before TurnStarted fires for enemy turns, cleared when switching to player turn.
	/// </summary>
	public bool IsEnemyTurnStarted => _turnState?.IsEnemyTurnStarted ?? false;

	/// <summary>
	/// Set to true in the time between when all players are ready to begin the enemy turn and when the enemy turn begins.
	/// </summary>
	public bool EndingPlayerTurnPhaseTwo => _turnState?.EndingPlayerTurnPhaseTwo ?? false;

	/// <summary>
	/// Set to true in the time during phase one of the end of the player's turn.
	/// </summary>
	public bool EndingPlayerTurnPhaseOne => _turnState?.EndingPlayerTurnPhaseOne ?? false;

	public CombatStateTracker StateTracker { get; }

	public CombatHistory History { get; }

	/// <summary>
	/// Is the combat currently in progress?
	/// True when the combat is done being initialized and has fully started.
	/// False when:
	/// * The combat is first being initialized.
	/// * The combat is ending (the last monster has been killed).
	/// * We're in a non-combat room.
	/// </summary>
	public bool IsInProgress => _turnState?.IsInProgress ?? false;

	/// <summary>
	/// The current combat's <see cref="T:MegaCrit.Sts2.Core.Combat.CombatId" />, or null outside combat. Capture this at the start of work that
	/// can outlive its combat (a card or potion effect, a kill) and pass it back to the entry point that acts on
	/// combat state, so that entry point can tell whether the work still belongs to the combat that is running.
	/// Reading <see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.IsInProgress" /> there instead only answers "is some combat running", which a continuation
	/// resuming into the next combat passes.
	/// </summary>
	public CombatId? CurrentCombatId => _turnState?.Id;

	/// <summary>
	/// Is a new combat currently being set up?
	/// True from the start of <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.SetUpCombat(MegaCrit.Sts2.Core.Combat.CombatState)" /> until <see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.IsInProgress" /> flips true in
	/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.StartCombatInternal(MegaCrit.Sts2.Core.Combat.CombatTurnState)" />. During this window <see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.IsInProgress" /> is still false, so it lets
	/// callers distinguish "combat is starting" (where combat hooks that run during setup, like the initial deck
	/// shuffle, must still fire) from "combat is over or ending".
	/// </summary>
	public bool IsStarting => _turnState?.IsStarting ?? false;

	/// <summary>
	/// Is combat about to end due to player death?
	/// True when LoseCombat() has been called but the loss hasn't been processed yet.
	/// This allows effects to bail out early while still letting the current action complete.
	/// </summary>
	public bool IsAboutToLose => _turnState?.PendingLoss != null;

	/// <summary>
	/// Is the combat in the process of ending (but still in progress)?
	/// True when combat is in progress but all the enemies are dead, and there is nothing stopping combat from ending
	/// (e.g. Phrog Parasite spawning in new enemies).
	/// Also true when a pending loss is waiting to be processed.
	/// False when
	/// * Combat is in progress and 1+ primary enemies are still alive.
	/// * Combat is not in progress.
	/// </summary>
	public bool IsEnding
	{
		get
		{
			CombatTurnState turnState = _turnState;
			if (turnState != null)
			{
				return IsCombatEnding(turnState);
			}
			return false;
		}
	}

	/// <summary>
	/// Has this combat ended (or is it in the process of ending)?
	/// When you want to skip/cancel an effect because combat is not in progress, you should usually use this instead of
	/// <see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.IsEnding" /> or !<see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.IsInProgress" />, because they can return unexpected results at certain
	/// boundary points.
	/// </summary>
	public bool IsOverOrEnding
	{
		get
		{
			if (!IsEnding)
			{
				return !IsInProgress;
			}
			return true;
		}
	}

	/// <summary>
	/// WARNING: ONLY USE THIS IN TESTS!
	/// The task that currently owns the turn loop. Exposed so a test can hold the outgoing turn loop across a
	/// teardown and read whether it is still alive, instead of inferring that from log output.
	/// </summary>
	internal Task? DebugOnlyCurrentTurnLoopTask => _turnLoopTask;

	/// <summary>
	/// Fired after combat is set up.
	/// Note that this happens a little bit before combat actually begins.
	/// </summary>
	public event Action<CombatState>? CombatSetUp;

	/// <summary>
	/// Fired whenever a new combat begins, after IsInProgress is set to true.
	/// </summary>
	public event Action<CombatState>? CombatBegan;

	/// <summary>
	/// Fired when combat ends.
	/// </summary>
	public event Action<CombatRoom>? CombatEnded;

	/// <summary>
	/// Fired when combat is won.
	/// </summary>
	public event Action<CombatRoom>? CombatWon;

	/// <summary>
	/// Fired whenever the arrangement of creatures in the combat changes. Specifically, when:
	/// * A creature is added.
	/// * A creature is removed.
	/// * A creature's position changes.
	/// </summary>
	public event Action<CombatState>? CreaturesChanged;

	/// <summary>
	/// Fired whenever a new turn starts.
	/// </summary>
	public event Action<CombatState>? TurnStarted;

	/// <summary>
	/// Fired whenever a turn ends.
	/// </summary>
	public event Action<CombatState>? TurnEnded;

	/// <summary>
	/// Fired whenever a player ends their turn. Remember that, in multiplayer, this is not the same as switching to the
	/// enemy's turn.
	/// </summary>
	public event Action<Player, bool>? PlayerEndedTurn;

	/// <summary>
	/// Fired whenever a player un-does the end of their turn.
	/// </summary>
	public event Action<Player>? PlayerUnendedTurn;

	/// <summary>
	/// Fired when all players have fully committed to ending turn and all player actions are done (including end of turn
	/// hooks like Well-Laid Plans), but before the player hand flush.
	/// </summary>
	public event Action<CombatState>? AboutToSwitchToEnemyTurn;

	/// <summary>
	/// Fired when the local player's actions become disabled or enabled.
	/// </summary>
	public event Action<CombatState>? PlayerActionsDisabledChanged;

	/// <summary>
	/// THIS IS TEMPORARY AND SHOULD ONLY BE USED IN TESTS
	/// </summary>
	/// <returns></returns>
	public CombatState? DebugOnlyGetState()
	{
		return _turnState?.State;
	}

	/// <summary>
	/// Sets <see cref="P:MegaCrit.Sts2.Core.Entities.Players.PlayerCombatState.Phase" /> to the same value for all players of the given combat.
	/// </summary>
	private static void SetPhaseForAllPlayers(CombatState state, PlayerTurnPhase phase)
	{
		foreach (Player player in state.Players)
		{
			if (player.PlayerCombatState != null)
			{
				player.PlayerCombatState.Phase = phase;
			}
		}
	}

	/// <summary>
	/// The live turn state, if it is in progress and owns <paramref name="combatId" />. Null when there is no combat,
	/// when that combat is not in progress, or when <paramref name="combatId" /> belongs to an earlier combat, which is
	/// the case that matters: work captured in combat A that resumes after combat B has started.
	///
	/// Deliberately does not also test <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.IsCombatEnding(MegaCrit.Sts2.Core.Combat.CombatTurnState)" />. The callers guarded on
	/// <see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.IsInProgress" /> before they took a <see cref="T:MegaCrit.Sts2.Core.Combat.CombatId" />, so adding the ending check here would be
	/// a behavior change smuggled in as an id change; and the ending window is already covered where it does work,
	/// since a hand check's only effect is a hook and <c>Hook.IterateCombatHookListeners</c> drops combat hook
	/// dispatch once combat is ending.
	/// </summary>
	private CombatTurnState? LiveTurnStateFor(CombatId? combatId)
	{
		CombatTurnState turnState = _turnState;
		if (turnState == null || !turnState.IsInProgress)
		{
			return null;
		}
		if (!(combatId == turnState.Id))
		{
			return null;
		}
		return turnState;
	}

	/// <summary>
	/// Is <paramref name="combatId" /> the combat that is running right now? Use this from work that can be suspended
	/// past the end of its own combat, to drop the work rather than apply it to the next combat.
	///
	/// Testing the id alone is not enough. A combat that has ended keeps its <see cref="T:MegaCrit.Sts2.Core.Combat.CombatId" /> until
	/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.Reset(System.Boolean)" /> runs, because <c>EndCombatInternal</c> only clears
	/// <see cref="P:MegaCrit.Sts2.Core.Combat.CombatTurnState.IsInProgress" />, so in that window the id still matches. This checks both.
	/// </summary>
	public bool IsCurrentLiveCombat(CombatId? combatId)
	{
		return LiveTurnStateFor(combatId) != null;
	}

	/// <summary>
	/// True while a <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.OnPlay(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" /> or a <see cref="M:MegaCrit.Sts2.Core.Models.PotionModel.OnUse(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Creatures.Creature)" /> effect body is currently
	/// executing for <paramref name="player" />, including nested auto-plays (e.g. a Sly card auto-played when
	/// discarded). Used to avoid premature hand-empty triggers while that player's effect is mid-resolution.
	/// </summary>
	public bool IsExecutingCardOrPotionEffect(Player player)
	{
		return _cardOrPotionEffectDepth.GetValueOrDefault(player) > 0;
	}

	/// <summary>
	/// Marks the start of a <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.OnPlay(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" /> or <see cref="M:MegaCrit.Sts2.Core.Models.PotionModel.OnUse(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Creatures.Creature)" /> effect body for
	/// <paramref name="player" />, incrementing their effect-nesting depth. Must be paired with a
	/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.EndCardOrPotionEffect(System.Nullable{MegaCrit.Sts2.Core.Combat.CombatId},MegaCrit.Sts2.Core.Entities.Players.Player)" /> in a finally block so the depth stays balanced even if the effect throws or
	/// the player dies mid-play.
	///
	/// Returns the <see cref="T:MegaCrit.Sts2.Core.Combat.CombatId" /> the effect is starting in. Hold it for the rest of the play or use, and pass
	/// it to the combat entry points called afterwards: an effect can be suspended past the end of its own combat, and
	/// those entry points need to know which combat the work belongs to rather than which one is running now. It is
	/// returned from here, rather than read at the point of use, so the capture cannot drift later than the effect's
	/// actual start.
	/// </summary>
	public CombatId? BeginCardOrPotionEffect(Player player)
	{
		_cardOrPotionEffectDepth[player] = _cardOrPotionEffectDepth.GetValueOrDefault(player) + 1;
		return CurrentCombatId;
	}

	/// <summary>
	/// Marks the end of a <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.OnPlay(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" /> or <see cref="M:MegaCrit.Sts2.Core.Models.PotionModel.OnUse(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Creatures.Creature)" /> effect body for
	/// <paramref name="player" />, decrementing their effect-nesting depth (and removing the entry once it reaches
	/// zero). Pairs with <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.BeginCardOrPotionEffect(MegaCrit.Sts2.Core.Entities.Players.Player)" />; call it from a finally block.
	/// </summary>
	public async Task EndCardOrPotionEffect(CombatId? combatId, Player player)
	{
		int num = _cardOrPotionEffectDepth.GetValueOrDefault(player) - 1;
		if (num <= 0)
		{
			_cardOrPotionEffectDepth.Remove(player);
			if (player.Creature.IsDead)
			{
				await RemoveDeadPlayerCardsFromCombat(combatId, player);
			}
		}
		else
		{
			_cardOrPotionEffectDepth[player] = num;
		}
	}

	/// <summary>
	/// Turn-state-relative variant for the turn loop: evaluated against the turn loop's own combat, so a stale turn state
	/// resuming here cannot end (or fail to end) the current combat by mistake.
	/// </summary>
	private bool IsCombatEnding(CombatTurnState turnState)
	{
		if (!turnState.IsInProgress)
		{
			return false;
		}
		if (turnState.PendingLoss != null)
		{
			return true;
		}
		if (turnState.State.Enemies.Any((Creature e) => e != null && e.IsAlive && e.IsPrimaryEnemy))
		{
			return false;
		}
		if (Hook.ShouldStopCombatFromEnding(turnState.State))
		{
			return false;
		}
		return true;
	}

	private CombatManager()
	{
		History = new CombatHistory();
		StateTracker = new CombatStateTracker(this);
	}

	public void SetUpCombat(CombatState state)
	{
		if (_turnState != null)
		{
			throw new InvalidOperationException("Make sure to reset the combat before setting up a new one.");
		}
		CombatTurnState combatTurnState = (_turnState = new CombatTurnState(state));
		Log.Debug($"Combat #{combatTurnState.Id} created for encounter {state.Encounter?.Id.Entry}");
		state.MultiplayerScalingModel?.OnCombatEntered(state);
		StateTracker.SetState(state);
		foreach (Player player in state.Players)
		{
			player.ResetCombatState();
		}
		foreach (Player player2 in state.Players)
		{
			player2.PopulateCombatState(player2.RunState.Rng.Shuffle, state);
		}
		NetCombatCardDb.Instance.StartCombat(state.Players);
		foreach (Creature creature in state.Creatures)
		{
			AddCreature(creature);
		}
		this.CombatSetUp?.Invoke(state);
	}

	public void AfterCombatRoomLoaded()
	{
		RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.PreCombatSetup);
		Task turnLoopTask = _turnLoopTask;
		TaskHelper.RunSafely(_turnLoopTask = RunTurnLoopAfter(turnLoopTask));
	}

	/// <summary>
	/// WARNING: ONLY USE THIS IN TESTS!
	/// Returns a task that completes the next time a combat turn loop suspends inside
	/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.WaitForPreviousTurnLoopToFinish(System.Threading.Tasks.Task,MegaCrit.Sts2.Core.Combat.CombatTurnState)" />, for tests that need to catch a turn loop held at that wait.
	/// When that happens is not predictable from outside: a restart tears the old combat down synchronously, then
	/// loads a room before the next turn loop launches, so a test has nothing else to wait on.
	/// Call this before starting the next combat, so the signal is in place before its turn loop launches. Each call
	/// installs a fresh signal, so an earlier combat's turn loop cannot complete it.
	/// A turn loop that skips the wait never suspends, so the task never completes. That is what a missing wait
	/// looks like, so bound the wait and report the timeout as that failure.
	/// </summary>
	internal Task DebugOnlyWhenNextTurnLoopWaitsForPrevious()
	{
		return (_turnLoopWaitingForPreviousSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)).Task;
	}

	/// <summary>
	/// Runs this combat's turn loop (<see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.StartCombatInternal(MegaCrit.Sts2.Core.Combat.CombatTurnState)" />) once <paramref name="previousTurnLoopTask" />,
	/// the previous combat's turn loop, has finished dying, and reports how it died in turn. Cancellation death
	/// is the normal teardown path and logs at debug; anything else while the combat is still running means the
	/// combat has lost its only turn loop and is stuck until the room is restarted, which must be loudly attributable.
	/// </summary>
	private async Task RunTurnLoopAfter(Task? previousTurnLoopTask)
	{
		CombatTurnState turnState = _turnState;
		try
		{
			await WaitForPreviousTurnLoopToFinish(previousTurnLoopTask, turnState);
			if (turnState != null)
			{
				Log.Debug($"Combat #{turnState.Id} turn loop started");
				await StartCombatInternal(turnState);
			}
		}
		catch (OperationCanceledException) when (turnState == null || !turnState.IsLive)
		{
			Log.Debug($"Combat #{turnState?.Id} turn loop died of cancellation (combat torn down)");
			throw;
		}
		catch (Exception ex2)
		{
			Exception e = ex2;
			if (turnState != null && turnState.IsLive && _turnState == turnState)
			{
				Log.Error($"Combat #{turnState.Id} turn loop died while its combat is in progress; the combat is stuck until the room is restarted: {e}");
				SentryService.CaptureException(new StuckCombatException("Combat turn loop died while its combat was in progress", e), delegate(Scope scope)
				{
					scope.SetFingerprint("StuckCombat", e.GetType().Name);
					scope.SetExtra("combatId", turnState.Id.Value);
				});
			}
			throw;
		}
	}

	/// <summary>
	/// Sequences this combat's start against the previous combat's turn loop as it finishes. The previous turn loop was
	/// cancelled when its combat was torn down and usually died synchronously inside that cancel, but a turn loop
	/// resumed by an external completion (a frame poll, a late-completing action, a test hook) can outlive teardown
	/// briefly; starting the next combat under it would let a stale turn loop continuation interleave with the new
	/// combat's setup.
	/// </summary>
	private async Task WaitForPreviousTurnLoopToFinish(Task? previousTurnLoopTask, CombatTurnState? turnState)
	{
		CombatTurnState turnState2 = turnState;
		if (previousTurnLoopTask == null || previousTurnLoopTask.IsCompleted)
		{
			return;
		}
		Log.Debug($"Combat #{turnState2?.Id} turn loop waiting for the previous combat's turn loop to die");
		_turnLoopWaitingForPreviousSource?.TrySetResult();
		try
		{
			await previousTurnLoopTask.WaitAsync(_previousTurnLoopTimeout);
		}
		catch (TimeoutException)
		{
			string text = $"The previous combat's turn loop is still running {_previousTurnLoopTimeout.TotalSeconds:0}s after its combat was torn down; starting combat #{turnState2?.Id} without waiting for it.";
			Log.Error(text);
			if (!_turnLoopWaitTimeoutReported)
			{
				_turnLoopWaitTimeoutReported = true;
				SentryService.CaptureMessage("Combat turn loop wait timed out; started a combat without waiting for the previous turn loop", SentryLevel.Error, delegate(Scope scope)
				{
					scope.SetExtra("combatId", turnState2?.Id.Value ?? (-1));
				});
			}
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// The turn loop body. Turn-state-threaded like the rest of the turn loop call graph: the turn state is the one
	/// captured when the turn loop was created, never re-read from <see cref="F:MegaCrit.Sts2.Core.Combat.CombatManager._turnState" />. A turn loop that was
	/// queued behind a slow predecessor (see <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.WaitForPreviousTurnLoopToFinish(System.Threading.Tasks.Task,MegaCrit.Sts2.Core.Combat.CombatTurnState)" />) can start after its
	/// combat was already torn down and replaced; reading the live field here would point that stale turn loop at the
	/// current combat and drive it alongside its real turn loop.
	/// </summary>
	private async Task StartCombatInternal(CombatTurnState turnState)
	{
		turnState.Ct.ThrowIfCancellationRequested();
		RunManager.Instance.ActionExecutor.Unpause();
		await RunManager.Instance.ActionExecutor.FinishedExecutingActions();
		RunManager.Instance.ActionExecutor.Pause();
		if (turnState.State.Encounter.HasBgm)
		{
			NRunMusicController.Instance?.PlayCustomMusic(turnState.State.Encounter.CustomBgm);
		}
		foreach (Creature creature in turnState.State.Creatures)
		{
			await AfterCreatureAdded(creature, turnState.State);
			turnState.Ct.ThrowIfCancellationRequested();
		}
		RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.NotPlayPhase);
		turnState.IsInProgress = true;
		turnState.IsStarting = false;
		await Hook.BeforeCombatStart(turnState.State.RunState, turnState.State);
		turnState.Ct.ThrowIfCancellationRequested();
		this.CombatBegan?.Invoke(turnState.State);
		NRunMusicController.Instance?.UpdateTrack();
		NCombatRulesFtue ftue = null;
		if (SaveManager.Instance.SeenFtue("combat_rules_ftue"))
		{
			NCombatRoom.Instance?.AddChildSafely(NCombatStartBanner.Create());
		}
		else
		{
			ftue = NCombatRulesFtue.Create();
			NModalContainer.Instance?.Add(ftue, showBackstop: false);
		}
		await Cmd.CustomScaledWait(0.5f, 1f);
		turnState.Ct.ThrowIfCancellationRequested();
		await StartTurn(turnState);
		ftue?.Start();
		Func<Task> actionDuringEnemyTurn = await AwaitTurnEndAndSwitchSides(turnState);
		while (turnState.IsLive)
		{
			if (turnState.State.CurrentSide == CombatSide.Player)
			{
				await StartTurn(turnState);
				actionDuringEnemyTurn = await AwaitTurnEndAndSwitchSides(turnState);
			}
			else
			{
				await StartTurn(turnState, actionDuringEnemyTurn);
				actionDuringEnemyTurn = null;
			}
		}
	}

	/// <summary>
	/// The player-turn half of the turn loop, called by the turn loop with the player turn set up (or combat
	/// over/torn down, which the guards absorb): suspends awaiting the all-players-ready-to-end-turn signal, runs
	/// end-turn phase one, suspends awaiting the all-players-ready-to-begin-enemy-turn signal, then runs phase two,
	/// which switches sides. Returns the optional test hook carried by the local ready action, for the turn loop to
	/// run during the enemy turn.
	/// </summary>
	private async Task<Func<Task>?> AwaitTurnEndAndSwitchSides(CombatTurnState turnState)
	{
		if (!turnState.IsInProgress)
		{
			return null;
		}
		if (turnState.State.CurrentSide != CombatSide.Player)
		{
			return null;
		}
		TaskCompletionSource<EndTurnSignal> endTurnSignalSource;
		TaskCompletionSource<Func<Task>?> beginEnemyTurnSignalSource;
		using (turnState.ReadyLock.EnterScope())
		{
			endTurnSignalSource = turnState.EndTurnSignalSource;
			beginEnemyTurnSignalSource = turnState.BeginEnemyTurnSignalSource;
		}
		if (endTurnSignalSource == null || beginEnemyTurnSignalSource == null)
		{
			return null;
		}
		EndTurnSignal endTurnSignal = await endTurnSignalSource.Task;
		if (!turnState.IsInProgress)
		{
			return null;
		}
		turnState.Ct.ThrowIfCancellationRequested();
		if (endTurnSignal.RunningAction != null)
		{
			try
			{
				await endTurnSignal.RunningAction.CompletionTask.WaitAsync(turnState.Ct);
			}
			catch (OperationCanceledException) when (!turnState.Ct.IsCancellationRequested)
			{
			}
		}
		await AfterAllPlayersReadyToEndTurn(turnState, endTurnSignal);
		if (!turnState.IsInProgress)
		{
			return null;
		}
		turnState.Ct.ThrowIfCancellationRequested();
		Func<Task> actionDuringEnemyTurn = await beginEnemyTurnSignalSource.Task;
		if (!turnState.IsInProgress)
		{
			return null;
		}
		turnState.Ct.ThrowIfCancellationRequested();
		await AfterAllPlayersReadyToBeginEnemyTurn(turnState);
		return actionDuringEnemyTurn;
	}

	private async Task StartTurn(CombatTurnState turnState, Func<Task>? actionDuringEnemyTurn = null)
	{
		if (!turnState.IsInProgress)
		{
			return;
		}
		turnState.Ct.ThrowIfCancellationRequested();
		SetPhaseForAllPlayers(turnState.State, PlayerTurnPhase.None);
		bool isExtraPlayerTurn;
		List<Creature> creaturesStartingTurn;
		List<Player> playersStartingTurn;
		using (turnState.ReadyLock.EnterScope())
		{
			isExtraPlayerTurn = turnState.PlayersTakingExtraTurn.Count > 0;
			if (turnState.State.CurrentSide == CombatSide.Player && isExtraPlayerTurn)
			{
				creaturesStartingTurn = turnState.PlayersTakingExtraTurn.Select((Player p) => p.Creature).ToList();
				playersStartingTurn = turnState.PlayersTakingExtraTurn.ToList();
			}
			else
			{
				creaturesStartingTurn = turnState.State.CreaturesOnCurrentSide.ToList();
				playersStartingTurn = ((turnState.State.CurrentSide == CombatSide.Player) ? turnState.State.Players.ToList() : new List<Player>());
			}
		}
		foreach (Creature item2 in creaturesStartingTurn)
		{
			if (turnState.Ct.IsCancellationRequested)
			{
				return;
			}
			item2.BeforeTurnStart(turnState.State.CurrentSide);
		}
		await Hook.BeforeSideTurnStart(turnState.State, turnState.State.CurrentSide, creaturesStartingTurn);
		turnState.Ct.ThrowIfCancellationRequested();
		if (turnState.State.CurrentSide == CombatSide.Player)
		{
			SetPhaseForAllPlayers(turnState.State, PlayerTurnPhase.Start);
			PlayerActionsDisabled = false;
			using (turnState.ReadyLock.EnterScope())
			{
				turnState.PlayersReadyToEndTurn.Clear();
				turnState.PlayersReadyToBeginEnemyTurn.Clear();
				turnState.EndTurnSignalSource = new TaskCompletionSource<EndTurnSignal>();
				turnState.BeginEnemyTurnSignalSource = new TaskCompletionSource<Func<Task>>();
			}
			int valueOrDefault = (LocalContext.GetMe(playersStartingTurn)?.PlayerCombatState?.TurnNumber).GetValueOrDefault(-1);
			if (valueOrDefault > 1)
			{
				NCombatRoom.Instance?.AddChildSafely(NPlayerTurnBanner.Create(valueOrDefault));
			}
			if (!isExtraPlayerTurn)
			{
				foreach (Creature enemy in turnState.State.Enemies)
				{
					enemy.PrepareForNextTurn(turnState.State.PlayerCreatures);
				}
			}
		}
		else
		{
			NCombatRoom.Instance?.AddChildSafely(NEnemyTurnBanner.Create());
		}
		await Cmd.CustomScaledWait(0.5f, 0.8f);
		turnState.Ct.ThrowIfCancellationRequested();
		foreach (Creature item3 in creaturesStartingTurn)
		{
			if (turnState.Ct.IsCancellationRequested)
			{
				return;
			}
			await item3.AfterTurnStart(turnState.State.CurrentSide);
			turnState.Ct.ThrowIfCancellationRequested();
		}
		foreach (Creature item4 in creaturesStartingTurn)
		{
			if (turnState.Ct.IsCancellationRequested)
			{
				return;
			}
			await Hook.AfterBlockCleared(turnState.State, item4);
			turnState.Ct.ThrowIfCancellationRequested();
		}
		List<(HookPlayerChoiceContext, Task)> setupPlayerTurnContext = new List<(HookPlayerChoiceContext, Task)>();
		foreach (Player item5 in playersStartingTurn)
		{
			if (LocalContext.NetId.HasValue)
			{
				HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item5, LocalContext.NetId.Value, GameActionType.CombatPlayPhaseOnly);
				Task task = SetupPlayerTurn(turnState, item5, playerChoiceContext);
				await playerChoiceContext.WaitForPauseOrCompletionWithoutAssigningTask(task);
				turnState.Ct.ThrowIfCancellationRequested();
				setupPlayerTurnContext.Add((playerChoiceContext, task));
			}
		}
		await Hook.AfterSideTurnStart(turnState.State, turnState.State.CurrentSide, creaturesStartingTurn);
		turnState.Ct.ThrowIfCancellationRequested();
		if (turnState.State.CurrentSide == CombatSide.Player)
		{
			foreach (Player item6 in playersStartingTurn)
			{
				if (item6.PlayerCombatState != null && LocalContext.NetId.HasValue)
				{
					HookPlayerChoiceContext hookPlayerChoiceContext = new HookPlayerChoiceContext(item6, LocalContext.NetId.Value, GameActionType.CombatPlayPhaseOnly);
					Task task2 = item6.PlayerCombatState.OrbQueue.AfterTurnStart(hookPlayerChoiceContext);
					await hookPlayerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task2);
					turnState.Ct.ThrowIfCancellationRequested();
				}
			}
			RunManager.Instance.ChecksumTracker.GenerateChecksum("After player turn start", null);
			if (turnState.Ct.IsCancellationRequested)
			{
				return;
			}
			foreach (Player player2 in turnState.State.Players)
			{
				if (player2.Creature.IsDead || !playersStartingTurn.Contains(player2))
				{
					Log.Info($"Setting player {player2.NetId} to ready at start of turn. IsDead: {player2.Creature.IsDead}. IsStartingTurn: {playersStartingTurn.Contains(player2)}");
					SetReadyToEndTurn(player2, canBackOut: false);
					if (AllPlayersReadyToEndTurn(turnState))
					{
						return;
					}
				}
			}
			foreach (var item7 in playersStartingTurn.Zip(setupPlayerTurnContext))
			{
				(HookPlayerChoiceContext, Task) item = item7.Second;
				var (player, _) = item7;
				var (hookPlayerChoiceContext2, setupPlayerTurnTask) = item;
				if (turnState.Ct.IsCancellationRequested)
				{
					return;
				}
				if (!player.Creature.IsDead)
				{
					Task task3 = RunAutoPrePlayPhase(turnState, hookPlayerChoiceContext2, setupPlayerTurnTask, player);
					await hookPlayerChoiceContext2.AssignTaskAndWaitForPauseOrCompletion(task3);
					turnState.Ct.ThrowIfCancellationRequested();
				}
			}
			await CheckWinCondition(turnState);
			turnState.Ct.ThrowIfCancellationRequested();
			if (turnState.IsInProgress)
			{
				RunManager.Instance.ActionExecutor.Unpause();
				RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.PlayPhase);
				turnState.IsEnemyTurnStarted = false;
				this.TurnStarted?.Invoke(turnState.State);
			}
		}
		else
		{
			turnState.IsEnemyTurnStarted = true;
			this.TurnStarted?.Invoke(turnState.State);
			RunManager.Instance.ChecksumTracker.GenerateChecksum("After enemy turn start", null);
			await WaitForUnpause(turnState);
			turnState.Ct.ThrowIfCancellationRequested();
			await CheckWinCondition(turnState);
			turnState.Ct.ThrowIfCancellationRequested();
			if (turnState.IsInProgress)
			{
				await ExecuteEnemyTurn(turnState, actionDuringEnemyTurn);
			}
		}
	}

	/// <summary>
	/// Awaits the player's setup task, then runs the auto-pre-play hooks, transitioning the player's phase
	/// from <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.Start" /> -&gt; <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPrePlay" /> -&gt; <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.Play" />.
	/// The setup await ensures a player whose setup is paused (making a <see cref="T:MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext" />)
	/// stays in <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.Start" /> until their setup actually completes.
	/// </summary>
	private async Task RunAutoPrePlayPhase(CombatTurnState turnState, HookPlayerChoiceContext playerChoiceContext, Task setupPlayerTurnTask, Player player)
	{
		await setupPlayerTurnTask;
		player.PlayerCombatState.Phase = PlayerTurnPhase.AutoPrePlay;
		await CheckForEmptyHand(turnState, playerChoiceContext, player);
		await Hook.AfterAutoPrePlayPhaseEntered(playerChoiceContext, turnState.State, player);
		player.PlayerCombatState.Phase = PlayerTurnPhase.Play;
	}

	/// <summary>
	/// Sets up a player's turn by resetting energy, drawing cards, and firing start-of-turn hooks.
	/// If the player's turn start executes a player choice (e.g. Mayhem plays Cosmic Indifference), then the entire
	/// sequence is paused for this player. However, other players' turn start sequences may continue, and they may
	/// play cards while this is occuring.
	/// </summary>
	/// <param name="turnState">The turn state of the combat this turn belongs to.</param>
	/// <param name="player">The player whose turn to setup.</param>
	/// <param name="playerChoiceContext">The player choice context to pass to hooks that take it.</param>
	private async Task SetupPlayerTurn(CombatTurnState turnState, Player player, HookPlayerChoiceContext playerChoiceContext)
	{
		if (player.Creature.IsDead)
		{
			return;
		}
		if (player.PlayerCombatState == null)
		{
			Log.Warn($"Player combat state is null. Assuming that the run has been cleaned up. (Player: {player.NetId})");
			return;
		}
		CombatState state = turnState.State;
		if (Hook.ShouldPlayerResetEnergy(state, player))
		{
			SfxCmd.Play("event:/sfx/ui/gain_energy");
			player.PlayerCombatState.ResetEnergy();
		}
		else
		{
			player.PlayerCombatState.AddMaxEnergyToCurrent();
		}
		await Hook.AfterEnergyReset(state, player);
		turnState.Ct.ThrowIfCancellationRequested();
		await Hook.BeforeHandDraw(state, player, playerChoiceContext);
		turnState.Ct.ThrowIfCancellationRequested();
		decimal handDraw2 = Hook.ModifyHandDraw(state, player, 5m, out IEnumerable<AbstractModel> modifiers);
		await Hook.AfterModifyingHandDraw(state, modifiers);
		turnState.Ct.ThrowIfCancellationRequested();
		if (player.PlayerCombatState.TurnNumber == 1)
		{
			CardPile pile = PileType.Draw.GetPile(player);
			List<CardModel> list = pile.Cards.Where((CardModel c) => c.Enchantment?.ShouldStartAtBottomOfDrawPile ?? false).ToList();
			foreach (CardModel item in list)
			{
				pile.MoveToBottomInternal(item);
			}
			List<CardModel> list2 = pile.Cards.Where((CardModel c) => c.Keywords.Contains(CardKeyword.Innate)).Except(list).ToList();
			foreach (CardModel item2 in list2)
			{
				pile.MoveToTopInternal(item2);
			}
			handDraw2 = Math.Max(handDraw2, list2.Count);
			handDraw2 = Math.Min(handDraw2, CardPile.MaxCardsInHand);
		}
		await CardPileCmd.Draw(playerChoiceContext, handDraw2, player, fromHandDraw: true);
		turnState.Ct.ThrowIfCancellationRequested();
		await Hook.AfterPlayerTurnStart(state, playerChoiceContext, player);
	}

	/// <summary>
	/// Called in EndPlayerTurnAction to indicate that the player is ready to execute end-of-turn events.
	/// </summary>
	/// <param name="player">The player that readied up.</param>
	/// <param name="canBackOut">In multiplayer, notes if the player is allowed to back out of ending their turn.</param>
	/// <param name="actionDuringEnemyTurn">Optional action to execute during the enemy turn. This is useful for tests.</param>
	public void SetReadyToEndTurn(Player player, bool canBackOut, Func<Task>? actionDuringEnemyTurn = null)
	{
		CombatTurnState turnState = _turnState;
		if (turnState == null)
		{
			return;
		}
		using (turnState.ReadyLock.EnterScope())
		{
			if (turnState.PlayersReadyToEndTurn.Contains(player))
			{
				return;
			}
			turnState.PlayersReadyToEndTurn.Add(player);
		}
		this.PlayerEndedTurn?.Invoke(player, canBackOut);
		if (AllPlayersReadyToEndTurn(turnState))
		{
			Log.Debug("All players ready to end turn");
			GameAction gameAction = RunManager.Instance.ActionExecutor.CurrentlyRunningAction;
			if (gameAction != null && !ActionQueueSet.IsGameActionPlayerDriven(gameAction))
			{
				gameAction = null;
			}
			int scheduledTurnNumber = player.PlayerCombatState?.TurnNumber ?? (-1);
			TaskCompletionSource<EndTurnSignal> endTurnSignalSource;
			using (turnState.ReadyLock.EnterScope())
			{
				endTurnSignalSource = turnState.EndTurnSignalSource;
			}
			endTurnSignalSource?.TrySetResult(new EndTurnSignal(gameAction, scheduledTurnNumber, player, actionDuringEnemyTurn));
		}
	}

	public void UndoReadyToEndTurn(Player player)
	{
		CombatTurnState turnState = _turnState;
		if (turnState != null)
		{
			using (turnState.ReadyLock.EnterScope())
			{
				turnState.PlayersReadyToEndTurn.Remove(player);
			}
		}
		if (LocalContext.IsMe(player))
		{
			PlayerActionsDisabled = false;
		}
		this.PlayerUnendedTurn?.Invoke(player);
	}

	/// <summary>
	/// Call this when the end turn button is pressed to disable local player actions until the start of the next turn.
	/// In multiplayer, this prevents the player from playing cards after they have ended turn.
	/// In both SP and MP, this prevents the player from playing cards before the AfterTurnStart hook has run.
	/// It's important that we do this when the end turn button is pressed, instead of when the EndTurnAction is
	/// processed, because the player might try to execute actions while the end turn action is waiting in the queue.
	/// This is a little fragile; if actions do slip through in MP, it has the potential to cause a state divergence.
	/// Revisit if needed - we might need to discard actions on the host side (which ends up being way more complicated).
	/// </summary>
	public void OnEndedTurnLocally()
	{
		PlayerActionsDisabled = true;
	}

	/// <summary>
	/// Called in ReadyToBeginEnemyTurnAction to indicate that the player is ready to switch to the monster turn (or
	/// extra player turn, if necessary). Note that this is called automatically, and is not player-driven.
	/// </summary>
	/// <param name="player">The player that is ready to switch sides.</param>
	/// <param name="actionDuringEnemyTurn">Optional action to execute during the enemy turn. This is useful for tests.</param>
	public void SetReadyToBeginEnemyTurn(Player player, Func<Task>? actionDuringEnemyTurn = null)
	{
		CombatTurnState turnState = _turnState;
		if (turnState == null || !turnState.IsInProgress)
		{
			Log.Error("Trying to set player ready to begin enemy turn, but combat is over!");
			return;
		}
		bool flag2;
		TaskCompletionSource<Func<Task>> beginEnemyTurnSignalSource;
		using (turnState.ReadyLock.EnterScope())
		{
			if (!turnState.PlayersReadyToBeginEnemyTurn.Add(player))
			{
				return;
			}
			bool flag = turnState.State.CurrentSide == CombatSide.Player;
			flag2 = (turnState.PlayersReadyToBeginEnemyTurn.Count == turnState.State.Players.Count && flag) || (flag && RunManager.Instance.NetService.Type == NetGameType.Singleplayer);
			beginEnemyTurnSignalSource = turnState.BeginEnemyTurnSignalSource;
		}
		if (flag2 && beginEnemyTurnSignalSource != null && !beginEnemyTurnSignalSource.TrySetResult(actionDuringEnemyTurn))
		{
			Log.Warn($"Ignoring ready-to-begin-enemy-turn for player {player.NetId}: a player-to-enemy transition has already been claimed for this turn.");
		}
	}

	/// <returns>
	/// True if the passed player has hit the end turn button, and the next player turn has not yet begun.
	/// </returns>
	public bool IsPlayerReadyToEndTurn(Player player)
	{
		CombatTurnState turnState = _turnState;
		if (turnState == null)
		{
			return false;
		}
		using (turnState.ReadyLock.EnterScope())
		{
			return turnState.PlayersReadyToEndTurn.Contains(player);
		}
	}

	public bool AllPlayersReadyToEndTurn()
	{
		CombatTurnState turnState = _turnState;
		if (turnState != null)
		{
			return AllPlayersReadyToEndTurn(turnState);
		}
		return false;
	}

	private bool AllPlayersReadyToEndTurn(CombatTurnState turnState)
	{
		bool flag;
		using (turnState.ReadyLock.EnterScope())
		{
			flag = turnState.PlayersReadyToEndTurn.Count == turnState.State.Players.Count;
		}
		if (!RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
		{
			if (flag)
			{
				return turnState.State.CurrentSide == CombatSide.Player;
			}
			return false;
		}
		return true;
	}

	private async Task EndEnemyTurn(CombatTurnState turnState)
	{
		if (turnState.IsInProgress)
		{
			turnState.Ct.ThrowIfCancellationRequested();
			if (turnState.State.CurrentSide != CombatSide.Enemy)
			{
				throw new InvalidOperationException($"EndEnemyTurn called while the current side is {turnState.State.CurrentSide}!");
			}
			await WaitForUnpause(turnState);
			turnState.Ct.ThrowIfCancellationRequested();
			await EndEnemyTurnInternal(turnState);
			turnState.Ct.ThrowIfCancellationRequested();
			await CheckWinCondition(turnState);
			if (!IsCombatEnding(turnState))
			{
				SwitchSides(turnState);
				await WaitForUnpause(turnState);
				turnState.Ct.ThrowIfCancellationRequested();
			}
		}
	}

	public void AddCreature(Creature creature)
	{
		CombatState state = _turnState.State;
		if (!state.ContainsCreature(creature))
		{
			throw new InvalidOperationException("CombatState must already contain creature.");
		}
		creature.Monster?.SetUpForCombat();
		if (creature.SlotName != null)
		{
			state.SortEnemiesBySlotName();
		}
		StateTracker.Subscribe(creature);
		this.CreaturesChanged?.Invoke(state);
	}

	/// <summary>
	/// Called after both the Creature has been added to the room _and_ the NCreature is spawned.
	/// </summary>
	/// <param name="creature"></param>
	public Task AfterCreatureAdded(Creature creature)
	{
		return AfterCreatureAdded(creature, _turnState.State);
	}

	/// <summary>
	/// Turn-state-relative variant for the turn loop's setup loop: reads the turn loop's own combat, so a setup continuation
	/// resuming after its combat was torn down cannot observe a null state or the next combat's players.
	/// </summary>
	private static async Task AfterCreatureAdded(Creature creature, CombatState state)
	{
		await creature.AfterAddedToRoom();
		if (creature.IsEnemy && state.CurrentSide == CombatSide.Player)
		{
			creature.Monster.RollMove(state.Players.Select((Player p) => p.Creature));
		}
	}

	/// <summary>
	/// Check for the player's hand to be empty and run the appropriate hooks if it is.
	///
	/// We can't just do this check every time the hand size changes, because sometimes we're in the middle of a
	/// sequence of effects and we want to wait to check until they're all done.
	///
	/// For example, if we have <see cref="T:MegaCrit.Sts2.Core.Models.Relics.UnceasingTop" /> and the last card in our hand is <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PommelStrike" />
	/// and we play it, we have to wait to check hand size until Pommel Strike is done being played, otherwise we'll
	/// draw two cards (one when your hand becomes "empty" immediately after Pommel Strike moves to the Play pile, and
	/// another after Pommel Strike's draw command executes).
	///
	/// So, instead of automatically doing this check every time the hand size changes, we manually check after a card
	/// is played, and after a potion is used, since these are the two ways a player can manually interact with combat
	/// state (besides ending turn, which should not trigger an empty hand check). We also check once at the start of
	/// the play-capable phase (see <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.RunAutoPrePlayPhase(MegaCrit.Sts2.Core.Combat.CombatTurnState,MegaCrit.Sts2.Core.GameActions.Multiplayer.HookPlayerChoiceContext,System.Threading.Tasks.Task,MegaCrit.Sts2.Core.Entities.Players.Player)" />), to catch a hand draw that ends empty because
	/// every card was auto-played as it was drawn. If we ever add more ways, we should add this check in those too,
	/// and update this comment.
	/// </summary>
	/// <param name="choiceContext">Object that keeps context of the action this is called from.</param>
	/// <param name="player">Player whose hand we want to check.</param>
	/// <param name="combatId">
	/// The combat the calling effect belongs to, from <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.BeginCardOrPotionEffect(MegaCrit.Sts2.Core.Entities.Players.Player)" />. The check is dropped if
	/// that combat is no longer the running one: a card played in a combat that ended must not empty-hand-hook into
	/// the next one.
	/// </param>
	public async Task CheckForEmptyHand(CombatId? combatId, PlayerChoiceContext choiceContext, Player player)
	{
		CombatTurnState combatTurnState = LiveTurnStateFor(combatId);
		if (combatTurnState != null)
		{
			await CheckForEmptyHand(combatTurnState, choiceContext, player);
		}
	}

	/// <summary>
	/// Turn-state-relative variant for the turn loop, which owns a turn state directly and needs no id round trip.
	/// </summary>
	private async Task CheckForEmptyHand(CombatTurnState turnState, PlayerChoiceContext choiceContext, Player player)
	{
		if (turnState.IsInProgress && !IsExecutingCardOrPotionEffect(player) && !PileType.Hand.GetPile(player).Cards.Any())
		{
			await Hook.AfterHandEmptied(turnState.State, choiceContext, player);
		}
	}

	/// <summary>
	/// Reset the combat manager to prepare for the next combat. All combat-scoped state lives on the turn state, which is
	/// dropped wholesale here; the only per-field cleanup left is manager-level state that outlives a single combat.
	/// </summary>
	/// <param name="graceful">Usually true. Only pass false if we're exiting the game completely.</param>
	public void Reset(bool graceful)
	{
		CombatTurnState turnState = _turnState;
		if (graceful && turnState != null)
		{
			SetPhaseForAllPlayers(turnState.State, PlayerTurnPhase.None);
			foreach (Creature item in turnState.State.Creatures.ToList())
			{
				item.Reset();
				RemoveCreature(item);
				turnState.State.RemoveCreature(item);
			}
		}
		_turnState = null;
		DebugForcedTopCardOnNextShuffle = null;
		History.Clear();
		_cardOrPotionEffectDepth.Clear();
		if (turnState != null)
		{
			Log.Debug($"Combat #{turnState.Id} dropped and cancelled by Reset");
			turnState.Cancel();
		}
		RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.NotInCombat);
	}

	/// <summary>
	/// Per-player death handling for one player dying in a combat the rest of the party is still fighting: their cards
	/// leave combat and their resources are zeroed. Does nothing once every player is dead, which is the run-loss path.
	/// </summary>
	/// <param name="combatId">
	/// The combat the kill belongs to. Death handling for a combat that has already ended must not run against the
	/// next one.
	/// </param>
	/// <param name="player">The player that died.</param>
	public async Task HandlePlayerDeath(CombatId? combatId, Player player)
	{
		CombatTurnState combatTurnState = LiveTurnStateFor(combatId);
		if (combatTurnState != null && !combatTurnState.State.Players.All((Player p) => p.Creature.IsDead))
		{
			Log.Info($"Player {player.NetId} died, doing death handling");
			if (!IsExecutingCardOrPotionEffect(player))
			{
				await RemoveDeadPlayerCardsFromCombat(combatId, player);
			}
			await PlayerCmd.SetEnergy(0m, player);
			await PlayerCmd.SetStars(0m, player);
		}
	}

	/// <summary>
	/// Removes a dead player's cards from combat. Runs from <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.HandlePlayerDeath(System.Nullable{MegaCrit.Sts2.Core.Combat.CombatId},MegaCrit.Sts2.Core.Entities.Players.Player)" /> when the player is not
	/// mid-effect, otherwise deferred to when their outermost card or potion effect ends.
	/// </summary>
	/// <param name="combatId">
	/// The combat the death belongs to. A removal deferred past the end of its combat must not strip the next
	/// combat's cards.
	/// </param>
	/// <param name="player">The dead player whose cards are being removed.</param>
	public async Task RemoveDeadPlayerCardsFromCombat(CombatId? combatId, Player player)
	{
		CombatTurnState combatTurnState = LiveTurnStateFor(combatId);
		if (combatTurnState != null && player.PlayerCombatState != null && !combatTurnState.State.Players.All((Player p) => p.Creature.IsDead))
		{
			List<CardModel> list = new List<CardModel>();
			list.AddRange(player.PlayerCombatState.Hand.Cards);
			list.AddRange(player.PlayerCombatState.DrawPile.Cards);
			list.AddRange(player.PlayerCombatState.DiscardPile.Cards);
			list.AddRange(player.PlayerCombatState.ExhaustPile.Cards);
			list.AddRange(player.PlayerCombatState.PlayPile.Cards);
			CardModel[] cards = list.ToArray();
			await CardPileCmd.RemoveFromCombat(cards);
		}
	}

	/// <summary>
	/// Marks combat as pending loss. The actual loss processing happens at the next safe point
	/// (in CheckWinCondition) to avoid race conditions where effects try to run after IsInProgress is false.
	/// </summary>
	public void LoseCombat()
	{
		CombatTurnState turnState = _turnState;
		if (turnState != null && !(turnState.PendingLoss != null))
		{
			turnState.PendingLoss = new PendingLossState((CombatRoom)turnState.State.RunState.CurrentRoom);
		}
	}

	/// <summary>
	/// Processes a pending combat loss. Called from CheckWinCondition at safe points.
	/// </summary>
	private void ProcessPendingLoss(CombatTurnState turnState)
	{
		if (!(turnState.PendingLoss == null))
		{
			PendingLossState pendingLoss = turnState.PendingLoss;
			turnState.PendingLoss = null;
			turnState.IsInProgress = false;
			this.CombatEnded?.Invoke(pendingLoss.Room);
		}
	}

	/// <summary>
	/// Prefer ModelTest.WinCombat.
	/// </summary>
	internal async Task EndCombatInternal()
	{
		CombatTurnState turnState = _turnState;
		if (turnState != null)
		{
			await EndCombatInternal(turnState);
		}
	}

	private async Task EndCombatInternal(CombatTurnState turnState)
	{
		CombatState combatState = turnState.State;
		Player localPlayer = LocalContext.GetMe(combatState);
		int turnsTaken = localPlayer.PlayerCombatState.TurnNumber;
		IRunState runState = combatState.RunState;
		CombatRoom room = (CombatRoom)runState.CurrentRoom;
		turnState.IsInProgress = false;
		using (turnState.ReadyLock.EnterScope())
		{
			turnState.PlayersTakingExtraTurn.Clear();
		}
		SetPhaseForAllPlayers(combatState, PlayerTurnPhase.None);
		PlayerActionsDisabled = false;
		foreach (Player player in combatState.Players)
		{
			await player.ReviveBeforeCombatEnd();
		}
		await Hook.AfterCombatEnd(runState, combatState, room);
		History.Clear();
		room.OnCombatEnded();
		if (RunManager.Instance.NetService.Type != NetGameType.Replay)
		{
			RunManager.Instance.WriteReplay(stopRecording: true);
		}
		foreach (Player player2 in combatState.Players)
		{
			player2.AfterCombatEnd();
		}
		await Hook.AfterCombatVictory(runState, combatState, room);
		NHoverTipSet.Clear();
		if (runState.CurrentMapPointHistoryEntry != null)
		{
			runState.CurrentMapPointHistoryEntry.Rooms.Last().TurnsTaken = turnsTaken;
		}
		bool flag = runState.Map.SecondBossMapPoint != null && runState.CurrentMapCoord == runState.Map.SecondBossMapPoint.coord;
		bool flag2 = runState.Map.SecondBossMapPoint == null && runState.CurrentMapCoord == runState.Map.BossMapPoint.coord;
		if (room.RoomType == RoomType.Boss && runState.CurrentActIndex == runState.Acts.Count - 1 && (flag || flag2))
		{
			RunManager.Instance.WinTime = RunManager.Instance.RunTime;
		}
		room.MarkPreFinished();
		await SaveManager.Instance.SaveRun(room, saveProgress: false);
		NMapScreen.Instance?.SetTravelEnabled(enabled: true);
		SaveManager.Instance.UpdateProgressAfterCombatWon(localPlayer, room);
		AchievementsHelper.CheckForDefeatedAllEnemiesAchievement(runState.Act, localPlayer);
		SaveManager.Instance.SaveProgressFile();
		if (room.RoomType == RoomType.Boss)
		{
			AchievementsHelper.AfterBossDefeated(localPlayer);
		}
		combatState.MultiplayerScalingModel?.OnCombatFinished();
		if (_turnState != null)
		{
			this.CombatWon?.Invoke(room);
		}
		RunManager.Instance.ActionExecutor.Unpause();
		RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.NotInCombat);
		NRunMusicController.Instance?.UpdateTrack();
		if (_turnState != null)
		{
			this.CombatEnded?.Invoke(room);
		}
	}

	public void RemoveCreature(Creature creature)
	{
		if (creature.IsMonster)
		{
			creature.Monster.BeforeRemovedFromRoom();
			creature.Monster.ResetStateMachine();
		}
		StateTracker.Unsubscribe(creature);
		this.CreaturesChanged?.Invoke(_turnState.State);
	}

	public async Task<bool> CheckWinCondition()
	{
		CombatTurnState turnState = _turnState;
		if (turnState == null)
		{
			return false;
		}
		return await CheckWinCondition(turnState);
	}

	/// <summary>
	/// Turn-state-relative variant for the turn loop: a stale turn state resuming here evaluates and can only end its own
	/// dead combat (a no-op), never the current one.
	/// </summary>
	private async Task<bool> CheckWinCondition(CombatTurnState turnState)
	{
		if (turnState.PendingLoss != null)
		{
			ProcessPendingLoss(turnState);
			return true;
		}
		if (IsCombatEnding(turnState))
		{
			await EndCombatInternal(turnState);
			return true;
		}
		return false;
	}

	private async Task ExecuteEnemyTurn(CombatTurnState turnState, Func<Task>? actionDuringEnemyTurn = null)
	{
		if (!turnState.IsInProgress)
		{
			return;
		}
		turnState.Ct.ThrowIfCancellationRequested();
		if (actionDuringEnemyTurn != null)
		{
			await actionDuringEnemyTurn();
			turnState.Ct.ThrowIfCancellationRequested();
		}
		foreach (Creature enemy in turnState.State.Enemies.ToList())
		{
			if (turnState.State.ContainsCreature(enemy))
			{
				NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(enemy);
				if (nCreature != null)
				{
					await nCreature.PerformIntent();
				}
				await enemy.TakeTurn();
				turnState.Ct.ThrowIfCancellationRequested();
				await WaitForUnpause(turnState);
				await CheckWinCondition(turnState);
				if (!turnState.IsInProgress)
				{
					return;
				}
			}
		}
		RunManager.Instance.ChecksumTracker.GenerateChecksum("After enemy turn end", null);
		await EndEnemyTurn(turnState);
	}

	private async Task AfterAllPlayersReadyToEndTurn(CombatTurnState turnState, EndTurnSignal signal)
	{
		CombatTurnState turnState2 = turnState;
		EndTurnSignal signal2 = signal;
		if (!turnState2.IsInProgress)
		{
			return;
		}
		if ((signal2.ScheduledPlayer.PlayerCombatState?.TurnNumber ?? (-1)) != signal2.ScheduledTurnNumber)
		{
			Log.Error($"Combat #{turnState2.Id}: stale player-turn-end transition for player {signal2.ScheduledPlayer.NetId}: the turn it was scheduled for has ended. Running it anyway; only the turn loop advances turns, so something has resumed a turn the turn loop did not.");
			if (!_staleTurnEndReported)
			{
				_staleTurnEndReported = true;
				SentryService.CaptureMessage("Ran a stale player-turn-end transition; the turn it was scheduled for had ended", SentryLevel.Error, delegate(Scope scope)
				{
					scope.SetExtra("combatId", turnState2.Id.Value);
					scope.SetExtra("scheduledTurnNumber", signal2.ScheduledTurnNumber);
					scope.SetExtra("currentTurnNumber", signal2.ScheduledPlayer.PlayerCombatState?.TurnNumber ?? (-1));
				});
			}
		}
		turnState2.Ct.ThrowIfCancellationRequested();
		turnState2.EndingPlayerTurnPhaseOne = true;
		try
		{
			RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.EndTurnPhaseOne);
			await WaitUntilQueueIsEmptyOrWaitingOnNonPlayerDrivenAction(turnState2);
			await EndPlayerTurnPhaseOneInternal(turnState2);
			if (turnState2.IsInProgress && RunManager.Instance.NetService.Type != NetGameType.Replay)
			{
				RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new ReadyToBeginEnemyTurnAction(LocalContext.GetMe(turnState2.State), signal2.ActionDuringEnemyTurn));
			}
		}
		finally
		{
			turnState2.EndingPlayerTurnPhaseOne = false;
		}
	}

	private async Task WaitUntilQueueIsEmptyOrWaitingOnNonPlayerDrivenAction(CombatTurnState turnState)
	{
		GameAction currentlyRunningAction = RunManager.Instance.ActionExecutor.CurrentlyRunningAction;
		if (currentlyRunningAction == null || !ActionQueueSet.IsGameActionPlayerDriven(currentlyRunningAction))
		{
			return;
		}
		TaskCompletionSource completionSource = new TaskCompletionSource();
		RunManager.Instance.ActionExecutor.AfterActionExecuted += AfterActionExecuted;
		try
		{
			await completionSource.Task.WaitAsync(turnState.Ct);
		}
		finally
		{
			RunManager.Instance.ActionExecutor.AfterActionExecuted -= AfterActionExecuted;
		}
		void AfterActionExecuted(GameAction action)
		{
			GameAction readyAction = RunManager.Instance.ActionQueueSet.GetReadyAction();
			if (readyAction == null || !ActionQueueSet.IsGameActionPlayerDriven(readyAction))
			{
				completionSource.TrySetResult();
			}
		}
	}

	/// <summary>
	/// Calls all end-of-turn hooks that could require player choices to be made.
	/// Prefer ModelTest.PassToEnemyTurn, whose doc comment covers when the bypass is safe.
	/// </summary>
	internal async Task EndPlayerTurnPhaseOneInternal()
	{
		CombatTurnState turnState = _turnState;
		if (turnState != null)
		{
			await EndPlayerTurnPhaseOneInternal(turnState);
		}
	}

	private async Task EndPlayerTurnPhaseOneInternal(CombatTurnState turnState)
	{
		if (turnState.Ct.IsCancellationRequested)
		{
			return;
		}
		if (turnState.State.CurrentSide != CombatSide.Player)
		{
			throw new InvalidOperationException($"EndPlayerTurn called while the current side is {turnState.State.CurrentSide}!");
		}
		await WaitForUnpause(turnState);
		List<Player> playersEndingTurn;
		using (turnState.ReadyLock.EnterScope())
		{
			playersEndingTurn = ((turnState.PlayersTakingExtraTurn.Count > 0) ? turnState.PlayersTakingExtraTurn.ToList() : turnState.State.Players.ToList());
		}
		List<(Player, HookPlayerChoiceContext)> autoPostPlayContexts = new List<(Player, HookPlayerChoiceContext)>();
		foreach (Player player in playersEndingTurn)
		{
			if (turnState.Ct.IsCancellationRequested)
			{
				return;
			}
			if (LocalContext.NetId.HasValue)
			{
				player.PlayerCombatState.Phase = PlayerTurnPhase.AutoPostPlay;
				HookPlayerChoiceContext playerChoiceContext2 = new HookPlayerChoiceContext(player, LocalContext.NetId.Value, GameActionType.CombatPlayPhaseOnly);
				Task task = Hook.AfterAutoPostPlayPhaseEntered(playerChoiceContext2, turnState.State, player);
				await playerChoiceContext2.AssignTaskAndWaitForPauseOrCompletion(task);
				autoPostPlayContexts.Add((player, playerChoiceContext2));
			}
		}
		foreach (var (player, hookPlayerChoiceContext) in autoPostPlayContexts)
		{
			await hookPlayerChoiceContext.WaitForCompletion();
			player.PlayerCombatState.Phase = PlayerTurnPhase.End;
		}
		if (turnState.Ct.IsCancellationRequested)
		{
			return;
		}
		await Hook.BeforeSideTurnEnd(turnState.State, turnState.State.CurrentSide, playersEndingTurn.Select((Player p) => p.Creature));
		if (await CheckWinCondition(turnState))
		{
			return;
		}
		List<HookPlayerChoiceContext> playerEndContexts = new List<HookPlayerChoiceContext>();
		foreach (Player item in playersEndingTurn)
		{
			if (LocalContext.NetId.HasValue)
			{
				HookPlayerChoiceContext playerChoiceContext2 = new HookPlayerChoiceContext(item, LocalContext.NetId.Value, GameActionType.Combat);
				Task task2 = DoTurnEnd(turnState, item, playerChoiceContext2);
				await playerChoiceContext2.AssignTaskAndWaitForPauseOrCompletion(task2);
				playerEndContexts.Add(playerChoiceContext2);
			}
		}
		foreach (HookPlayerChoiceContext item2 in playerEndContexts)
		{
			await item2.WaitForCompletion();
		}
		if (await CheckWinCondition(turnState))
		{
			return;
		}
		if (!turnState.Ct.IsCancellationRequested)
		{
			foreach (Player item3 in playersEndingTurn)
			{
				await Hook.BeforeFlush(turnState.State, item3);
			}
		}
		RunManager.Instance.ChecksumTracker.GenerateChecksum("After player turn phase one end", null);
		await CheckWinCondition(turnState);
	}

	/// <summary>
	/// Executes turn end hooks for a player.
	/// If player choice occurs during this method, it uses the passed choice context. This way, each player's turn end
	/// runs independently of all others.
	/// </summary>
	/// <param name="turnState">The turn state for the current combat.</param>
	/// <param name="player">The player whose turn is ending.</param>
	/// <param name="choiceContext">The context to use for any player choices.</param>
	private async Task DoTurnEnd(CombatTurnState turnState, Player player, PlayerChoiceContext choiceContext)
	{
		await player.PlayerCombatState.OrbQueue.BeforeTurnEnd(choiceContext);
		if (!turnState.IsInProgress || IsCombatEnding(turnState))
		{
			return;
		}
		CardPile pile = PileType.Hand.GetPile(player);
		List<CardModel> turnEndCards = new List<CardModel>();
		List<CardModel> list = new List<CardModel>();
		foreach (CardModel card in pile.Cards)
		{
			if (card.HasTurnEndInHandEffect)
			{
				turnEndCards.Add(card);
			}
			else if (card.Keywords.Contains(CardKeyword.Ethereal) && Hook.ShouldEtherealTrigger(player.Creature.CombatState, card))
			{
				list.Add(card);
			}
		}
		foreach (CardModel item in list)
		{
			await CardCmd.Exhaust(choiceContext, item, causedByEthereal: true);
		}
		await DoTurnEndCards(turnState, turnEndCards, choiceContext);
	}

	/// <summary>
	/// Invokes turn end effects on all cards in hand with such an effect.
	/// Does some timing trickery so that the cards don't fly in one-by-one with linear timing.
	/// Player choices can happen in between turn end resolutions. It is very rare, so I've ignored it for now, but the
	/// UX for it is a little weird. Ideally it should pause the entire sequence.
	/// </summary>
	private async Task DoTurnEndCards(CombatTurnState turnState, List<CardModel> turnEndCards, PlayerChoiceContext choiceContext)
	{
		Task task = null;
		List<Task> list = new List<Task>();
		float num = 0f;
		int num2 = 0;
		foreach (CardModel turnEndCard in turnEndCards)
		{
			Task task2 = AddTurnEndCardToPlayPileWithDelay(turnEndCard, num);
			Task waitTask = task2;
			if (task != null)
			{
				global::_003C_003Ey__InlineArray2<Task> buffer = default(global::_003C_003Ey__InlineArray2<Task>);
				global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 0) = task2;
				global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 1) = task;
				waitTask = Task.WhenAll(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray2<Task>, Task>(in buffer, 2));
			}
			Task<CardPileAddResult?> task3 = ResolveTurnEndCardEffects(turnEndCard, choiceContext, waitTask);
			Task item = TweenTurnEndCardToResultPile(turnState, task3);
			list.Add(item);
			task = task3;
			float num3 = 1f - (float)num2 / ((float)num2 + 3f);
			float num4 = (LocalContext.IsMine(turnEndCard) ? ((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.4f : 0.8f) : 0.3f);
			num += num4 * num3;
			num2++;
		}
		await Task.WhenAll(list);
	}

	private async Task AddTurnEndCardToPlayPileWithDelay(CardModel card, float delay)
	{
		await Cmd.Wait(delay);
		await CardPileCmd.Add(card, PileType.Play);
	}

	private async Task<CardPileAddResult?> ResolveTurnEndCardEffects(CardModel card, PlayerChoiceContext choiceContext, Task waitTask)
	{
		await waitTask;
		await card.OnTurnEndInHandWrapper(choiceContext);
		return (!card.Keywords.Contains(CardKeyword.Ethereal)) ? new CardPileAddResult?(await CardPileCmd.Add(card, PileType.Discard.GetPile(card.Owner), CardPilePosition.Bottom, null, skipVisuals: true)) : (await CardCmd.Exhaust(choiceContext, card, causedByEthereal: true, skipVisuals: true));
	}

	private async Task TweenTurnEndCardToResultPile(CombatTurnState turnState, Task<CardPileAddResult?> resultTask)
	{
		CardPileAddResult? cardPileAddResult = await resultTask;
		if (cardPileAddResult.HasValue && cardPileAddResult.GetValueOrDefault().success)
		{
			Tween item = CardPileCmd.GetTweenForCardsChangingPiles(new global::_003C_003Ez__ReadOnlySingleElementList<CardPileAddResult>(cardPileAddResult.Value), fromSilentAdd: true).Item1;
			if (item != null)
			{
				await item.AwaitFinished(turnState.Ct);
			}
		}
	}

	private async Task EndEnemyTurnInternal(CombatTurnState turnState)
	{
		List<Creature> enemies = turnState.State.CreaturesOnCurrentSide.ToList();
		await Hook.BeforeSideTurnEnd(turnState.State, turnState.State.CurrentSide, enemies);
		foreach (Player player in turnState.State.Players)
		{
			player.PlayerCombatState.EndOfTurnCleanup();
		}
		await Hook.AfterSideTurnEnd(turnState.State, turnState.State.CurrentSide, enemies);
	}

	private async Task AfterAllPlayersReadyToBeginEnemyTurn(CombatTurnState turnState)
	{
		if (!turnState.IsInProgress)
		{
			return;
		}
		turnState.Ct.ThrowIfCancellationRequested();
		turnState.EndingPlayerTurnPhaseTwo = true;
		try
		{
			RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.NotPlayPhase);
			this.AboutToSwitchToEnemyTurn?.Invoke(turnState.State);
			await Task.Yield();
			if (turnState.IsInProgress && !turnState.Ct.IsCancellationRequested && turnState.State.CurrentSide == CombatSide.Player)
			{
				await EndPlayerTurnPhaseTwoInternal(turnState);
				await SwitchFromPlayerToEnemySide(turnState);
			}
		}
		finally
		{
			turnState.EndingPlayerTurnPhaseTwo = false;
		}
	}

	/// <summary>
	/// Does all the player state cleanup for the end of their turn. It must not call any hooks that might cause
	/// player choices to occur.
	/// Prefer ModelTest.PassToEnemyTurn, whose doc comment covers when the bypass is safe.
	/// </summary>
	internal async Task EndPlayerTurnPhaseTwoInternal()
	{
		CombatTurnState turnState = _turnState;
		if (turnState != null)
		{
			await EndPlayerTurnPhaseTwoInternal(turnState);
		}
	}

	private async Task EndPlayerTurnPhaseTwoInternal(CombatTurnState turnState)
	{
		turnState.Ct.ThrowIfCancellationRequested();
		if (turnState.State.CurrentSide != CombatSide.Player)
		{
			throw new InvalidOperationException($"EndPlayerTurnPhaseTwo called while the current side is {turnState.State.CurrentSide}!");
		}
		List<Player> playersEndingTurn;
		using (turnState.ReadyLock.EnterScope())
		{
			playersEndingTurn = ((turnState.PlayersTakingExtraTurn.Count > 0) ? turnState.PlayersTakingExtraTurn.ToList() : turnState.State.Players.ToList());
		}
		List<HookPlayerChoiceContext> flushPlayerHandContexts = new List<HookPlayerChoiceContext>();
		foreach (Player item in playersEndingTurn)
		{
			if (turnState.Ct.IsCancellationRequested)
			{
				return;
			}
			if (LocalContext.NetId.HasValue)
			{
				HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item, LocalContext.NetId.Value, GameActionType.CombatPlayPhaseOnly);
				Task task = FlushPlayerHand(turnState, item, playerChoiceContext);
				await playerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task);
				flushPlayerHandContexts.Add(playerChoiceContext);
			}
		}
		foreach (HookPlayerChoiceContext item2 in flushPlayerHandContexts)
		{
			await item2.WaitForCompletion();
		}
		if (!turnState.Ct.IsCancellationRequested)
		{
			await Hook.AfterSideTurnEnd(turnState.State, turnState.State.CurrentSide, playersEndingTurn.Select((Player p) => p.Creature));
			turnState.Ct.ThrowIfCancellationRequested();
		}
		RunManager.Instance.ChecksumTracker.GenerateChecksum("after player turn phase two end", null);
	}

	private async Task FlushPlayerHand(CombatTurnState turnState, Player player, HookPlayerChoiceContext playerChoiceContext)
	{
		if (player.Creature.IsDead)
		{
			return;
		}
		if (player.PlayerCombatState == null)
		{
			Log.Warn($"Player combat state is null. Assuming that the run has been cleaned up. (Player: {player.NetId})");
			return;
		}
		CombatState state = turnState.State;
		List<CardModel> cardsToFlush = new List<CardModel>();
		List<CardModel> cardsToRetain = new List<CardModel>();
		bool flag = Hook.ShouldFlush(state, player);
		foreach (CardModel card in PileType.Hand.GetPile(player).Cards)
		{
			if (!flag || card.ShouldRetainThisTurn)
			{
				cardsToRetain.Add(card);
			}
			else
			{
				cardsToFlush.Add(card);
			}
		}
		if (cardsToFlush.Count > 0)
		{
			await CardPileCmd.Add(cardsToFlush, PileType.Discard);
			turnState.Ct.ThrowIfCancellationRequested();
		}
		await Hook.AfterFlush(state, player, playerChoiceContext, cardsToFlush, cardsToRetain);
		turnState.Ct.ThrowIfCancellationRequested();
		player.PlayerCombatState.EndOfTurnCleanup();
	}

	/// <summary>
	/// Switches from the player side to the enemy side, handling extra player turns if necessary. It only performs the
	/// switch; the turn loop runs the turn on the new side.
	/// </summary>
	private async Task SwitchFromPlayerToEnemySide(CombatTurnState turnState)
	{
		if (turnState.Ct.IsCancellationRequested)
		{
			return;
		}
		List<Player> list;
		using (turnState.ReadyLock.EnterScope())
		{
			turnState.PlayersTakingExtraTurn.Clear();
			foreach (Player player in turnState.State.Players)
			{
				if (Hook.ShouldTakeExtraTurn(turnState.State, player))
				{
					Log.Info($"Player {player.NetId} ({player.Character.Id.Entry}) is taking an extra turn");
					turnState.PlayersTakingExtraTurn.Add(player);
				}
			}
			list = turnState.PlayersTakingExtraTurn.ToList();
		}
		SwitchSides(turnState);
		foreach (Player item in list)
		{
			if (turnState.Ct.IsCancellationRequested)
			{
				return;
			}
			await Hook.AfterTakingExtraTurn(turnState.State, item);
		}
		await WaitForUnpause(turnState);
	}

	private void SwitchSides(CombatTurnState turnState)
	{
		if (turnState.Ct.IsCancellationRequested)
		{
			return;
		}
		bool flag;
		using (turnState.ReadyLock.EnterScope())
		{
			flag = turnState.PlayersTakingExtraTurn.Count > 0;
		}
		if (turnState.State.CurrentSide == CombatSide.Player && !flag)
		{
			turnState.State.CurrentSide = CombatSide.Enemy;
		}
		else
		{
			turnState.State.CurrentSide = CombatSide.Player;
			IReadOnlyList<Player> readOnlyList;
			if (flag)
			{
				readOnlyList = turnState.PlayersTakingExtraTurn;
			}
			else
			{
				readOnlyList = turnState.State.Players;
				turnState.State.RoundNumber++;
			}
			foreach (Player item in readOnlyList)
			{
				item.PlayerCombatState.IncrementTurnNumber();
			}
		}
		foreach (Creature creature in turnState.State.Creatures)
		{
			creature.OnSideSwitch();
		}
		this.TurnEnded?.Invoke(turnState.State);
	}

	/// <summary>
	/// Pause combat.
	/// </summary>
	public void Pause()
	{
		if (!NonInteractiveMode.IsActive && IsInProgress)
		{
			IsPaused = true;
		}
	}

	/// <summary>
	/// Un-pause combat.
	/// </summary>
	public void Unpause()
	{
		if (!NonInteractiveMode.IsActive)
		{
			IsPaused = false;
		}
	}

	/// <summary>
	/// Returns true if the passed player is taking part in the current player turn.
	/// Returns false if some player is taking an extra turn, and it's not us.
	/// If it is not the player turn, then this returns false.
	/// </summary>
	public bool IsPartOfPlayerTurn(Player player)
	{
		CombatTurnState turnState = _turnState;
		if (turnState == null || turnState.State.CurrentSide != CombatSide.Player)
		{
			return false;
		}
		using (turnState.ReadyLock.EnterScope())
		{
			if (turnState.PlayersTakingExtraTurn.Count == 0)
			{
				return true;
			}
			return turnState.PlayersTakingExtraTurn.Contains(player);
		}
	}

	public Task WaitForUnpause()
	{
		CombatTurnState turnState = _turnState;
		if (turnState != null)
		{
			return WaitForUnpause(turnState);
		}
		return Task.CompletedTask;
	}

	/// <summary>
	/// Turn-state-relative variant for the turn loop: waits against this turn state's combat, so a stale turn state
	/// resumes (and then dies at its next token check) instead of waiting on the current combat's pause state.
	/// </summary>
	private async Task WaitForUnpause(CombatTurnState turnState)
	{
		if (!NonInteractiveMode.IsActive)
		{
			while (IsPaused && turnState.IsLive)
			{
				await NGame.Instance.AwaitProcessFrame();
			}
		}
	}

	/// <summary>
	/// WARNING: ONLY CALL THIS IN TESTS!
	/// Force the specified card to be moved to the top of the next shuffle.
	/// Useful for tests for shuffle tests where the first card drawn afterwards matters.
	/// </summary>
	/// <param name="card">Card to force to the top.</param>
	public void DebugForceTopCardOnNextShuffle(CardModel card)
	{
		card.AssertMutable();
		DebugForcedTopCardOnNextShuffle = card;
	}

	/// <summary>
	/// WARNING: ONLY CALL THIS IN TESTS!
	/// Clear the forced specified card to be moved to the top of the next shuffle.
	/// Useful for tests for shuffle tests where the first card drawn afterwards matters.
	/// </summary>
	public void DebugClearForcedTopCardOnNextShuffle()
	{
		DebugForcedTopCardOnNextShuffle = null;
	}
}
