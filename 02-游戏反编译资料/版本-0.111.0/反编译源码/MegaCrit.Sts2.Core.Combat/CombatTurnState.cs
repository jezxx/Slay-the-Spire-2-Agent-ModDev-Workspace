using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Combat;

/// <summary>
/// The turn-scoped state owned by one combat. Every combat gets a fresh instance in
/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.SetUpCombat(MegaCrit.Sts2.Core.Combat.CombatState)" />, and <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.Reset(System.Boolean)" /> drops it wholesale, so combat-scoped
/// state cannot leak into the next combat through a forgotten per-field reset.
/// The turn loop threads its own turn state through the entire turn flow and checks liveness against it, never
/// against the manager's current fields: a turn state that resumes after its combat was torn down observes its own
/// cancelled token instead of adopting the next combat's fresh one.
///
/// Internal, along with the signal records above: the single-owner model depends on exactly one turn state existing
/// per combat, so nothing outside this assembly should be able to build one and hand it to the manager. Tests get in
/// through InternalsVisibleTo (sts2.csproj), not through public API.
/// </summary>
internal sealed class CombatTurnState
{
	private static int _sequenceCounter;

	private readonly CancellationTokenSource _cts = new CancellationTokenSource();

	/// <summary>
	/// Monotonically increasing id, used to correlate turn loop log lines with the combat they belong to, and handed
	/// to callers outside the assembly (as <see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.CurrentCombatId" />) so they can capture which
	/// combat their work belongs to.
	/// </summary>
	public CombatId Id { get; }

	/// <summary>
	/// The combat this turn state owns. Code running on a turn state reads this, never the manager's current state,
	/// so a stale turn state mutates only its own dead combat.
	/// </summary>
	public CombatState State { get; }

	/// <summary>
	/// Cancelled exactly once, in <see cref="M:MegaCrit.Sts2.Core.Combat.CombatTurnState.Cancel" />, when this turn state's combat is torn down.
	/// </summary>
	public CancellationToken Ct => _cts.Token;

	/// <summary>
	/// True while this turn state's combat is running: in progress and not torn down. At the end of a won combat
	/// <see cref="P:MegaCrit.Sts2.Core.Combat.CombatTurnState.IsInProgress" /> is already false while the turn state is only cancelled when the room exits;
	/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatTurnState.Cancel" /> also clears <see cref="P:MegaCrit.Sts2.Core.Combat.CombatTurnState.IsInProgress" />, so a torn-down turn state fails both halves.
	/// </summary>
	public bool IsLive
	{
		get
		{
			if (IsInProgress)
			{
				return !_cts.IsCancellationRequested;
			}
			return false;
		}
	}

	public Lock ReadyLock { get; } = new Lock();


	public HashSet<Player> PlayersReadyToEndTurn { get; } = new HashSet<Player>();


	public HashSet<Player> PlayersReadyToBeginEnemyTurn { get; } = new HashSet<Player>();


	public List<Player> PlayersTakingExtraTurn { get; } = new List<Player>();


	/// <summary>
	/// Completed by <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.SetReadyToEndTurn(MegaCrit.Sts2.Core.Entities.Players.Player,System.Boolean,System.Func{System.Threading.Tasks.Task})" /> once all players are ready to end the current player
	/// turn. The turn loop awaits this: ready deliveries are data, and the turn loop is the single owner of turn control
	/// flow. Because the turn loop does not consume the signal until player-turn setup has finished, an end-turn that
	/// fires during setup (e.g. Void Form auto-played by Whispering Earring during the AutoPrePlay phase) is held until
	/// setup completes. Recreated at each player turn start; cancelled in <see cref="M:MegaCrit.Sts2.Core.Combat.CombatTurnState.Cancel" /> so a turn loop suspended
	/// awaiting it dies with its combat. Assignment guarded by <see cref="P:MegaCrit.Sts2.Core.Combat.CombatTurnState.ReadyLock" />; completed outside the lock.
	/// </summary>
	public TaskCompletionSource<EndTurnSignal>? EndTurnSignalSource { get; set; }

	/// <summary>
	/// Completed by <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.SetReadyToBeginEnemyTurn(MegaCrit.Sts2.Core.Entities.Players.Player,System.Func{System.Threading.Tasks.Task})" /> once all players' ready actions have been
	/// delivered for the current turn. Same ownership model as <see cref="P:MegaCrit.Sts2.Core.Combat.CombatTurnState.EndTurnSignalSource" />. The payload is the
	/// optional test hook to run during the enemy turn, carried by the local player's ready action.
	/// </summary>
	public TaskCompletionSource<Func<Task>?>? BeginEnemyTurnSignalSource { get; set; }

	/// <summary>
	/// Set when this combat is lost mid-action; processed (combat ended) at the next safe point in
	/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.CheckWinCondition" />. Turn-state-scoped so a loss pending at teardown dies with its
	/// combat instead of being processed against the next one.
	/// </summary>
	public PendingLossState? PendingLoss { get; set; }

	public bool IsInProgress { get; set; }

	public bool IsStarting { get; set; } = true;


	public bool IsEnemyTurnStarted { get; set; }

	public bool EndingPlayerTurnPhaseOne { get; set; }

	public bool EndingPlayerTurnPhaseTwo { get; set; }

	public CombatTurnState(CombatState state)
	{
		State = state;
		Id = new CombatId(Interlocked.Increment(ref _sequenceCounter));
	}

	/// <summary>
	/// Tears this turn state down: cancels the token, then the per-turn signals, so a turn loop suspended anywhere in
	/// the turn flow dies with its combat. Callers must not hold <see cref="P:MegaCrit.Sts2.Core.Combat.CombatTurnState.ReadyLock" />: a turn loop suspended
	/// awaiting a signal, or inside a WaitAsync, unwinds synchronously inside the cancel calls, and that unwind must
	/// not run under the lock.
	/// </summary>
	public void Cancel()
	{
		TaskCompletionSource<EndTurnSignal> endTurnSignalSource;
		TaskCompletionSource<Func<Task>> beginEnemyTurnSignalSource;
		using (ReadyLock.EnterScope())
		{
			endTurnSignalSource = EndTurnSignalSource;
			beginEnemyTurnSignalSource = BeginEnemyTurnSignalSource;
			EndTurnSignalSource = null;
			BeginEnemyTurnSignalSource = null;
		}
		IsInProgress = false;
		_cts.Cancel();
		endTurnSignalSource?.TrySetCanceled();
		beginEnemyTurnSignalSource?.TrySetCanceled();
	}
}
