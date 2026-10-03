using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Multiplayer;

/// <summary>
/// Sent during the handshake between host and client.
/// This is the only struct of its kind that must NOT change format significantly. It must stay stable enough between
/// versions that we can detect version mismatches, i.e. we should only add to this and never remove.
/// </summary>
public struct PeerVersionInfo
{
	/// <summary>
	/// The version of the game the host is running.
	/// </summary>
	public string version;

	/// <summary>
	/// The steam branch that the host is playing on.
	/// DO NOT compare this to the client's when joining to check versions. The same game version might be on different
	/// steam branches. Only compare <see cref="F:MegaCrit.Sts2.Core.Multiplayer.PeerVersionInfo.version" /> and only use this for diagnostic purposes.
	/// </summary>
	public PlatformBranch branch;

	/// <summary>
	/// A hash of all the IDs in the model database.
	/// </summary>
	public uint idDatabaseHash;

	/// <summary>
	/// A list of all the gameplay-affecting mods that the host has installed, if any.
	/// </summary>
	public List<string>? gameplayAffectingMods;

	/// <summary>
	/// A list of all the non-gameplay-affecting mods that the host has installed, if any.
	/// </summary>
	public List<string>? otherMods;

	/// <summary>
	/// Returns a PeerVersionInfo with data filled in from various singletons.
	/// </summary>
	public static PeerVersionInfo LocalDefault()
	{
		PeerVersionInfo result = default(PeerVersionInfo);
		result.version = NGame.GetGameVersion();
		result.branch = PlatformUtil.GetPlatformBranch();
		result.idDatabaseHash = ModelIdSerializationCache.Hash;
		result.gameplayAffectingMods = ModManager.GetGameplayRelevantModNameList();
		result.otherMods = ModManager.GetNonGameplayRelevantModNameList();
		return result;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteString(version);
		writer.WriteEnum(branch);
		writer.WriteUInt(idDatabaseHash);
		writer.WriteBool(gameplayAffectingMods != null);
		if (gameplayAffectingMods != null)
		{
			writer.WriteInt(gameplayAffectingMods.Count);
			foreach (string gameplayAffectingMod in gameplayAffectingMods)
			{
				writer.WriteString(gameplayAffectingMod);
			}
		}
		writer.WriteBool(otherMods != null);
		if (otherMods == null)
		{
			return;
		}
		writer.WriteInt(otherMods.Count);
		foreach (string otherMod in otherMods)
		{
			writer.WriteString(otherMod);
		}
	}

	public bool TryDeserialize(PacketReader reader)
	{
		try
		{
			version = reader.ReadString();
			if (!SemanticVersion.TryFromString(version, out SemanticVersion _) && !OS.HasFeature("editor"))
			{
				return false;
			}
			branch = reader.ReadEnum<PlatformBranch>();
			idDatabaseHash = reader.ReadUInt();
			if (reader.ReadBool())
			{
				int num = reader.ReadInt();
				gameplayAffectingMods = new List<string>();
				for (int i = 0; i < num; i++)
				{
					gameplayAffectingMods.Add(reader.ReadString());
				}
			}
			if (reader.ReadBool())
			{
				int num2 = reader.ReadInt();
				otherMods = new List<string>();
				for (int j = 0; j < num2; j++)
				{
					otherMods.Add(reader.ReadString());
				}
			}
			return true;
		}
		catch (Exception value)
		{
			Log.Error($"Failed to deserialize ${"PeerVersionInfo"}! Attempting to proceed with a partial read. Exception: {value}");
			return false;
		}
	}

	public bool IsModded()
	{
		List<string>? list = gameplayAffectingMods;
		if (list == null || list.Count <= 0)
		{
			List<string>? list2 = otherMods;
			if (list2 == null)
			{
				return false;
			}
			return list2.Count > 0;
		}
		return true;
	}
}
