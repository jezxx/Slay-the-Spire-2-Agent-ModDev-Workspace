using System;
using System.Collections.Generic;
using System.Reflection;

namespace MegaCrit.Sts2.Core.Modding;

public class AssemblyInfo
{
	/// <summary>
	/// Helper dictionary mapping assemblies to the mods they represent.
	/// </summary>
	public static Dictionary<Assembly, Mod>? ModMap { get; private set; }

	/// <summary>
	/// The base game assembly.
	/// </summary>
	public static Assembly? BaseGame { get; private set; }

	public static Dictionary<Type, (Mod?, bool)>? MockTypes { get; set; }

	public static void Init()
	{
		BaseGame = Assembly.GetExecutingAssembly();
		ModMap = new Dictionary<Assembly, Mod>();
		foreach (Mod mod in ModManager.Mods)
		{
			if (mod.state != ModLoadState.Loaded)
			{
				continue;
			}
			foreach (Assembly assembly in mod.assemblies)
			{
				ModMap[assembly] = mod;
			}
		}
	}

	public static Mod? ModForType(Type type, out bool isBaseGame)
	{
		if (MockTypes != null && MockTypes.TryGetValue(type, out (Mod, bool) value))
		{
			isBaseGame = value.Item2;
			return value.Item1;
		}
		if (ModMap == null)
		{
			throw new InvalidOperationException();
		}
		Assembly assembly = type.Assembly;
		ModMap.TryGetValue(assembly, out Mod value2);
		isBaseGame = BaseGame == assembly;
		return value2;
	}

	/// <summary>
	/// This should only be called by tests.
	/// Typically, AssemblyInfo is initialized once at the start of the game and never cleared.
	/// </summary>
	public static void ClearForTests()
	{
		BaseGame = null;
		ModMap = null;
	}
}
