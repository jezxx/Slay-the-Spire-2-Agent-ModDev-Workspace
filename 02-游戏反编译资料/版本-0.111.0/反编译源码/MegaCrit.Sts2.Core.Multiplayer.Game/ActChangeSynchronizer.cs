using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class ActChangeSynchronizer
{
	private readonly RunState _runState;

	private readonly List<bool> _readyPlayers = new List<bool>();

	private readonly Logger _logger = new Logger("ActChangeSynchronizer", LogType.GameSync);

	/// <summary>
	/// The last act index we transitioned away from.
	/// All attempts to vote to transition away from this act again will be ignored.
	/// </summary>
	private int _lastTransitioningActIndex = -1;

	public ActChangeSynchronizer(RunState runState)
	{
		_runState = runState;
		for (int i = 0; i < runState.Players.Count; i++)
		{
			_readyPlayers.Add(item: false);
		}
	}

	public void SetLocalPlayerReady()
	{
		_logger.Info("Local player ready to move to next act");
		Player me = LocalContext.GetMe(_runState);
		if (me != null)
		{
			RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new VoteToMoveToNextActAction(me, _runState.CurrentActIndex));
		}
	}

	public bool IsWaitingForOtherPlayers()
	{
		int playerSlotIndex = _runState.GetPlayerSlotIndex(LocalContext.NetId.Value);
		for (int i = 0; i < _readyPlayers.Count; i++)
		{
			if (!_readyPlayers[i] && i != playerSlotIndex)
			{
				return true;
			}
		}
		return false;
	}

	public void OnPlayerReady(Player player, int actIndex)
	{
		_logger.Debug($"Player {player.NetId} ready to move to next act from {actIndex}");
		AbstractRoom currentRoom = _runState.CurrentRoom;
		if ((currentRoom == null || !currentRoom.IsVictoryRoom) && (actIndex < _runState.CurrentActIndex || actIndex <= _lastTransitioningActIndex))
		{
			_logger.Warn($"Player {player.NetId} tried to transition to next act from index {actIndex}, but the current index is {_runState.CurrentActIndex} and we last transitioned from {_lastTransitioningActIndex}. Ignoring.");
		}
		else
		{
			int playerSlotIndex = _runState.GetPlayerSlotIndex(player);
			_readyPlayers[playerSlotIndex] = true;
			if (_readyPlayers.All((bool x) => x))
			{
				MoveToNextAct();
			}
		}
	}

	private void MoveToNextAct()
	{
		for (int i = 0; i < _readyPlayers.Count; i++)
		{
			_readyPlayers[i] = false;
		}
		_logger.Info("All players ready to move to next act, beginning transition");
		_lastTransitioningActIndex = _runState.CurrentActIndex;
		_runState.ActFloor++;
		TaskHelper.RunSafely(RunManager.Instance.EnterNextAct());
		if (NOverlayStack.Instance?.Peek() is NRewardsScreen nRewardsScreen)
		{
			nRewardsScreen.HideWaitingForPlayersScreen();
		}
	}
}
