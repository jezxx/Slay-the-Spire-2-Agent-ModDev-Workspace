using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public record struct CardLocation
{
	public Player player;

	public PileType pileType;

	public CardPilePosition position;

	public CardLocation(Player player, PileType pileType, CardPilePosition position)
	{
		this.player = player;
		this.pileType = pileType;
		this.position = position;
	}
}
