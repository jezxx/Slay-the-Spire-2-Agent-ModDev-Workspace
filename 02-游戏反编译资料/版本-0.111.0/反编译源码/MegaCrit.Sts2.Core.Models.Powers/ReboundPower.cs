using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ReboundPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation location)
	{
		if (card.Owner.Creature != base.Owner)
		{
			return location;
		}
		if (location.pileType != PileType.Discard)
		{
			return location;
		}
		location.pileType = PileType.Draw;
		location.position = CardPilePosition.Top;
		return location;
	}

	public override async Task AfterModifyingCardPlayResultLocation(CardModel card, CardLocation location)
	{
		if (card.Owner.Creature == base.Owner)
		{
			Flash();
			await PowerCmd.Decrement(this);
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner))
		{
			await PowerCmd.Remove(this);
		}
	}
}
