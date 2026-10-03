using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class BurrowedPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool ShouldClearBlock(Creature creature)
	{
		if (base.Owner != creature)
		{
			return true;
		}
		return false;
	}

	public override async Task AfterBlockBroken(PlayerChoiceContext choiceContext, Creature target, Creature? breaker)
	{
		if (target == base.Owner)
		{
			MonsterModel monster = target.Monster;
			if (monster is Tunneler tunneler)
			{
				await tunneler.GetStunned();
				await CreatureCmd.Stun(base.Owner, tunneler.StillDizzyMove, "BITE_MOVE");
				await PowerCmd.Remove<BurrowedPower>(base.Owner);
			}
		}
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		await CreatureCmd.LoseBlock(new BlockingPlayerChoiceContext(), oldOwner, 999999999m, null);
	}
}
