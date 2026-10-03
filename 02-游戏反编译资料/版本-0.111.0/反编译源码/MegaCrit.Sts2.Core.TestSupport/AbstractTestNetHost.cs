using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.TestSupport;

public abstract class AbstractTestNetHost : NetHost
{
	protected AbstractTestNetHost(INetHostHandler handler)
		: base(handler)
	{
	}

	public abstract Task<NetErrorInfo?> StartHost(int maxClients);
}
