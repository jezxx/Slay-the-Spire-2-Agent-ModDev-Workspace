using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class AmbergrisPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override bool IsVisibleInternal => false;

	public override bool ShouldTakeExtraTurn(Player player)
	{
		if (base.Amount > 0)
		{
			return player == base.Owner.Player;
		}
		return false;
	}

	public override async Task AfterTakingExtraTurn(Player player)
	{
		if (player == base.Owner.Player)
		{
			await PowerCmd.Decrement(this);
		}
	}
}
