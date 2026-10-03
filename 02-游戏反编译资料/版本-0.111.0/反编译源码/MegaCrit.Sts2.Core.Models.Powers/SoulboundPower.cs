using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SoulboundPower : PowerModel
{
	private bool _isAddingSoul;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Soul>());

	private bool IsAddingSoul
	{
		get
		{
			return _isAddingSoul;
		}
		set
		{
			AssertMutable();
			_isAddingSoul = value;
		}
	}

	public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
	{
		if (creator != null && creator.Creature == base.Applier && card is Soul && !IsAddingSoul)
		{
			IsAddingSoul = true;
			Flash();
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Soul.Create(base.Owner.Player, base.Amount, base.CombatState), PileType.Draw, base.Owner.Player, CardPilePosition.Random));
			IsAddingSoul = false;
		}
	}
}
