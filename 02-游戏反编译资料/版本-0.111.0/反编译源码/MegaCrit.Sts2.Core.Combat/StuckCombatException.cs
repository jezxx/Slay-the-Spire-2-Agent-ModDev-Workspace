using System;

namespace MegaCrit.Sts2.Core.Combat;

/// <summary>
/// Reported when a combat's turn loop dies while the combat is still in progress, leaving the player unable to act.
///
/// The underlying exception already reaches Sentry via <see cref="M:MegaCrit.Sts2.Core.Helpers.TaskHelper.RunSafely(System.Threading.Tasks.Task)" />, but as whatever it
/// happened to be, with nothing marking it as having stopped a combat. This wrapper gives those their own issue, the
/// way <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Game.StateDivergenceException" /> does for divergences.
/// </summary>
public class StuckCombatException : Exception
{
	public StuckCombatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
