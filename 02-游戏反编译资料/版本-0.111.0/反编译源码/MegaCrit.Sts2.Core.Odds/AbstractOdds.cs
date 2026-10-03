using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Odds;

public abstract class AbstractOdds
{
	protected readonly Rng _rng;

	public float CurrentValue { get; protected set; }

	protected AbstractOdds(float initialValue, Rng rng)
	{
		CurrentValue = initialValue;
		_rng = rng;
		base._002Ector();
	}

	/// <summary>
	/// Sets CurrentValue. Should be used only in instances where we're loading the player from another source, such as
	/// multiplayer host.
	/// </summary>
	/// <param name="newValue">The new value.</param>
	public void OverrideCurrentValue(float newValue)
	{
		CurrentValue = newValue;
	}
}
