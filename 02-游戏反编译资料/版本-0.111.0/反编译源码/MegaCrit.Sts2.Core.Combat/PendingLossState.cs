using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Combat;

/// <summary>
/// A combat loss waiting to be processed at the next safe point (see <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.CheckWinCondition" />).
/// </summary>
internal sealed record PendingLossState(CombatRoom Room);
