using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

/// <summary>
/// Test-only power that holds a player turn in its <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.Start" /> phase until a test
/// completes <see cref="P:MegaCrit.Sts2.Core.Models.Powers.Mocks.MockGatedTurnStartPower.Gate" />.
/// </summary>
/// <remarks>AfterPlayerTurnStart is the last await in SetupPlayerTurn, so blocking here holds the phase.</remarks>
public sealed class MockGatedTurnStartPower : PowerModel
{
	/// <summary><see cref="P:MegaCrit.Sts2.Core.Models.Powers.Mocks.MockGatedTurnStartPower.Gate" /> is cleared as the turn is held, so the live one is kept here to be released.</summary>
	private static TaskCompletionSource? _heldGate;

	public override bool IsMock => true;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	/// <summary>Set by a test. Only the first turn start after it is set gets held.</summary>
	public static TaskCompletionSource? Gate { get; set; }

	/// <summary>Completed once the turn start is actually held, so a test can await it instead of polling.</summary>
	public static TaskCompletionSource? Held { get; set; }

	/// <remarks>Releases a held gate; dropping it leaves SetupPlayerTurn awaiting a task nobody can complete.</remarks>
	public static void Reset()
	{
		Gate?.TrySetResult();
		_heldGate?.TrySetResult();
		Held?.TrySetResult();
		Gate = null;
		_heldGate = null;
		Held = null;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner.Player)
		{
			TaskCompletionSource gate = Gate;
			if (gate != null)
			{
				Gate = null;
				_heldGate = gate;
				Held?.TrySetResult();
				await gate.Task;
			}
		}
	}
}
