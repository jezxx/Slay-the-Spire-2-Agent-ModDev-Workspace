using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions;

public struct NetVoteToMoveToNextActAction : INetAction, IPacketSerializable
{
	public int currentActIndex;

	public GameAction ToGameAction(Player player)
	{
		return new VoteToMoveToNextActAction(player, currentActIndex);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(currentActIndex);
	}

	public void Deserialize(PacketReader reader)
	{
		currentActIndex = reader.ReadInt();
	}

	public override string ToString()
	{
		return $"{"NetVoteToMoveToNextActAction"} act {currentActIndex}";
	}
}
