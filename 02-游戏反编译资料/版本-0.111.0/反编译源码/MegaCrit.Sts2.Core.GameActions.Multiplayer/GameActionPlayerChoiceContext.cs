using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

public class GameActionPlayerChoiceContext : PlayerChoiceContext
{
	private ActionQueueSynchronizer? _actionQueueSynchronizer;

	private ActionQueueSet? _actionQueueSet;

	private ActionExecutor? _actionExecutor;

	public GameAction Action { get; }

	public override ulong? OwnerId => Action.OwnerId;

	private ActionQueueSynchronizer ActionQueueSynchronizer => _actionQueueSynchronizer ?? RunManager.Instance.ActionQueueSynchronizer;

	private ActionQueueSet ActionQueueSet => _actionQueueSet ?? RunManager.Instance.ActionQueueSet;

	private ActionExecutor ActionExecutor => _actionExecutor ?? RunManager.Instance.ActionExecutor;

	public GameActionPlayerChoiceContext(GameAction action)
	{
		Action = action;
	}

	public override Task SignalPlayerChoiceBegun(Player chooser, PlayerChoiceOptions options)
	{
		if (chooser.NetId != OwnerId)
		{
			Log.Warn($"{"GameActionPlayerChoiceContext"} is executing player choice owned by {chooser.NetId}, but the player choice began with owner {OwnerId}! This will work, but will likely result in some weird-looking user experience. See {"BlockingPlayerChoiceContext"} for a resolution.");
		}
		if (ActionExecutor != null && ActionExecutor.CurrentlyRunningAction != Action)
		{
			Log.Error($"Tried to interrupt shared queue action {ActionExecutor.CurrentlyRunningAction} with a player choice context with action {Action}!");
			return Task.CompletedTask;
		}
		ActionQueueSet.PauseActionForPlayerChoice(Action, options);
		return Task.CompletedTask;
	}

	public override async Task SignalPlayerChoiceEnded()
	{
		if (Action.OwnerId == LocalContext.NetId || TestFlags.ShouldSendResumeForRemotePlayers)
		{
			ActionQueueSynchronizer.RequestResumeActionAfterPlayerChoice(Action);
		}
		await Action.WaitForActionToResumeExecutingAfterPlayerChoice();
	}

	public void MockDependenciesForTest(ActionQueueSynchronizer? actionQueueSynchronizer, ActionQueueSet? actionQueueSet, ActionExecutor? actionExecutor)
	{
		_actionQueueSet = actionQueueSet;
		_actionQueueSynchronizer = actionQueueSynchronizer;
		_actionExecutor = actionExecutor;
	}
}
