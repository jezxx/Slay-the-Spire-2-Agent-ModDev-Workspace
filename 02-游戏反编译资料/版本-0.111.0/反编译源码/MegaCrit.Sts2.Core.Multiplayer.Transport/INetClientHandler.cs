using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport;

public interface INetClientHandler : INetHandler
{
	void Initialize(NetClient client, PlatformType platformType);

	void OnConnectedToHost();

	void OnDisconnectedFromHost(ulong hostNetId, NetErrorInfo info);
}
