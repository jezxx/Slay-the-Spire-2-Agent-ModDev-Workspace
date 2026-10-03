using System;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

/// <summary>
/// Provides additional client-related methods on top of the default game service.
/// </summary>
public interface INetClientGameService : INetGameService, INetClientHandler, INetHandler
{
	NetClient? NetClient { get; }

	event Action<NetErrorInfo>? ConnectionFailed;
}
