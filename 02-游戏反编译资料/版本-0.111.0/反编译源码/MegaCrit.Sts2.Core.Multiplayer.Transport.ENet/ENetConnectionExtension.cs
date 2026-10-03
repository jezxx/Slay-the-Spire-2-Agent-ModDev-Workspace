using Godot;
using Godot.Collections;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;

public static class ENetConnectionExtension
{
	/// <summary>
	/// Polls an ENetConnection, returning the events that have occurred since the last poll.
	/// This wraps ENet.Service into a more digestible method.
	/// </summary>
	/// <param name="connection">The connection to poll.</param>
	/// <param name="output">The event we polled from the connection, if any.</param>
	/// <returns></returns>
	public static bool TryService(this ENetConnection connection, out ENetServiceData? output)
	{
		Array array = connection.Service();
		output = null;
		if (array == null)
		{
			return false;
		}
		ENetConnection.EventType eventType = array[0].As<ENetConnection.EventType>();
		if (eventType == ENetConnection.EventType.None)
		{
			return false;
		}
		ENetServiceData eNetServiceData = default(ENetServiceData);
		eNetServiceData.type = eventType;
		eNetServiceData.peer = array[1].As<ENetPacketPeer>();
		eNetServiceData.originalData = array;
		ENetServiceData value = eNetServiceData;
		if (eventType == ENetConnection.EventType.Receive)
		{
			value.channel = array[3].As<int>();
			value.packetData = value.peer.GetPacket();
			value.error = value.peer.GetPacketError();
			value.mode = NetTransferMode.None;
		}
		output = value;
		return true;
	}
}
