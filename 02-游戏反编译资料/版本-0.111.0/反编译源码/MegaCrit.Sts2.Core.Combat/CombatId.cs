namespace MegaCrit.Sts2.Core.Combat;

/// <summary>
/// Identifies one combat. Handed out by <see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.CurrentCombatId" /> and captured by callers at the
/// start of work that can outlive its combat (a card or potion effect, a kill), then passed back in so the manager
/// can tell whether the combat that work belongs to is still the current one.
///
/// Deliberately an opaque value rather than the combat's state or turn state: it cannot be dereferenced, cannot go
/// null partway through an effect the way <see cref="P:MegaCrit.Sts2.Core.Models.CardModel.CombatState" /> can, and gives a caller outside
/// the assembly no way to reach into combat internals.
/// </summary>
public readonly record struct CombatId(int Value)
{
	public override string ToString()
	{
		return Value.ToString();
	}
}
