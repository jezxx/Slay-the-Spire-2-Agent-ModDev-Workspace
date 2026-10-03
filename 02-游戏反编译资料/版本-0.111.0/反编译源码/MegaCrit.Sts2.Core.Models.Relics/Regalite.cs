using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Regalite : RelicModel
{
	private bool _usedThisTurn;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(4m, ValueProp.Unpowered));

	private bool UsedThisTurn
	{
		get
		{
			return _usedThisTurn;
		}
		set
		{
			AssertMutable();
			_usedThisTurn = value;
		}
	}

	public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
	{
		if (creator != null && creator == base.Owner && !UsedThisTurn)
		{
			UsedThisTurn = true;
			Flash();
			await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, null, fast: true);
		}
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner.Creature))
		{
			return Task.CompletedTask;
		}
		UsedThisTurn = false;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		UsedThisTurn = false;
		return Task.CompletedTask;
	}
}
