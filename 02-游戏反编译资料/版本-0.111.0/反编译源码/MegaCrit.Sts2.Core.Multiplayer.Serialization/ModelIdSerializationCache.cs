using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

/// <summary>
/// Database of string to integer mappings, for use in network serialization.
/// </summary>
public static class ModelIdSerializationCache
{
	private static readonly Dictionary<Type, List<PropertyInfo>> _savedPropertyCache = new Dictionary<Type, List<PropertyInfo>>();

	/// For serializing over the network, to save space, we map the string names statically to integers...
	private static readonly Dictionary<string, int> _categoryNameToNetIdMap = new Dictionary<string, int> { [ModelId.none.Category] = 0 };

	/// ...and vice-versa.
	private static readonly List<string> _netIdToCategoryNameMap;

	private static readonly Dictionary<string, int> _entryNameToNetIdMap;

	private static readonly List<string> _netIdToEntryNameMap;

	private static readonly Dictionary<string, int> _epochNameToNetIdMap;

	private static readonly List<string> _netIdToEpochNameMap;

	private static readonly Dictionary<string, int> _propertyNameToNetIdMap;

	private static readonly List<string> _netIdToPropertyNameMap;

	private static bool _initialized;

	public static int CategoryIdBitSize { get; private set; }

	public static int EntryIdBitSize { get; private set; }

	public static int EpochIdBitSize { get; private set; }

	public static int PropertyIdBitSize { get; private set; }

	public static int MaxCategoryId => _netIdToCategoryNameMap.Count - 1;

	public static int MaxEntryId => _netIdToEntryNameMap.Count - 1;

	public static int MaxEpochId => _netIdToEpochNameMap.Count - 1;

	public static int MaxPropertyId => _netIdToPropertyNameMap.Count - 1;

	public static uint Hash { get; private set; }

	/// <summary>
	/// Initializes the serialization cache.
	/// Note that the alternative is to initialize in a static initializer, but if we do this we don't control the time
	/// at which it gets initialized, and it usually happens at a bad time during gameplay.
	/// </summary>
	public static void Init()
	{
		byte[] array = new byte[512];
		XxHash32 xxHash = new XxHash32();
		List<ContentSorter<ModelId>.Item> list = ContentSorter<ModelId>.Sort(ModelDb.All.Select((AbstractModel m) => m.GetType()), ModelDb.GetId);
		foreach (ContentSorter<ModelId>.Item item in list)
		{
			ModelId id = item.id;
			if (!_categoryNameToNetIdMap.ContainsKey(id.Category))
			{
				int count = _netIdToCategoryNameMap.Count;
				_categoryNameToNetIdMap[id.Category] = count;
				_netIdToCategoryNameMap.Add(id.Category);
			}
			if (!_entryNameToNetIdMap.ContainsKey(id.Entry))
			{
				int count2 = _netIdToEntryNameMap.Count;
				_entryNameToNetIdMap[id.Entry] = count2;
				_netIdToEntryNameMap.Add(id.Entry);
			}
			if ((item.mod?.manifest?.affectsGameplay).GetValueOrDefault(true))
			{
				int bytes = Encoding.UTF8.GetBytes(id.Category, 0, id.Category.Length, array, 0);
				xxHash.Append(array.AsSpan(0, bytes));
				bytes = Encoding.UTF8.GetBytes(id.Entry, 0, id.Entry.Length, array, 0);
				xxHash.Append(array.AsSpan(0, bytes));
			}
		}
		foreach (ContentSorter<ModelId>.Item item2 in list)
		{
			if ((item2.mod?.manifest?.affectsGameplay).GetValueOrDefault(true))
			{
				CachePropertiesForType(item2.type, xxHash, array);
			}
			else
			{
				CachePropertiesForType(item2.type, null, null);
			}
		}
		IEnumerable<ContentSorter<string>.Item> enumerable = ContentSorter<string>.Sort(EpochModel.AllEpochs, EpochModel.GetId);
		foreach (ContentSorter<string>.Item item3 in enumerable)
		{
			string id2 = item3.id;
			if (!_epochNameToNetIdMap.ContainsKey(id2))
			{
				int count3 = _netIdToEpochNameMap.Count;
				_epochNameToNetIdMap[id2] = count3;
				_netIdToEpochNameMap.Add(id2);
			}
			int bytes2 = Encoding.UTF8.GetBytes(id2, 0, id2.Length, array, 0);
			xxHash.Append(array.AsSpan(0, bytes2));
		}
		_initialized = true;
		CategoryIdBitSize = Mathf.CeilToInt(Math.Log2(_netIdToCategoryNameMap.Count));
		EntryIdBitSize = Mathf.CeilToInt(Math.Log2(_netIdToEntryNameMap.Count));
		PropertyIdBitSize = Mathf.CeilToInt(Math.Log2(_netIdToPropertyNameMap.Count));
		EpochIdBitSize = Mathf.CeilToInt(Math.Log2(_netIdToEpochNameMap.Count));
		Hash = xxHash.GetCurrentHashAsUInt32();
		Log.Info($"ModelIdSerializationCache initialized. Categories: {_netIdToCategoryNameMap.Count} Entries: {_netIdToEntryNameMap.Count} Epochs: {_netIdToEpochNameMap.Count} Properties: {_netIdToPropertyNameMap.Count} Hash: {Hash}");
		if (ModManager.State == ModManagerState.Initialized && ModManager.Mods.Any((Mod m) => m.state == ModLoadState.Loaded && !(m.manifest?.affectsGameplay ?? true)))
		{
			Log.Info("  There are mods included that do not affect gameplay. Hash may not include all IDs.");
		}
		_initialized = true;
	}

	public static void ResetForTest()
	{
		_savedPropertyCache.Clear();
		_categoryNameToNetIdMap.Clear();
		_netIdToCategoryNameMap.Clear();
		_entryNameToNetIdMap.Clear();
		_netIdToEntryNameMap.Clear();
		_categoryNameToNetIdMap[ModelId.none.Category] = 0;
		_netIdToCategoryNameMap.Add(ModelId.none.Category);
		_entryNameToNetIdMap[ModelId.none.Entry] = 0;
		_netIdToEntryNameMap.Add(ModelId.none.Entry);
		_epochNameToNetIdMap.Clear();
		_netIdToEpochNameMap.Clear();
		_propertyNameToNetIdMap.Clear();
		_netIdToPropertyNameMap.Clear();
		CategoryIdBitSize = 0;
		EntryIdBitSize = 0;
		EpochIdBitSize = 0;
		PropertyIdBitSize = 0;
		Hash = 0u;
		_initialized = false;
	}

	private static void CachePropertiesForType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] Type type, XxHash32? hasher, byte[]? byteBuffer)
	{
		List<PropertyInfo> list = (from p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where p.GetCustomAttribute<SavedPropertyAttribute>() != null
			select p).ToList();
		list.Sort(CompareProperties);
		if (list.Count > 0)
		{
			_savedPropertyCache[type] = list;
		}
		foreach (PropertyInfo item in list)
		{
			if (!_propertyNameToNetIdMap.ContainsKey(item.Name))
			{
				int count = _netIdToPropertyNameMap.Count;
				_propertyNameToNetIdMap[item.Name] = count;
				_netIdToPropertyNameMap.Add(item.Name);
				if (hasher != null && byteBuffer != null)
				{
					int bytes = Encoding.UTF8.GetBytes(item.Name, 0, item.Name.Length, byteBuffer, 0);
					hasher.Append(byteBuffer.AsSpan(0, bytes));
				}
			}
		}
	}

	private static int CompareProperties(PropertyInfo p1, PropertyInfo p2)
	{
		SavedPropertyAttribute customAttribute = p1.GetCustomAttribute<SavedPropertyAttribute>();
		SavedPropertyAttribute customAttribute2 = p2.GetCustomAttribute<SavedPropertyAttribute>();
		if (customAttribute.order != customAttribute2.order)
		{
			return customAttribute.order.CompareTo(customAttribute2.order);
		}
		return string.CompareOrdinal(p1.Name, p2.Name);
	}

	public static int GetNetIdForCategory(string category)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		if (!_categoryNameToNetIdMap.TryGetValue(category, out var value))
		{
			throw new ArgumentException("ModelId category " + category + " could not be mapped to any net ID!");
		}
		return value;
	}

	public static bool TryGetNetIdForCategory(string category, out int netId)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		return _categoryNameToNetIdMap.TryGetValue(category, out netId);
	}

	public static string GetCategoryForNetId(int netId)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		if (netId < 0 || netId >= _netIdToCategoryNameMap.Count)
		{
			throw new ArgumentOutOfRangeException($"ModelId category ID {netId} is out of range! We have {_netIdToCategoryNameMap.Count} categories");
		}
		return _netIdToCategoryNameMap[netId];
	}

	public static int GetNetIdForEntry(string entry)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		if (!_entryNameToNetIdMap.TryGetValue(entry, out var value))
		{
			throw new ArgumentException("ModelId entry " + entry + " could not be mapped to any net ID!");
		}
		return value;
	}

	public static bool TryGetNetIdForEntry(string entry, out int netId)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		return _entryNameToNetIdMap.TryGetValue(entry, out netId);
	}

	public static string GetEntryForNetId(int netId)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		if (netId < 0 || netId >= _netIdToEntryNameMap.Count)
		{
			throw new ArgumentOutOfRangeException($"ModelId entry ID {netId} is out of range! We have {_netIdToEntryNameMap.Count} entries");
		}
		return _netIdToEntryNameMap[netId];
	}

	public static int GetNetIdForEpochId(string epochId)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		if (!_epochNameToNetIdMap.TryGetValue(epochId, out var value))
		{
			throw new ArgumentException("Epoch ID " + epochId + " could not be mapped to any net ID!");
		}
		return value;
	}

	public static string GetEpochIdForNetId(int netId)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		if (netId < 0 || netId >= _netIdToEpochNameMap.Count)
		{
			throw new ArgumentOutOfRangeException($"Epoch ID {netId} is out of range! We have {_netIdToEpochNameMap.Count} entries");
		}
		return _netIdToEpochNameMap[netId];
	}

	public static int GetNetIdForPropertyName(string propertyName)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		if (!_propertyNameToNetIdMap.TryGetValue(propertyName, out var value))
		{
			throw new ArgumentException("SavedProperty name " + propertyName + " could not be mapped to any net ID!");
		}
		return value;
	}

	public static string GetPropertyNameForNetId(int netId)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		if (netId < 0 || netId >= _netIdToPropertyNameMap.Count)
		{
			throw new ArgumentOutOfRangeException($"SavedProperty net ID {netId} is out of range! We have {_netIdToPropertyNameMap.Count} property names");
		}
		return _netIdToPropertyNameMap[netId];
	}

	public static List<PropertyInfo>? GetJsonPropertiesForType(Type t)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		if (_savedPropertyCache.TryGetValue(t, out List<PropertyInfo> value))
		{
			return value;
		}
		return null;
	}

	public static void CacheSavedPropertiesForTypeDebug([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] Type type)
	{
		CachePropertiesForType(type, null, null);
	}

	public static string Dump()
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ModelIdSerializationCache used before it was initialized!");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("CATEGORIES");
		StringBuilder stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler;
		for (int i = 0; i < _netIdToCategoryNameMap.Count; i++)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
			handler.AppendFormatted(i.ToString().PadRight(3));
			handler.AppendLiteral(": ");
			handler.AppendFormatted(_netIdToCategoryNameMap[i]);
			stringBuilder3.AppendLine(ref handler);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("ENTRIES");
		for (int j = 0; j < _netIdToEntryNameMap.Count; j++)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
			handler.AppendFormatted(j.ToString().PadRight(3));
			handler.AppendLiteral(": ");
			handler.AppendFormatted(_netIdToEntryNameMap[j]);
			stringBuilder4.AppendLine(ref handler);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("PROPERTIES");
		for (int k = 0; k < _netIdToPropertyNameMap.Count; k++)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
			handler.AppendFormatted(k.ToString().PadRight(3));
			handler.AppendLiteral(": ");
			handler.AppendFormatted(_netIdToPropertyNameMap[k]);
			stringBuilder5.AppendLine(ref handler);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("EPOCHS");
		for (int l = 0; l < _netIdToEpochNameMap.Count; l++)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
			handler.AppendFormatted(l.ToString().PadRight(3));
			handler.AppendLiteral(": ");
			handler.AppendFormatted(_netIdToEpochNameMap[l]);
			stringBuilder6.AppendLine(ref handler);
		}
		stringBuilder.AppendLine();
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder7 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
		handler.AppendLiteral("Hash: ");
		handler.AppendFormatted(Hash);
		stringBuilder7.AppendLine(ref handler);
		return stringBuilder.ToString();
	}

	static ModelIdSerializationCache()
	{
		int num = 1;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = ModelId.none.Category;
		_netIdToCategoryNameMap = list;
		_entryNameToNetIdMap = new Dictionary<string, int> { [ModelId.none.Entry] = 0 };
		index = 1;
		List<string> list2 = new List<string>(index);
		CollectionsMarshal.SetCount(list2, index);
		span = CollectionsMarshal.AsSpan(list2);
		num = 0;
		span[num] = ModelId.none.Entry;
		_netIdToEntryNameMap = list2;
		_epochNameToNetIdMap = new Dictionary<string, int>();
		_netIdToEpochNameMap = new List<string>();
		_propertyNameToNetIdMap = new Dictionary<string, int>();
		_netIdToPropertyNameMap = new List<string>();
	}
}
