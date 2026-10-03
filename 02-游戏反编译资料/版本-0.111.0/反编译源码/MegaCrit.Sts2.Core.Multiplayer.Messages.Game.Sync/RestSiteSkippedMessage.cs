using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;

/// <summary>
/// Sent when a player is exiting a rest site with options remaining.
/// When a rest site room is exiting, players must wait to receive this message from players who still have options
/// remaining before generating the room-exit checksum. Otherwise, there may be a race between that checksum and another
/// player's rest site choice.
/// </summary>
public struct RestSiteSkippedMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public RunLocation Location { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.Write(Location);
	}

	public void Deserialize(PacketReader reader)
	{
		Location = reader.Read<RunLocation>();
	}
}
