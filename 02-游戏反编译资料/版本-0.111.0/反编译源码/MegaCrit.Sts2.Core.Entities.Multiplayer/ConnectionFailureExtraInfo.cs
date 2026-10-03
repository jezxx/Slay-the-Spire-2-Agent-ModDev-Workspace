using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Multiplayer;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

/// <summary>
/// Class for holding extra info to display in multiplayer errors if the error is a ConnectionFailureReason.
/// </summary>
public record ConnectionFailureExtraInfo
{
	public PeerVersionInfo localInfo;

	public PeerVersionInfo remoteInfo;

	public bool localIsHost;

	public List<string> GetMissingModsOnLocal(bool nonGameplay)
	{
		if (!nonGameplay)
		{
			return remoteInfo.gameplayAffectingMods?.Except(localInfo.gameplayAffectingMods ?? new List<string>()).ToList() ?? new List<string>();
		}
		return remoteInfo.otherMods?.Except(localInfo.otherMods ?? new List<string>()).ToList() ?? new List<string>();
	}

	public List<string> GetMissingModsOnRemote(bool nonGameplay)
	{
		if (!nonGameplay)
		{
			return localInfo.gameplayAffectingMods?.Except(remoteInfo.gameplayAffectingMods ?? new List<string>()).ToList() ?? new List<string>();
		}
		return localInfo.otherMods?.Except(remoteInfo.otherMods ?? new List<string>()).ToList() ?? new List<string>();
	}
}
