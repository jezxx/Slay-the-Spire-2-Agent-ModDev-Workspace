using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class HistoryCourse : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Event;

	public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner && base.Owner.PlayerCombatState.TurnNumber != 1)
		{
			CardModel cardModel = CombatManager.Instance.History.CardPlaysFinished.LastOrDefault((CardPlayFinishedEntry e) => e.CardPlay.Player == base.Owner && e.HappenedLastPlayerTurn(base.Owner) && e.CardPlay.Card.Type == CardType.Attack && !e.CardPlay.Card.IsDupe)?.CardPlay.Card;
			if (cardModel != null)
			{
				Flash();
				await CardCmd.AutoPlay(choiceContext, cardModel.CreateDupe(player), null);
			}
		}
	}
}
