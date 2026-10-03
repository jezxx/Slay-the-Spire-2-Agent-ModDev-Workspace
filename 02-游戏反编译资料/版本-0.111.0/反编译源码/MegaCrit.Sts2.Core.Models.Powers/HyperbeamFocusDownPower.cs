using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class HyperbeamFocusDownPower : TemporaryFocusPower
{
	public override AbstractModel OriginModel => ModelDb.Card<Hyperbeam>();

	protected override bool IsPositive => false;
}
