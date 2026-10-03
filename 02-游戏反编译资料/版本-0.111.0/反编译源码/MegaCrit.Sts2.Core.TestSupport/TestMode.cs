using System.Linq;
using Godot;

namespace MegaCrit.Sts2.Core.TestSupport;

public static class TestMode
{
	/// <summary>
	/// Whether the game is running in test mode.
	/// True when we're running unit tests, true when we're running the normal game.
	/// </summary>
	public static bool IsOn { get; set; }

	/// <summary>
	/// Whether the game iS NOT running in test mode.
	/// True when we're running the normal game, false when we're running unit tests.
	/// </summary>
	public static bool IsOff => !IsOn;

	/// <summary>
	/// Whether this process was launched by a test runner, derived from the command line rather than
	/// <see cref="P:MegaCrit.Sts2.Core.TestSupport.TestMode.IsOn" />. The runner scenes (CiCoreRunner/NetCoreRunner, both under RiderTestRunner/) set
	/// <see cref="P:MegaCrit.Sts2.Core.TestSupport.TestMode.IsOn" /> from the main scene, which enters the tree AFTER autoloads, so <see cref="P:MegaCrit.Sts2.Core.TestSupport.TestMode.IsOn" /> is
	/// still false during autoload _EnterTree. Code that runs that early (Logger's printer choice, Sentry
	/// bootstrap) must use this instead.
	/// </summary>
	public static bool IsTestRunFromCmdline()
	{
		return OS.GetCmdlineArgs().Any((string arg) => arg.Contains("RiderTestRunner/"));
	}

	public static void AssertOn()
	{
		if (IsOn)
		{
			return;
		}
		throw new TestModeOffException();
	}

	public static void AssertOff()
	{
		if (IsOff)
		{
			return;
		}
		throw new TestModeOnException();
	}

	/// <summary>
	/// NEVER CALL THIS. Only calls should be in NetCoreRunner and CiCoreRunner.
	/// </summary>
	public static void TurnOnInternal()
	{
		IsOn = true;
	}
}
