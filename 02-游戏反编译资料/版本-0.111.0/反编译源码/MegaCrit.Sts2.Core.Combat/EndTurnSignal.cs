using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;

namespace MegaCrit.Sts2.Core.Combat;

/// <summary>
/// Payload of <see cref="P:MegaCrit.Sts2.Core.Combat.CombatTurnState.EndTurnSignalSource" />: the player-driven action that triggered the end of
/// turn (which must finish before the end-of-turn sequence begins, e.g. Void Form's card play), the turn the
/// end-turn was scheduled for (see the stamping comment in <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.SetReadyToEndTurn(MegaCrit.Sts2.Core.Entities.Players.Player,System.Boolean,System.Func{System.Threading.Tasks.Task})" />), and the
/// optional test hook to run during the enemy turn. The combat needs no stamp: the signal lives on one combat's
/// turn state, so it can only ever be consumed by that combat's turn loop.
/// </summary>
internal sealed record EndTurnSignal(GameAction? RunningAction, int ScheduledTurnNumber, Player ScheduledPlayer, Func<Task>? ActionDuringEnemyTurn);
