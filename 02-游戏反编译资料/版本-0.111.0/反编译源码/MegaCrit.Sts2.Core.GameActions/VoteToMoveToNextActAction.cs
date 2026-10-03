using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions;

/// <summary>
/// An action enqueued at the rewards screen when the player is ready to move to the next act.
/// Once a player receives actions from all other players indicating that they're ready to move to the next act, then
/// the player should begin transitioning to the next act.
/// </summary>
public class VoteToMoveToNextActAction : GameAction
{
	/// <summary>
	/// The player who is voting.
	/// </summary>
	private readonly Player _player;

	/// <summary>
	/// The current act index.
	/// If the current act is not this or we are already transitioning from this act, then this action is ignored.
	/// </summary>
	public int CurrentActIndex { get; }

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	public VoteToMoveToNextActAction(Player player, int currentActIndex)
	{
		_player = player;
		CurrentActIndex = currentActIndex;
	}

	protected override Task ExecuteAction()
	{
		RunManager.Instance.ActChangeSynchronizer.OnPlayerReady(_player, CurrentActIndex);
		return Task.CompletedTask;
	}

	public override INetAction ToNetAction()
	{
		NetVoteToMoveToNextActAction netVoteToMoveToNextActAction = default(NetVoteToMoveToNextActAction);
		netVoteToMoveToNextActAction.currentActIndex = CurrentActIndex;
		return netVoteToMoveToNextActAction;
	}

	public override string ToString()
	{
		return $"{"VoteToMoveToNextActAction"} {_player.NetId} act {CurrentActIndex}";
	}
}
