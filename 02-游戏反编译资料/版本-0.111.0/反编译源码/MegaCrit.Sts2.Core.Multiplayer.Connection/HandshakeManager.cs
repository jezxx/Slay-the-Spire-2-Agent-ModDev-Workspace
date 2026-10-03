using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Multiplayer.Connection;

public class HandshakeManager
{
	private class HandshakeInProgress
	{
		public ulong peerId;

		public required CancellationTokenSource cts;
	}

	private const int _handshakeTimeoutMsec = 10000;

	public static TaskCompletionSource? skipTimeoutTcs;

	public static readonly byte[] magicBytes = "STS2"u8.ToArray();

	private readonly Logger _logger;

	private readonly IHandshakeHandler _handler;

	private readonly PacketWriter _writer;

	private PeerVersionInfo _localVersionInfo;

	private List<HandshakeInProgress> _handshakesInProgress = new List<HandshakeInProgress>();

	public HandshakeManager(IHandshakeHandler handler, PeerVersionInfo localVersionInfo, PacketWriter writer)
	{
		_logger = new Logger("HandshakeManager", LogType.Network);
		_localVersionInfo = localVersionInfo;
		_handler = handler;
		_writer = writer;
	}

	public bool IsHandshaking(ulong senderId)
	{
		return _handshakesInProgress.Any((HandshakeInProgress h) => h.peerId == senderId);
	}

	public void HandshakeMessageReceived(ulong senderId, PacketReader reader)
	{
		HandshakeResult handshakeResult = TryReadHandshakeMessage(senderId, reader);
		if (handshakeResult.status != HandshakeStatus.InvalidHandshake)
		{
			_handshakesInProgress.RemoveAll((HandshakeInProgress h) => h.peerId == senderId);
			if (_handler.Type == NetGameType.Client)
			{
				WriteHandshakeMessage(senderId);
			}
		}
		if (handshakeResult.status == HandshakeStatus.Success)
		{
			_handler.HandshakeSucceeded(senderId, handshakeResult.remoteVersionInfo.Value);
		}
		else if (handshakeResult.status == HandshakeStatus.VersionMismatch)
		{
			_handler.HandshakeFailed(senderId, new NetErrorInfo(ConnectionFailureReason.VersionMismatch, handshakeResult.extraInfo));
		}
		else if (handshakeResult.status == HandshakeStatus.ModMismatch)
		{
			_handler.HandshakeFailed(senderId, new NetErrorInfo(ConnectionFailureReason.ModMismatch, handshakeResult.extraInfo));
		}
		else if (handshakeResult.status != HandshakeStatus.InvalidHandshake)
		{
			throw new ArgumentOutOfRangeException();
		}
	}

	private HandshakeResult TryReadHandshakeMessage(ulong senderId, PacketReader reader)
	{
		if (reader.Buffer.Length < magicBytes.Length)
		{
			return new HandshakeResult(HandshakeStatus.InvalidHandshake);
		}
		byte[] array = magicBytes;
		foreach (byte b in array)
		{
			if (reader.ReadByte() != b)
			{
				return new HandshakeResult(HandshakeStatus.InvalidHandshake);
			}
		}
		HandshakeInProgress handshakeInProgress = _handshakesInProgress.Find((HandshakeInProgress h) => h.peerId == senderId);
		if (handshakeInProgress == null)
		{
			Log.Warn($"Received handshake message for {senderId} who is not currently in the middle of a handshake!");
			return new HandshakeResult(HandshakeStatus.InvalidHandshake);
		}
		handshakeInProgress.cts.Cancel();
		PeerVersionInfo peerVersionInfo = default(PeerVersionInfo);
		if (!peerVersionInfo.TryDeserialize(reader))
		{
			Log.Warn($"Got a partial read on peer {senderId}'s handshake! Attempting to proceed");
		}
		ConnectionFailureExtraInfo connectionFailureExtraInfo = new ConnectionFailureExtraInfo
		{
			localInfo = _localVersionInfo,
			remoteInfo = peerVersionInfo,
			localIsHost = (_handler.Type != NetGameType.Client)
		};
		_logger.Info($"Got handshake from sender {senderId}. Version: {peerVersionInfo.version} Branch: {peerVersionInfo.branch} Hash: {peerVersionInfo.idDatabaseHash}");
		if (peerVersionInfo.version != _localVersionInfo.version)
		{
			Log.Error($"Version mismatch. Remote: {peerVersionInfo.version} Ours: {_localVersionInfo.version} Remote branch: {peerVersionInfo.branch}");
			return new HandshakeResult(HandshakeStatus.VersionMismatch, peerVersionInfo, connectionFailureExtraInfo);
		}
		List<string> missingModsOnRemote = connectionFailureExtraInfo.GetMissingModsOnRemote(nonGameplay: false);
		List<string> missingModsOnLocal = connectionFailureExtraInfo.GetMissingModsOnLocal(nonGameplay: false);
		if (missingModsOnLocal.Count > 0 || missingModsOnRemote.Count > 0)
		{
			_logger.Warn($"Mismatch in gameplay-relevant mods with the remote!\nMods that remote has that local does not: {string.Join(",", missingModsOnLocal)}.\nMods that local has that remote does not: {string.Join(",", missingModsOnRemote)}.");
			return new HandshakeResult(HandshakeStatus.ModMismatch, peerVersionInfo, connectionFailureExtraInfo);
		}
		if (peerVersionInfo.idDatabaseHash != _localVersionInfo.idDatabaseHash)
		{
			_logger.Warn("Our version " + _localVersionInfo.version + " matches the remote, but our Model ID hash does not!");
			return new HandshakeResult(HandshakeStatus.VersionMismatch, peerVersionInfo, connectionFailureExtraInfo);
		}
		List<string> missingModsOnRemote2 = connectionFailureExtraInfo.GetMissingModsOnRemote(nonGameplay: true);
		List<string> missingModsOnLocal2 = connectionFailureExtraInfo.GetMissingModsOnLocal(nonGameplay: true);
		if (missingModsOnRemote2.Count > 0 || missingModsOnLocal2.Count > 0)
		{
			_logger.Warn($"Mismatch in non-gameplay relevant mods. This is allowed, but it's up to the mod authors to guarantee that it doesn't break anything.\nNon-gameplay relevant mods that remote has that local does not: {string.Join(",", missingModsOnLocal2)}.\nNon-gameplay relevant mods that local has that remote does not: {string.Join(",", missingModsOnRemote2)}.");
		}
		return new HandshakeResult(HandshakeStatus.Success, peerVersionInfo, connectionFailureExtraInfo);
	}

	public void WriteHandshakeMessage(ulong peerId)
	{
		WriteHandshakeMessage(peerId, _localVersionInfo, _writer);
		_handler.SendHandshakeMessage(peerId, _writer);
	}

	public static void WriteHandshakeMessage(ulong peerId, PeerVersionInfo versionInfo, PacketWriter writer)
	{
		writer.Reset();
		writer.WriteBytes(magicBytes, magicBytes.Length);
		versionInfo.Serialize(writer);
	}

	public void BeginHandshakeFor(ulong peerId)
	{
		if (IsHandshaking(peerId))
		{
			throw new InvalidOperationException($"Tried to begin handshake for {peerId} who is already handshaking!");
		}
		CancellationTokenSource cts = new CancellationTokenSource();
		_handshakesInProgress.Add(new HandshakeInProgress
		{
			peerId = peerId,
			cts = cts
		});
		if (_handler.Type == NetGameType.Host)
		{
			WriteHandshakeMessage(peerId);
		}
		TaskHelper.RunSafely(TimeoutIfHandshakeNotReceived(peerId, cts));
	}

	public async Task TimeoutIfHandshakeNotReceived(ulong peerId, CancellationTokenSource cts)
	{
		Task task = Task.Delay(10000, cts.Token);
		if (skipTimeoutTcs == null)
		{
			await task;
		}
		else
		{
			await TaskHelper.WhenAny(task, skipTimeoutTcs.Task);
		}
		if (!cts.IsCancellationRequested)
		{
			AbortHandshake(peerId);
			_handler.HandshakeFailed(peerId, new NetErrorInfo(ConnectionFailureReason.HandshakeTimeout));
		}
	}

	public void AbortHandshake(ulong peerId)
	{
		for (int i = 0; i < _handshakesInProgress.Count; i++)
		{
			if (_handshakesInProgress[i].peerId == peerId)
			{
				_handshakesInProgress[i].cts.Cancel();
				_handshakesInProgress.RemoveAt(i);
				i--;
			}
		}
	}
}
