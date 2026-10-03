namespace MegaCrit.Sts2.Core.Models;

public interface ITemporaryPower
{
	/// <summary>
	/// The canonical model that applies this power.
	/// </summary>
	/// <example><see cref="T:MegaCrit.Sts2.Core.Models.Potions.FlexPotion" /> for <see cref="T:MegaCrit.Sts2.Core.Models.Powers.FlexPotionPower" /></example>
	AbstractModel OriginModel { get; }

	/// <summary>
	/// The canonical power that this power internally applies.
	/// </summary>
	/// <example><see cref="T:MegaCrit.Sts2.Core.Models.Powers.StrengthPower" /> for <see cref="T:MegaCrit.Sts2.Core.Models.Powers.TemporaryStrengthPower" /></example>
	PowerModel InternallyAppliedPower { get; }
}
