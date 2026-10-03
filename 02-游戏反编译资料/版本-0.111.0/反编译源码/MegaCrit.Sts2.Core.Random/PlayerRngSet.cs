using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Rngs;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Random;

public class PlayerRngSet
{
	private readonly Dictionary<PlayerRngType, Rng> _rngs = new Dictionary<PlayerRngType, Rng>();

	/// <summary>
	/// Determines per-player reward generation. This means:
	/// * What cards are generated for rewards.
	/// * What potions are generated for rewards.
	/// * Whether or not the player gets a potion reward at all.
	/// </summary>
	public Rng Rewards => GetRng(PlayerRngType.Rewards);

	/// <summary>
	/// Determines what the different shops are selling.
	/// </summary>
	public Rng Shops => GetRng(PlayerRngType.Shops);

	/// <summary>
	/// Determines what a transformed card will roll into,
	/// </summary>
	public Rng Transformations => GetRng(PlayerRngType.Transformations);

	public ulong Seed { get; }

	public PlayerRngSet(ulong seed)
	{
		Seed = seed;
		PlayerRngType[] values = Enum.GetValues<PlayerRngType>();
		foreach (PlayerRngType playerRngType in values)
		{
			_rngs[playerRngType] = CreateRng(playerRngType);
		}
	}

	private Rng CreateRng(PlayerRngType rngType)
	{
		string name = StringHelper.SnakeCase(rngType.ToString());
		return new Rng(Seed, name);
	}

	public SerializablePlayerRngSet ToSerializable()
	{
		SerializablePlayerRngSet serializablePlayerRngSet = new SerializablePlayerRngSet
		{
			Seed = Seed
		};
		foreach (var (key, rng2) in _rngs)
		{
			serializablePlayerRngSet.Rngs[key] = rng2.ToSerializable();
		}
		return serializablePlayerRngSet;
	}

	public static PlayerRngSet FromSerializable(SerializablePlayerRngSet save)
	{
		PlayerRngSet playerRngSet = new PlayerRngSet(save.Seed);
		foreach (var (key, serializable) in save.Rngs)
		{
			playerRngSet._rngs[key] = new Rng(serializable);
		}
		return playerRngSet;
	}

	public void LoadFromSerializable(SerializablePlayerRngSet save)
	{
		if (Seed != save.Seed)
		{
			throw new NotImplementedException("RngSet seed should not change during the run!");
		}
		foreach (var (key, serializable) in save.Rngs)
		{
			_rngs[key].LoadFromSerializable(serializable);
		}
	}

	public Rng GetRng(PlayerRngType rngType)
	{
		return _rngs[rngType];
	}
}
