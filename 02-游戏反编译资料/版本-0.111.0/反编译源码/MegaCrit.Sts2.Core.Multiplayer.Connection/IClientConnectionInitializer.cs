using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace MegaCrit.Sts2.Core.Multiplayer.Connection;

public interface IClientConnectionInitializer
{
	Task<NetErrorInfo?> Connect(INetClientGameService netService, CancellationToken cancelToken = default(CancellationToken));
}
