using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class NostalgiaPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation location)
	{
		if (card.Owner.Creature != base.Owner)
		{
			return location;
		}
		CardType type = card.Type;
		if ((uint)(type - 1) > 1u)
		{
			return location;
		}
		if (location.pileType != PileType.Discard)
		{
			return location;
		}
		int num = CombatManager.Instance.History.CardPlaysStarted.Count(delegate(CardPlayStartedEntry e)
		{
			bool flag = e.HappenedThisTurn(base.CombatState);
			bool flag2 = flag;
			if (flag2)
			{
				CardType type2 = e.CardPlay.Card.Type;
				bool flag3 = (uint)(type2 - 1) <= 1u;
				flag2 = flag3;
			}
			return flag2 && e.CardPlay.Player == base.Owner.Player;
		});
		if (num >= base.Amount)
		{
			return location;
		}
		location.pileType = PileType.Draw;
		location.position = CardPilePosition.Top;
		return location;
	}

	public override Task AfterModifyingCardPlayResultLocation(CardModel card, CardLocation location)
	{
		if (card.Owner.Creature != base.Owner)
		{
			return Task.CompletedTask;
		}
		Flash();
		return Task.CompletedTask;
	}
}
