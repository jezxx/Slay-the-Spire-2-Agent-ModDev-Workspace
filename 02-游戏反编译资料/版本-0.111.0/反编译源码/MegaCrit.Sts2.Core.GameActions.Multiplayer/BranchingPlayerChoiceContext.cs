using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

/// <summary>
/// A player choice context used when there is an existing player choice context, but we might begin a player choice for
/// a different player.
///
/// If there is a GameAction in progress, and we're executing within it, then a choice for the non-owning player will
/// block the OWNING player's queue by default. For example, if I throw a Skill Potion at another player, then it will
/// block MY actions, and not the other player's. This is undesirable for several reasons:
///  - I can't play cards while you're making a choice.
///  - I might open a new choice screen for you while you're already looking at one.
///
/// The BranchingPlayerChoiceContext exists to solve these problems. If a new choice begins, then:
///  - If the choice is for the owning player, we continue with the existing choice context.
///  - If the choice is for someone else, we create a new GameAction (via the HookPlayerChoiceContext) and use that new
///    GameAction for execution of the player choice.
///
/// It is up to the caller to specify what the boundaries of execution for the new GameAction are, using
/// AssignTaskAndWaitForPauseOrCompletion. For example, take Tutor:
///  - We want everything after the player choice within OnPlay to execute on the CHOOSING player's queue...
///  - But we want Hook.AfterCardPlayed to execute without waiting for that choice on the OWNING player's queue.
/// Imagine if the owning player has Unceasing Top; we want that draw to take place before the choosing player makes
/// their choice, so that the owning player is not awkwardly waiting for the choosing player. This is why the
/// BranchingPlayerChoiceContext is passed the Task for CardModel.OnPlay, and not CardModel.OnPlayWrapper.
///
/// For a contrasting example, see Huddle Up. If its draws trigger an autoplay:
///  - We want that specific set of draws to delay on the autoplay on the CHOOSING player's queue...
///  - But we do not want that set of draws to delay other player's draws.
///  - And if no draw triggers an autoplay, we want the entire group of draws to execute in one GameAction.
/// It uses BranchingPlayerChoiceContext directly within its OnPlay to achieve this effect. Each CardPileCmd.Draw call
/// creates its own BranchingPlayerChoiceContext, which surrounds only the Draw Task and not the entire CardModel.OnPlay.
///
/// With all that said, this API is very experimental and needs proving out in real testing. I think it's likely I have
/// not thought through every single scenario.
/// </summary>
public class BranchingPlayerChoiceContext : PlayerChoiceContext
{
	private PlayerChoiceContext _originalContext;

	private readonly GameActionType _gameActionType;

	private readonly ulong _localPlayerId;

	private HookPlayerChoiceContext? _createdContext;

	private TaskCompletionSource _pausedCompletionSource = new TaskCompletionSource();

	public AbstractModel? Source { get; }

	public override ulong? OwnerId => _originalContext.OwnerId;

	public event Action<HookPlayerChoiceContext>? AfterBranched;

	public BranchingPlayerChoiceContext(ulong localPlayerId, GameActionType gameActionType, PlayerChoiceContext existing)
	{
		_originalContext = existing;
		_gameActionType = gameActionType;
		_localPlayerId = localPlayerId;
	}

	public BranchingPlayerChoiceContext(AbstractModel source, ulong localPlayerId, GameActionType gameActionType, PlayerChoiceContext existing)
	{
		_originalContext = existing;
		_gameActionType = gameActionType;
		_localPlayerId = localPlayerId;
		Source = source;
	}

	public override async Task SignalPlayerChoiceBegun(Player chooser, PlayerChoiceOptions options)
	{
		PlayerChoiceContext playerChoiceContext;
		if (!_originalContext.OwnerId.HasValue || chooser.NetId == _originalContext.OwnerId)
		{
			if (!_originalContext.OwnerId.HasValue)
			{
				Log.LogMessage(LogLevel.Debug, LogType.GameSync, $"Branching context began choice for {chooser.NetId} and there is no owner. Using existing {_originalContext}");
			}
			playerChoiceContext = _originalContext;
		}
		else if (_createdContext != null)
		{
			Log.Warn($"{"BranchingPlayerChoiceContext"} has been used twice! We switched owners from {_originalContext.OwnerId} to {_createdContext.OwnerId}, and now we are trying to switch to {chooser.NetId}. Re-using the existing created context.");
			playerChoiceContext = _createdContext;
		}
		else
		{
			Log.LogMessage(LogLevel.Debug, LogType.GameSync, $"Branching context began choice for {chooser.NetId} who is not the owner ({_originalContext.OwnerId}). Using new {"HookPlayerChoiceContext"}");
			if (Source != null)
			{
				_createdContext = new HookPlayerChoiceContext(Source, chooser, _localPlayerId, _gameActionType);
			}
			else
			{
				_createdContext = new HookPlayerChoiceContext(chooser, _localPlayerId, _gameActionType);
			}
			this.AfterBranched?.Invoke(_createdContext);
			_pausedCompletionSource.SetResult();
			playerChoiceContext = _createdContext;
		}
		await playerChoiceContext.SignalPlayerChoiceBegun(chooser, options);
	}

	public async Task AssignTaskAndWaitForPauseOrCompletion(Task task)
	{
		await TaskHelper.WhenAny(task, _pausedCompletionSource.Task);
		if (_pausedCompletionSource.Task.IsCompleted)
		{
			await _createdContext.AssignTaskAndWaitForPauseOrCompletion(task);
		}
	}

	public override Task SignalPlayerChoiceEnded()
	{
		return (_createdContext ?? _originalContext).SignalPlayerChoiceEnded();
	}
}
