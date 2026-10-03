using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Debug;

[Tool]
[ScriptPath("res://src/Core/Debug/NBgLayerDebug.cs")]
public class NBgLayerDebug : Control
{
	public enum LayerVisibility
	{
		A,
		B,
		C
	}

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the 'ReloadLayers' method.
		/// </summary>
		public static readonly StringName ReloadLayers = "ReloadLayers";

		/// <summary>
		/// Cached name for the 'UpdateLayers' method.
		/// </summary>
		public static readonly StringName UpdateLayers = "UpdateLayers";

		/// <summary>
		/// Cached name for the 'AddLayer' method.
		/// </summary>
		public static readonly StringName AddLayer = "AddLayer";

		/// <summary>
		/// Cached name for the 'ToLayerName' method.
		/// </summary>
		public static readonly StringName ToLayerName = "ToLayerName";

		/// <summary>
		/// Cached name for the 'ClearLayers' method.
		/// </summary>
		public static readonly StringName ClearLayers = "ClearLayers";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'VisibleLayer' property.
		/// </summary>
		public static readonly StringName VisibleLayer = "VisibleLayer";

		/// <summary>
		/// Cached name for the 'ReloadLayersCallable' property.
		/// </summary>
		public static readonly StringName ReloadLayersCallable = "ReloadLayersCallable";

		/// <summary>
		/// Cached name for the '_layerA' field.
		/// </summary>
		public static readonly StringName _layerA = "_layerA";

		/// <summary>
		/// Cached name for the '_layerB' field.
		/// </summary>
		public static readonly StringName _layerB = "_layerB";

		/// <summary>
		/// Cached name for the '_layerC' field.
		/// </summary>
		public static readonly StringName _layerC = "_layerC";

		/// <summary>
		/// Cached name for the '_visibleLayer' field.
		/// </summary>
		public static readonly StringName _visibleLayer = "_visibleLayer";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	[CompilerGenerated]
	private sealed class _003CGetLayerNodes_003Ed__16 : IEnumerable<Control>, IEnumerable, IEnumerator<Control>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Control _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public NBgLayerDebug _003C_003E4__this;

		private IEnumerator<Node> _003C_003E7__wrap1;

		Control IEnumerator<Control>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetLayerNodes_003Ed__16(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = System.Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				NBgLayerDebug nBgLayerDebug = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = nBgLayerDebug.GetChildren().GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					Node current = _003C_003E7__wrap1.Current;
					if (current.Name.ToString().StartsWith("Layer_"))
					{
						_003C_003E2__current = (Control)current;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Control> IEnumerable<Control>.GetEnumerator()
		{
			_003CGetLayerNodes_003Ed__16 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == System.Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CGetLayerNodes_003Ed__16(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Control>)this).GetEnumerator();
		}
	}

	private const string _layerNodePrefix = "Layer_";

	private PackedScene? _layerA;

	private PackedScene? _layerB;

	private PackedScene? _layerC;

	private LayerVisibility _visibleLayer;

	[Export(PropertyHint.None, "")]
	public LayerVisibility VisibleLayer
	{
		get
		{
			return _visibleLayer;
		}
		set
		{
			_visibleLayer = value;
			if (Engine.IsEditorHint())
			{
				UpdateLayers();
			}
		}
	}

	[ExportToolButton("Reload Layers")]
	private Callable ReloadLayersCallable => Callable.From(ReloadLayers);

	public override void _EnterTree()
	{
		if (Engine.IsEditorHint())
		{
			ReloadLayers();
		}
	}

	private void ReloadLayers()
	{
		string sceneFilePath = GetTree().GetEditedSceneRoot().SceneFilePath;
		if (sceneFilePath == null)
		{
			return;
		}
		string text;
		if (base.Name.ToString() == "Foreground")
		{
			text = "fg";
		}
		else
		{
			string text2 = base.Name.ToString();
			int length = "Layer_".Length;
			if (!int.TryParse(text2.Substring(length, text2.Length - length), out var result))
			{
				return;
			}
			text = $"bg_{result:D2}";
		}
		string file = sceneFilePath.GetFile();
		string text3 = file.Substring(0, file.LastIndexOf('_'));
		string text4 = Path.Combine(sceneFilePath.GetBaseDir(), "layers", text3 + "_" + text);
		string path = text4 + "_a.tscn";
		string path2 = text4 + "_b.tscn";
		string path3 = text4 + "_c.tscn";
		if (ResourceLoader.Exists(path))
		{
			_layerA = ResourceLoader.Load<PackedScene>(path, null, ResourceLoader.CacheMode.Reuse);
		}
		if (ResourceLoader.Exists(path2))
		{
			_layerB = ResourceLoader.Load<PackedScene>(path2, null, ResourceLoader.CacheMode.Reuse);
		}
		if (ResourceLoader.Exists(path3))
		{
			_layerC = ResourceLoader.Load<PackedScene>(path3, null, ResourceLoader.CacheMode.Reuse);
		}
		UpdateLayers();
	}

	private void UpdateLayers()
	{
		ClearLayers();
		if (_visibleLayer == LayerVisibility.A && _layerA != null)
		{
			AddLayer(LayerVisibility.A, _layerA);
		}
		if (_visibleLayer == LayerVisibility.B && _layerB != null)
		{
			AddLayer(LayerVisibility.B, _layerB);
		}
		if (_visibleLayer == LayerVisibility.C && _layerC != null)
		{
			AddLayer(LayerVisibility.C, _layerC);
		}
	}

	private void AddLayer(LayerVisibility name, PackedScene layerScene)
	{
		Control control = layerScene.Instantiate<Control>(PackedScene.GenEditState.Disabled);
		control.Name = ToLayerName(name);
		this.AddChildSafely(control);
	}

	private static string ToLayerName(LayerVisibility layer)
	{
		return $"{"Layer_"}{layer}";
	}

	[IteratorStateMachine(typeof(_003CGetLayerNodes_003Ed__16))]
	private IEnumerable<Control> GetLayerNodes()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetLayerNodes_003Ed__16(-2)
		{
			_003C_003E4__this = this
		};
	}

	private void ClearLayers()
	{
		foreach (Control layerNode in GetLayerNodes())
		{
			this.RemoveChildSafely(layerNode);
			layerNode.QueueFreeSafely();
		}
	}

	public override void _ExitTree()
	{
		ClearLayers();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReloadLayers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateLayers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AddLayer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Object, "layerScene", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("PackedScene"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ToLayerName, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "layer", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearLayers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReloadLayers && args.Count == 0)
		{
			ReloadLayers();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateLayers && args.Count == 0)
		{
			UpdateLayers();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddLayer && args.Count == 2)
		{
			AddLayer(VariantUtils.ConvertTo<LayerVisibility>(in args[0]), VariantUtils.ConvertTo<PackedScene>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToLayerName && args.Count == 1)
		{
			string from = ToLayerName(VariantUtils.ConvertTo<LayerVisibility>(in args[0]));
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName.ClearLayers && args.Count == 0)
		{
			ClearLayers();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ToLayerName && args.Count == 1)
		{
			string from = ToLayerName(VariantUtils.ConvertTo<LayerVisibility>(in args[0]));
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName.ReloadLayers)
		{
			return true;
		}
		if (method == MethodName.UpdateLayers)
		{
			return true;
		}
		if (method == MethodName.AddLayer)
		{
			return true;
		}
		if (method == MethodName.ToLayerName)
		{
			return true;
		}
		if (method == MethodName.ClearLayers)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.VisibleLayer)
		{
			VisibleLayer = VariantUtils.ConvertTo<LayerVisibility>(in value);
			return true;
		}
		if (name == PropertyName._layerA)
		{
			_layerA = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._layerB)
		{
			_layerB = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._layerC)
		{
			_layerC = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._visibleLayer)
		{
			_visibleLayer = VariantUtils.ConvertTo<LayerVisibility>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.VisibleLayer)
		{
			LayerVisibility from = VisibleLayer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ReloadLayersCallable)
		{
			Callable from2 = ReloadLayersCallable;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName._layerA)
		{
			value = VariantUtils.CreateFrom(in _layerA);
			return true;
		}
		if (name == PropertyName._layerB)
		{
			value = VariantUtils.CreateFrom(in _layerB);
			return true;
		}
		if (name == PropertyName._layerC)
		{
			value = VariantUtils.CreateFrom(in _layerC);
			return true;
		}
		if (name == PropertyName._visibleLayer)
		{
			value = VariantUtils.CreateFrom(in _visibleLayer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._layerA, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._layerB, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._layerC, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._visibleLayer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.VisibleLayer, PropertyHint.Enum, "A,B,C", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Callable, PropertyName.ReloadLayersCallable, PropertyHint.ToolButton, "Reload Layers", PropertyUsageFlags.Editor, exported: true));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName visibleLayer = PropertyName.VisibleLayer;
		LayerVisibility from = VisibleLayer;
		info.AddProperty(visibleLayer, Variant.From(in from));
		info.AddProperty(PropertyName._layerA, Variant.From(in _layerA));
		info.AddProperty(PropertyName._layerB, Variant.From(in _layerB));
		info.AddProperty(PropertyName._layerC, Variant.From(in _layerC));
		info.AddProperty(PropertyName._visibleLayer, Variant.From(in _visibleLayer));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.VisibleLayer, out var value))
		{
			VisibleLayer = value.As<LayerVisibility>();
		}
		if (info.TryGetProperty(PropertyName._layerA, out var value2))
		{
			_layerA = value2.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._layerB, out var value3))
		{
			_layerB = value3.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._layerC, out var value4))
		{
			_layerC = value4.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._visibleLayer, out var value5))
		{
			_visibleLayer = value5.As<LayerVisibility>();
		}
	}
}
