using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Random;

/// <summary>
/// A custom random class which allows predictable results when utilizing seeds.
/// </summary>
public class Rng
{
	private int _counter;

	private readonly MegaRandom _random;

	/// <summary>
	/// A non-deterministic RNG instance. This will not produce the same results after saving and loading.
	/// Good for when we need to randomize things that don't impact gameplay.
	/// We use this instead of the GD.Rand functions because they're extremely slow when called from C#.
	/// </summary>
	public static Rng Chaotic { get; } = new Rng((ulong)DateTimeOffset.Now.ToUnixTimeSeconds());


	public Rng(ulong seed = 0uL)
	{
		_random = new MegaRandom(seed);
	}

	public Rng(SerializableRng serializable)
	{
		_counter = serializable.counter;
		_random = new MegaRandom(serializable);
	}

	/// <summary>
	/// An ergonomic constructor for creating an RNG for a specific piece of content during a run.
	/// This RNG will be unique among:
	///  - The run (using player.RunState.Rng)
	///  - The player SLOT
	///  - The ID of the content passed
	///
	/// This uses the player _slot index_, rather than the NetId, to ensure consistent results when playing a daily run.
	/// Because it only uses the three things above, if the player sees the same piece of content again this run, the
	/// RNG will return the same results. If that is a possibility, pass in a <paramref name="mixin" />.
	/// </summary>
	public Rng(Player player, ModelId id, ulong mixin = 0uL)
		: this((ulong)((long)player.RunState.Rng.Seed + (long)player.RunState.GetPlayerSlotIndex(player) + (long)StringHelper.GetDeterministicHashCode(id.Entry)) + mixin)
	{
	}

	public Rng(ulong seed, string name)
		: this(seed + StringHelper.GetDeterministicHashCode(name))
	{
	}

	/// <summary>
	/// Load RNG state from serializable.
	/// Use this when you need to keep the old instance's references intact.
	/// </summary>
	public void LoadFromSerializable(SerializableRng serializable)
	{
		_counter = serializable.counter;
		_random.Reinitialise(serializable);
	}

	/// <summary>
	/// Returns true or false. 50/50
	/// </summary>
	public bool NextBool()
	{
		_counter++;
		return _random.Next(2) == 0;
	}

	/// <summary>
	/// Get a random integer between 0 (inclusive) and maxExclusive.
	/// </summary>
	/// <param name="maxExclusive">1 higher than the highest allowed int.</param>
	/// <returns>Random integer.</returns>
	public int NextInt(int maxExclusive = int.MaxValue)
	{
		_counter++;
		return _random.Next(maxExclusive);
	}

	/// <summary>
	/// Get a random integer between minInclusive and maxExclusive.
	/// </summary>
	/// <param name="minInclusive">Lowest allowed number.</param>
	/// <param name="maxExclusive">1 higher than the maximum allowed number.</param>
	/// <returns>Random integer.</returns>
	public int NextInt(int minInclusive, int maxExclusive)
	{
		if (minInclusive >= maxExclusive)
		{
			throw new ArgumentOutOfRangeException("minInclusive", "Minimum must be lower than maximum.");
		}
		_counter++;
		return _random.Next(minInclusive, maxExclusive);
	}

	/// <summary>
	/// Get a random integer between 0 (inclusive) and maxExclusive.
	/// </summary>
	/// <param name="maxExclusive">1 higher than the highest allowed int.</param>
	/// <returns>Random integer.</returns>
	public uint NextUnsignedInt(uint maxExclusive = uint.MaxValue)
	{
		return NextUnsignedInt(0u, maxExclusive);
	}

	/// <summary>
	/// Get a random integer between minInclusive and maxInclusive.
	/// </summary>
	/// <param name="minInclusive">Lowest allowed number.</param>
	/// <param name="maxExclusive">1 higher than the maximum allowed number.</param>
	/// <returns>Random integer.</returns>
	public uint NextUnsignedInt(uint minInclusive, uint maxExclusive)
	{
		if (minInclusive >= maxExclusive)
		{
			throw new ArgumentOutOfRangeException("minInclusive", "Minimum must be lower than maximum.");
		}
		_counter++;
		double num = _random.NextDouble();
		double num2 = maxExclusive - minInclusive;
		uint num3 = (uint)(num * num2);
		return minInclusive + num3;
	}

	public ulong NextUnsignedLong()
	{
		_counter++;
		return _random.NextULong();
	}

	public ulong NextUnsignedLong(ulong maxExclusive)
	{
		if (maxExclusive == ulong.MaxValue)
		{
			return NextUnsignedLong();
		}
		return NextUnsignedLong(0uL, maxExclusive);
	}

	public ulong NextUnsignedLong(ulong minInclusive, ulong maxExclusive)
	{
		if (minInclusive >= maxExclusive)
		{
			throw new ArgumentOutOfRangeException("minInclusive", "Minimum must be lower than maximum.");
		}
		_counter++;
		double num = _random.NextDouble();
		double num2 = maxExclusive - minInclusive;
		ulong num3 = (ulong)(num * num2);
		return minInclusive + num3;
	}

	/// <summary>
	/// Get a random floating-point between 0 and max.
	/// </summary>
	/// <param name="max">Highest allowed number.</param>
	/// <returns>Random float.</returns>
	public float NextFloat(float max = 1f)
	{
		return NextFloat(0f, max);
	}

	/// <summary>
	/// Get a random floating-point number between min and max.
	/// </summary>
	/// <param name="min">Lowest allowed number.</param>
	/// <param name="max">Highest allowed number.</param>
	/// <returns>Random float.</returns>
	public float NextFloat(float min, float max)
	{
		if (min > max)
		{
			throw new ArgumentOutOfRangeException("min", "Minimum must not be higher than maximum.");
		}
		_counter++;
		return (float)(_random.NextDouble() * (double)(max - min) + (double)min);
	}

	/// <summary>
	/// Get a random double-precision (double) between 0.0 (inclusive) and 1.0 (exclusive).
	/// </summary>
	/// <returns>Random double.</returns>
	public double NextDouble()
	{
		_counter++;
		return _random.NextDouble();
	}

	/// <summary>
	/// Get a random double-precision floating-point number between 0 and 1.
	/// </summary>
	/// <returns>Random double.</returns>
	public double NextDouble(double min, double max)
	{
		if (min > max)
		{
			throw new ArgumentOutOfRangeException("min", "Minimum must not be higher than maximum.");
		}
		_counter++;
		return _random.NextDouble() * (max - min) + min;
	}

	public float NextGaussianFloat(float mean = 0f, float stdDev = 1f, float min = 0f, float max = 1f)
	{
		return (float)NextGaussianDouble(mean, stdDev, min, max);
	}

	/// <summary>
	/// Generate a random floating-point number with a Gaussian distribution between min and max with the specified mean
	/// and standard deviation.
	/// See https://en.wikipedia.org/wiki/Normal_distribution for more on Gaussian distributions.
	/// </summary>
	/// <param name="mean">Mean for the Gaussian distribution.</param>
	/// <param name="stdDev">Standard deviation for the Gaussian distribution.</param>
	/// <param name="min">Lowest allowed number.</param>
	/// <param name="max">Highest allowed number.</param>
	/// <returns>Random double with a Gaussian distribution.</returns>
	public double NextGaussianDouble(double mean = 0.0, double stdDev = 1.0, double min = 0.0, double max = 1.0)
	{
		if (min > max)
		{
			throw new ArgumentOutOfRangeException("min", "Minimum must not be higher than maximum.");
		}
		if ((mean < 0.0 || mean > 1.0) ? true : false)
		{
			throw new ArgumentOutOfRangeException("mean", mean, "Mean must be within [0, 1].");
		}
		double num4;
		do
		{
			double d = 1.0 - NextDouble();
			double num = 1.0 - NextDouble();
			double num2 = Math.Sqrt(-2.0 * Math.Log(d));
			double d2 = Math.PI * 2.0 * num;
			double num3 = num2 * Math.Cos(d2);
			num4 = mean + num3 * stdDev;
		}
		while ((num4 < 0.0 || num4 > 1.0) ? true : false);
		return num4 * (max - min) + min;
	}

	/// <summary>
	/// Returns a random number within a normal distribution.
	/// This is used as part of the algorithm for location assignment.
	/// <param name="mean">Mean for the Gaussian distribution.</param>
	/// <param name="stdDev">Standard deviation for the Gaussian distribution.</param>
	/// <param name="min">Lowest allowed number.</param>
	/// <param name="max">Highest allowed number.</param>
	/// <returns></returns>
	/// </summary>
	public int NextGaussianInt(int mean, int stdDev, int min, int max)
	{
		if (min > max)
		{
			throw new ArgumentOutOfRangeException("min", "Minimum must not be higher than maximum.");
		}
		if (mean < min || mean > max)
		{
			throw new ArgumentOutOfRangeException("mean", mean, "Mean must be within [min, max].");
		}
		int num3;
		do
		{
			double d = 1.0 - NextDouble();
			double num = 1.0 - NextDouble();
			double num2 = Math.Sqrt(-2.0 * Math.Log(d)) * Math.Sin(Math.PI * 2.0 * num);
			double a = (double)mean + (double)stdDev * num2;
			num3 = (int)Math.Round(a);
		}
		while (num3 < min || num3 > max);
		return num3;
	}

	/// <summary>
	/// Get a random item from the specified set of items.
	/// </summary>
	/// <param name="items">Set of items to pull from.</param>
	/// <typeparam name="T">Type of items contained in the set.</typeparam>
	/// <returns>Random item.</returns>
	public T? NextItem<T>(IEnumerable<T> items)
	{
		IEnumerable<T> source = (items as T[]) ?? items.ToArray();
		int num = source.Count();
		if (num == 0)
		{
			return default(T);
		}
		int index = NextInt(0, num);
		return source.ElementAt(index);
	}

	/// <summary>
	/// Get a random item from the specified set of items, respecting a specified weighting function.
	/// </summary>
	/// <param name="items">Set of items to pull from.</param>
	/// <param name="weightFetcher">Function to determine which items should be returned more/less often.</param>
	/// <typeparam name="T">Type of items contained in the set.</typeparam>
	/// <returns>Random item.</returns>
	public T? WeightedNextItem<T>(IEnumerable<T> items, Func<T?, float> weightFetcher)
	{
		return WeightedNextItem<T>(NextFloat(), items, weightFetcher, default(T));
	}

	/// <summary>
	/// Get a random item from the specified set of items, respecting a specified weighting function.
	/// </summary>
	/// <param name="randInput">Floating-point number between 0 and 1 to use for randomness.</param>
	/// <param name="items">Set of items to pull from.</param>
	/// <param name="weightFetcher">Function to determine which items should be returned more/less often.</param>
	/// <param name="fallback">Element to return if the weighting function fails to grab an item.</param>
	/// <typeparam name="T">Type of items contained in the set.</typeparam>
	/// <returns>Random item.</returns>
	public static T WeightedNextItem<T>(float randInput, IEnumerable<T> items, Func<T, float> weightFetcher, T fallback)
	{
		float num = items.Sum(weightFetcher);
		float num2 = randInput * num;
		foreach (T item in items)
		{
			num2 -= weightFetcher(item);
			if (num2 <= 0f)
			{
				return item;
			}
		}
		return fallback;
	}

	/// <summary>
	/// Shuffles a list in place using the Fisher-Yates algorithm.
	/// </summary>
	/// <param name="list">The list to shuffle.</param>
	/// <typeparam name="T">Type of items in the list.</typeparam>
	public void Shuffle<T>(IList<T> list)
	{
		for (int num = list.Count - 1; num > 0; num--)
		{
			int num2 = NextInt(num + 1);
			int index = num;
			int index2 = num2;
			T value = list[num2];
			T value2 = list[num];
			list[index] = value;
			list[index2] = value2;
		}
	}

	public SerializableRng ToSerializable()
	{
		SerializableRng serializableRng = new SerializableRng
		{
			counter = _counter
		};
		_random.FillSerializableState(serializableRng);
		return serializableRng;
	}
}
