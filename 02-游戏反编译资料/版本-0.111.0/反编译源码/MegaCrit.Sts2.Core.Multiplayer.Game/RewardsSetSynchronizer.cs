using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

/// <summary>
/// Synchronizer responsible for executing reward set behavior for each player.
/// When any player generates a reward from any source (end of combat, relics, events), the RewardsSet calls into this
/// synchronizer. Then, as the owning player selects or skips rewards, they send messages to all other players
/// indicating which rewards they took. This synchronizer handles those messages.
/// </summary>
public class RewardsSetSynchronizer : IDisposable
{
	/// <summary>
	/// Denotes how a reward set was completed.
	/// </summary>
	private enum RewardSetCompleteState
	{
		None,
		/// <summary>
		/// All the rewards in the set were taken.
		/// </summary>
		Completed,
		/// <summary>
		/// Some (or all) of the rewards in the set were skipped.
		/// </summary>
		Skipped
	}

	/// <summary>
	/// Represents a message that was sent for a remote player about a reward set that has not yet spawned for the local
	/// player.
	/// For example, if players have differing combat speeds, one player may reach the end of combat before us, and
	/// begin taking their rewards. This will start sending messages, but our local NCombatUi will not have begun the
	/// rewards process yet, so we need to buffer the remote messages for when it does.
	/// </summary>
	private record struct BufferedMessage
	{
		public int SetId => selectedMessage?.setId ?? (skippedMessage ?? throw new InvalidOperationException()).setId;

		public RewardSelectedMessage? selectedMessage;

		public RewardSetSkippedMessage? skippedMessage;

		public ulong senderId;
	}

	/// <summary>
	/// Reward state for a given player. Contains a stack of reward sets.
	/// </summary>
	private class PlayerRewardState
	{
		/// <summary>
		/// The reward sets the player is currently choosing from.
		/// Players can have multiple rewards screens up because relics, when taken from a rewards screen, may themselves
		/// spawn new rewards screens. This acts like a stack.
		/// </summary>
		public required List<RewardsSetState> rewardsStack;

		/// <summary>
		/// If messages are received for a reward before it is started locally, then they will be buffered here until
		/// the reward set is spawned.
		/// </summary>
		public required List<BufferedMessage> bufferedMessages;

		/// <summary>
		/// Completed reward IDs, for bookkeeping purposes.
		/// </summary>
		public readonly Dictionary<int, RewardSetCompleteState> completedRewards = new Dictionary<int, RewardSetCompleteState>();

		/// <summary>
		/// The next ID to assign the rewards set for the given player.
		/// </summary>
		public int nextId;
	}

	/// <summary>
	/// State of a single reward set.
	/// </summary>
	private class RewardsSetState
	{
		/// <summary>
		/// The set of rewards.
		/// </summary>
		public required RewardsSet set;

		/// <summary>
		/// The completion source to complete when the rewards are all taken, or when they are skipped.
		/// </summary>
		public required TaskCompletionSource completionSource;
	}

	[CompilerGenerated]
	private sealed class _003CGetNextRewardIds_003Ed__32 : IEnumerable<int>, IEnumerable, IEnumerator<int>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private int _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public RewardsSetSynchronizer _003C_003E4__this;

		private List<PlayerRewardState>.Enumerator _003C_003E7__wrap1;

		int IEnumerator<int>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetNextRewardIds_003Ed__32(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = default(List<PlayerRewardState>.Enumerator);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				RewardsSetSynchronizer rewardsSetSynchronizer = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = rewardsSetSynchronizer._rewardStates.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					PlayerRewardState current = _003C_003E7__wrap1.Current;
					_003C_003E2__current = current.nextId;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = default(List<PlayerRewardState>.Enumerator);
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<int> IEnumerable<int>.GetEnumerator()
		{
			_003CGetNextRewardIds_003Ed__32 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CGetNextRewardIds_003Ed__32(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<int>)this).GetEnumerator();
		}
	}

	private readonly RunLocationTargetedMessageBuffer _messageBuffer;

	private readonly INetGameService _netService;

	private readonly IPlayerCollection _playerCollection;

	private readonly ulong _localPlayerId;

	private readonly Logger _logger = new Logger("RewardsSetSynchronizer", LogType.GameSync);

	/// <summary>
	/// Reward state for each player.
	/// There is one for each player in _playerCollection, in the same order.
	/// </summary>
	private readonly List<PlayerRewardState> _rewardStates = new List<PlayerRewardState>();

	private Player LocalPlayer => _playerCollection.GetPlayer(_localPlayerId);

	/// <summary>
	/// Called when we skip all rewards just before leaving the room.
	/// </summary>
	public event Action? RewardsSkippedDuringRoomExit;

	public RewardsSetSynchronizer(RunLocationTargetedMessageBuffer messageBuffer, INetGameService netService, IPlayerCollection playerCollection, ulong localPlayerId)
	{
		_netService = netService;
		_playerCollection = playerCollection;
		_localPlayerId = localPlayerId;
		for (int i = 0; i < playerCollection.Players.Count; i++)
		{
			_rewardStates.Add(new PlayerRewardState
			{
				rewardsStack = new List<RewardsSetState>(),
				bufferedMessages = new List<BufferedMessage>()
			});
		}
		_messageBuffer = messageBuffer;
		_messageBuffer.RegisterMessageHandler<RewardSelectedMessage>(HandleRewardSelectedMessage);
		_messageBuffer.RegisterMessageHandler<RewardSetSkippedMessage>(HandleRewardSetSkippedMessage);
	}

	public void Dispose()
	{
		_messageBuffer.UnregisterMessageHandler<RewardSelectedMessage>(HandleRewardSelectedMessage);
		_messageBuffer.UnregisterMessageHandler<RewardSetSkippedMessage>(HandleRewardSetSkippedMessage);
	}

	private PlayerRewardState GetRewardStateForPlayer(Player player)
	{
		for (int i = _rewardStates.Count; i <= _playerCollection.GetPlayerSlotIndex(player); i++)
		{
			_rewardStates.Add(new PlayerRewardState
			{
				rewardsStack = new List<RewardsSetState>(),
				bufferedMessages = new List<BufferedMessage>()
			});
		}
		return _rewardStates[_playerCollection.GetPlayerSlotIndex(player)];
	}

	/// <summary>
	/// Begin tracking a rewards set.
	/// This is called when a reward set is spawned and offered to the owning player.
	/// </summary>
	/// <param name="set">The rewards set to track.</param>
	/// <returns>A Task which completes when the player is done taking the rewards.</returns>
	public Task BeginRewardsSet(RewardsSet set)
	{
		PlayerRewardState rewardStateForPlayer = GetRewardStateForPlayer(set.Player);
		set.Id = rewardStateForPlayer.nextId;
		rewardStateForPlayer.nextId++;
		_logger.Debug($"Beginning rewards set {set}");
		TaskCompletionSource taskCompletionSource = new TaskCompletionSource();
		RewardsSetState rewardsSetState = new RewardsSetState
		{
			set = set,
			completionSource = taskCompletionSource
		};
		rewardStateForPlayer.rewardsStack.Add(rewardsSetState);
		foreach (BufferedMessage item in rewardStateForPlayer.bufferedMessages.ToList())
		{
			if (set.Id == item.SetId)
			{
				if (item.selectedMessage != null)
				{
					_logger.Debug("Handling buffered RewardSelectedMessage");
					HandleRewardSelectedMessage(item.selectedMessage, item.senderId);
				}
				else if (item.skippedMessage != null)
				{
					_logger.Debug("Handling buffered RewardSetSkippedMessage");
					HandleRewardSetSkippedMessage(item.skippedMessage, item.senderId);
				}
				rewardStateForPlayer.bufferedMessages.Remove(item);
			}
		}
		CompleteRewardsSetIfNecessary(rewardsSetState);
		return taskCompletionSource.Task;
	}

	/// <summary>
	/// Selects a reward locally.
	/// Throws if the reward is not owned by the local player.
	/// </summary>
	/// <param name="reward">The reward to select.</param>
	/// <returns>A Task which completes when the player finishes with the reward. See <see cref="M:MegaCrit.Sts2.Core.Rewards.Reward.SelectUnsynchronized" />.</returns>
	public async Task<bool> SelectLocalReward(Reward reward)
	{
		if (reward.Player != LocalPlayer)
		{
			throw new InvalidOperationException($"{"SelectLocalReward"} called for reward {reward} with non-local player {reward.Player.NetId}! This is not allowed");
		}
		PlayerRewardState rewardStateForPlayer = GetRewardStateForPlayer(LocalPlayer);
		if (rewardStateForPlayer.rewardsStack.Count <= 0)
		{
			throw new InvalidOperationException("Tried to sync reward for local player, but they are not currently viewing any reward set!");
		}
		RewardsSetState rewardsSetState = rewardStateForPlayer.rewardsStack.Last();
		RewardsSet set = rewardsSetState.set;
		int rewardIndex = set.Rewards.IndexOf(reward);
		RewardSelectedMessage message = new RewardSelectedMessage
		{
			location = _messageBuffer.CurrentLocation,
			setId = rewardsSetState.set.Id,
			rewardIndex = rewardIndex
		};
		_netService.SendMessage(message);
		return await SelectRewardForPlayer(rewardsSetState, reward);
	}

	/// <summary>
	/// Skips the current rewards that the local player is looking at.
	/// </summary>
	public void SkipLocalRewardsSet()
	{
		_logger.Debug("Skipping local RewardsSet");
		RewardsSetState rewardsSetState = SkipRewardsSetOnStackTopForPlayer(LocalPlayer);
		RewardSetSkippedMessage message = new RewardSetSkippedMessage
		{
			location = _messageBuffer.CurrentLocation,
			setId = rewardsSetState.set.Id
		};
		_netService.SendMessage(message);
	}

	/// <summary>
	/// Called when a remote player selects a reward.
	/// </summary>
	public void HandleRewardSelectedMessage(RewardSelectedMessage message, ulong senderId)
	{
		_logger.Debug($"Received {"RewardSelectedMessage"} from player {senderId}, set id: {message.setId} reward index: {message.rewardIndex}");
		Player player = _playerCollection.GetPlayer(senderId);
		PlayerRewardState rewardStateForPlayer = GetRewardStateForPlayer(player);
		if (rewardStateForPlayer.nextId <= message.setId)
		{
			_logger.Debug($"Buffering {"RewardSelectedMessage"} because RewardsSet id {message.setId} hasn't been created yet");
			rewardStateForPlayer.bufferedMessages.Add(new BufferedMessage
			{
				selectedMessage = message,
				senderId = senderId
			});
		}
		else
		{
			TaskHelper.RunSafely(SelectRewardForPlayer(player, message.rewardIndex));
		}
	}

	/// <summary>
	/// Called when a remote player skips the remaining the rewards in a set.
	/// Occurs both when leaving a room as a result of <see cref="M:MegaCrit.Sts2.Core.Multiplayer.Game.RewardsSetSynchronizer.BeforeLeavingRoom" /> and pressing the skip button on
	/// non-terminal rewards.
	/// </summary>
	public void HandleRewardSetSkippedMessage(RewardSetSkippedMessage message, ulong senderId)
	{
		_logger.Debug($"Received {"RewardSetSkippedMessage"} from player {senderId}, set id: {message.setId}");
		Player player = _playerCollection.GetPlayer(senderId);
		PlayerRewardState rewardStateForPlayer = GetRewardStateForPlayer(player);
		if (rewardStateForPlayer.nextId <= message.setId)
		{
			_logger.Debug($"Buffering {"RewardSetSkippedMessage"} because RewardsSet id {message.setId} hasn't been created yet");
			rewardStateForPlayer.bufferedMessages.Add(new BufferedMessage
			{
				skippedMessage = message,
				senderId = senderId
			});
		}
		else
		{
			SkipRewardsSetOnStackTopForPlayer(_playerCollection.GetPlayer(senderId));
		}
	}

	/// <summary>
	/// Selects the reward at the specified index for the given player's top-most rewards screen.
	/// </summary>
	private async Task SelectRewardForPlayer(Player player, int rewardIndex)
	{
		PlayerRewardState rewardStateForPlayer = GetRewardStateForPlayer(player);
		if (rewardStateForPlayer.rewardsStack.Count <= 0)
		{
			throw new InvalidOperationException($"Tried to select reward for player {player.NetId}, but they are not currently viewing any reward set!");
		}
		RewardsSetState rewardsSetState = rewardStateForPlayer.rewardsStack.Last();
		RewardsSet set = rewardsSetState.set;
		if (rewardIndex < 0 || rewardIndex >= set.Rewards.Count)
		{
			throw new InvalidOperationException($"Tried to select reward index {rewardIndex} for player {player.NetId}, but it is out of bounds in their current rewards set {set}!");
		}
		Reward reward = set.Rewards[rewardIndex];
		await SelectRewardForPlayer(rewardsSetState, reward);
	}

	/// <summary>
	/// Selects the passed reward for the given reward set.
	/// Takes care of completing the reward set if all rewards have been obtained.
	/// </summary>
	private async Task<bool> SelectRewardForPlayer(RewardsSetState setState, Reward reward)
	{
		_logger.Debug($"Selecting reward {reward} for player {reward.Player.NetId}");
		bool result = await reward.SelectUnsynchronized();
		CompleteRewardsSetIfNecessary(setState);
		return result;
	}

	/// <summary>
	/// If all rewards in the passed state are successfully complete, then the set is marked as complete.
	/// </summary>
	private void CompleteRewardsSetIfNecessary(RewardsSetState setState)
	{
		if (setState.set.AllRewardsSuccessfullySelected)
		{
			CompleteRewardsSet(setState, RewardSetCompleteState.Completed);
		}
	}

	/// <summary>
	/// Skips all rewards in reward set on the top of the stack for the given player, completing the reward set.
	/// </summary>
	private RewardsSetState SkipRewardsSetOnStackTopForPlayer(Player player)
	{
		PlayerRewardState rewardStateForPlayer = GetRewardStateForPlayer(player);
		if (rewardStateForPlayer.rewardsStack.Count <= 0)
		{
			throw new InvalidOperationException($"Tried to skip reward set for player {player.NetId}, but they are not currently viewing any reward set!");
		}
		List<RewardsSetState> rewardsStack = rewardStateForPlayer.rewardsStack;
		RewardsSetState rewardsSetState = rewardsStack[rewardsStack.Count - 1];
		SkipRewardsSet(rewardsSetState);
		return rewardsSetState;
	}

	/// <summary>
	/// Skips all rewards in the passed reward set, completing the reward set.
	/// </summary>
	private void SkipRewardsSet(RewardsSetState setState)
	{
		foreach (Reward reward in setState.set.Rewards)
		{
			if (!reward.SuccessfullySelected)
			{
				reward.OnSkipped();
			}
		}
		CompleteRewardsSet(setState, RewardSetCompleteState.Skipped);
	}

	/// <summary>
	/// Marks a rewards set as complete (or skipped), popping it off the owning player's stack.
	/// </summary>
	private void CompleteRewardsSet(RewardsSetState setState, RewardSetCompleteState completeState)
	{
		PlayerRewardState rewardStateForPlayer = GetRewardStateForPlayer(setState.set.Player);
		if (IsRewardsSetCompleted(setState.set))
		{
			_logger.Error($"Reward set with id {setState.set} is already finished (state {rewardStateForPlayer.completedRewards[setState.set.Id]})!");
		}
		else
		{
			rewardStateForPlayer.rewardsStack.Remove(setState);
			rewardStateForPlayer.completedRewards[setState.set.Id] = completeState;
			setState.completionSource.SetResult();
			_logger.Debug($"Reward set {setState.set} completed with state: {completeState}");
		}
	}

	/// <summary>
	/// Returns true if the passed reward set has been completed.
	/// </summary>
	public bool IsRewardsSetCompleted(RewardsSet set)
	{
		PlayerRewardState rewardStateForPlayer = GetRewardStateForPlayer(set.Player);
		if (set.Id < 0)
		{
			return false;
		}
		return rewardStateForPlayer.completedRewards.ContainsKey(set.Id);
	}

	/// <summary>
	/// Returns true if the passed reward set ID has been completed for the given player.
	/// </summary>
	public bool IsRewardsSetCompleted(Player player, int id)
	{
		PlayerRewardState rewardStateForPlayer = GetRewardStateForPlayer(player);
		return rewardStateForPlayer.completedRewards.ContainsKey(id);
	}

	/// <summary>
	/// Skips all remaining rewards for the local player.
	/// </summary>
	public void BeforeLeavingRoom()
	{
		PlayerRewardState rewardStateForPlayer = GetRewardStateForPlayer(LocalPlayer);
		if (rewardStateForPlayer.rewardsStack.Count > 0)
		{
			_logger.Debug($"Skipping remaining rewards for local player {LocalPlayer.NetId} because we're exiting the room");
		}
		for (int num = rewardStateForPlayer.rewardsStack.Count - 1; num >= 0; num--)
		{
			RewardsSetState rewardsSetState = rewardStateForPlayer.rewardsStack[num];
			SkipRewardsSet(rewardStateForPlayer.rewardsStack[num]);
			RewardSetSkippedMessage message = new RewardSetSkippedMessage
			{
				location = _messageBuffer.CurrentLocation,
				setId = rewardsSetState.set.Id
			};
			_netService.SendMessage(message);
		}
		rewardStateForPlayer.rewardsStack.Clear();
		this.RewardsSkippedDuringRoomExit?.Invoke();
	}

	/// <summary>
	/// Gets all the next reward IDs in player slot order.
	/// </summary>
	[IteratorStateMachine(typeof(_003CGetNextRewardIds_003Ed__32))]
	public IEnumerable<int> GetNextRewardIds()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetNextRewardIds_003Ed__32(-2)
		{
			_003C_003E4__this = this
		};
	}

	public void FastForwardRewardIds(List<int> rewardIds)
	{
		for (int i = 0; i < rewardIds.Count; i++)
		{
			_logger.Debug($"Fast-forwarded reward set ID for {i} to {rewardIds[i]}");
			_rewardStates[i].nextId = rewardIds[i];
		}
	}
}
