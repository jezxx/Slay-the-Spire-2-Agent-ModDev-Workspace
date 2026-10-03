using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Cards.Mocks;

/// <summary>
/// Test-only card with a turn-end-in-hand effect that counts how many times it fires. The turn-end card block in
/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.DoTurnEnd(MegaCrit.Sts2.Core.Combat.CombatTurnState,MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext)" /> runs only when that method's turn-state-relative liveness read decides the
/// combat is still live, so this counter observes whether a stale <c>DoTurnEnd</c> proceeds past that read against
/// a freshly started next combat.
/// </summary>
public sealed class MockTurnEndInHandRecorderCard : MockCardModel
{
	/// <summary>Number of times this card's turn-end-in-hand effect has fired since it was last reset.</summary>
	public static int FireCount { get; set; }

	public override CardType Type => CardType.Skill;

	public override TargetType TargetType => TargetType.Self;

	public override bool HasTurnEndInHandEffect => true;

	protected override int GetBaseBlock()
	{
		return 0;
	}

	public override MockCardModel MockBlock(int block)
	{
		return this;
	}

	protected override Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
	{
		FireCount++;
		return Task.CompletedTask;
	}
}
