using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class FadePower : TemporaryDexterityPower
{
	public override AbstractModel OriginModel => ModelDb.Card<Fade>();
}
