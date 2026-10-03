using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Orbs.Mock;

/// <summary>
/// Test-only orb whose end-of-turn trigger suspends the turn loop inside
/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.DoTurnEnd(MegaCrit.Sts2.Core.Combat.CombatTurnState,MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext)" />, before its turn-state-relative liveness read. The orb queue's
/// <c>BeforeTurnEnd</c> runs inside <c>DoTurnEnd</c> ahead of that read, so a test can tear the combat down while the
/// turn loop is suspended here and then let the read run to prove it never acts on a freshly started next combat.
/// </summary>
public class MockTurnEndSuspendOrb : OrbModel
{
	/// <summary>Completed by the orb when the turn loop has suspended inside <c>DoTurnEnd</c>.</summary>
	public static TaskCompletionSource? Suspended { get; set; }

	/// <summary>Awaited by the orb; the test completes it to let the turn loop run the liveness read.</summary>
	public static TaskCompletionSource? Release { get; set; }

	public override bool IsMock => true;

	public override decimal PassiveVal => 0m;

	public override decimal EvokeVal => 0m;

	public override Color DarkenedColor => new Color("000000");

	public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
	{
		Suspended?.TrySetResult();
		if (Release != null)
		{
			await Release.Task;
		}
	}

	public override Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
	{
		return Task.FromResult((IEnumerable<Creature>)Array.Empty<Creature>());
	}
}
