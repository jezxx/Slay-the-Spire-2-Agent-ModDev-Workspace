using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public struct CardPileAddResult
{
	/// <summary>
	/// Whether we were successful in adding the card to a pile.
	/// </summary>
	public bool success;

	/// <summary>
	/// The card that was added to a pile.
	/// </summary>
	public CardModel cardAdded;

	/// <summary>
	/// The old pile of the card, if any. If null, the card may have been generated anew.
	/// </summary>
	public CardPile? oldPile;

	/// <summary>
	/// The target pile we tried to add the card to.
	/// Currently, the only way this is different from cardAdded.Pile.Type is when we tried to add the card to a full
	/// hand and it got sent to the discard.
	/// </summary>
	public PileType targetPile;

	/// <summary>
	/// The models that were involved in modifying the result pile.
	/// </summary>
	public List<AbstractModel>? modifyingModels;
}
