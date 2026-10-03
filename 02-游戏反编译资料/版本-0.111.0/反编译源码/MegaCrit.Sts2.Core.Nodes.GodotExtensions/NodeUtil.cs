using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Nodes.GodotExtensions;

public static class NodeUtil
{
	[CompilerGenerated]
	private sealed class _003CGetChildrenRecursive_003Ed__9<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T : notnull
	{
		private int _003C_003E1__state;

		private T _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node node;

		public Node _003C_003E3__node;

		private IEnumerator<Node> _003C_003E7__wrap1;

		private Node _003Cchild_003E5__3;

		private IEnumerator<T> _003C_003E7__wrap3;

		T IEnumerator<T>.Current
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
		public _003CGetChildrenRecursive_003Ed__9(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = System.Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
			{
				try
				{
					if (num == -4 || num == 1)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003Cchild_003E5__3 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = node.GetChildren().GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00fe;
				case 1:
					_003C_003E1__state = -4;
					goto IL_00a5;
				case 2:
					{
						_003C_003E1__state = -3;
						goto IL_00f7;
					}
					IL_00fe:
					if (_003C_003E7__wrap1.MoveNext())
					{
						_003Cchild_003E5__3 = _003C_003E7__wrap1.Current;
						_003C_003E7__wrap3 = _003Cchild_003E5__3.GetChildrenRecursive<T>().GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00a5;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					return false;
					IL_00f7:
					_003Cchild_003E5__3 = null;
					goto IL_00fe;
					IL_00a5:
					if (_003C_003E7__wrap3.MoveNext())
					{
						T current = _003C_003E7__wrap3.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap3 = null;
					if (_003Cchild_003E5__3 is T)
					{
						Node obj = _003Cchild_003E5__3;
						T val = (T)(object)((obj is T) ? obj : null);
						_003C_003E2__current = val;
						_003C_003E1__state = 2;
						return true;
					}
					goto IL_00f7;
				}
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

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			_003CGetChildrenRecursive_003Ed__9<T> _003CGetChildrenRecursive_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == System.Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetChildrenRecursive_003Ed__ = this;
			}
			else
			{
				_003CGetChildrenRecursive_003Ed__ = new _003CGetChildrenRecursive_003Ed__9<T>(0);
			}
			_003CGetChildrenRecursive_003Ed__.node = _003C_003E3__node;
			return _003CGetChildrenRecursive_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}

	/// <summary>
	/// Awaits the next process frame and returns the delta time.
	/// Throws <see cref="T:System.OperationCanceledException" /> if the token is cancelled or the node has exited the tree,
	/// which <see cref="M:MegaCrit.Sts2.Core.Helpers.TaskHelper.RunSafely(System.Threading.Tasks.Task)" /> silently swallows.
	/// </summary>
	public static async Task<float> AwaitProcessFrame(this Node node, CancellationToken ct = default(CancellationToken))
	{
		ct.ThrowIfCancellationRequested();
		SceneTree treeOrNull = node.GetTreeOrNull();
		if (treeOrNull == null)
		{
			throw new TaskCanceledException();
		}
		await treeOrNull.ToSignal(treeOrNull, SceneTree.SignalName.ProcessFrame);
		ct.ThrowIfCancellationRequested();
		if (!node.IsValid() || !node.IsInsideTree())
		{
			throw new TaskCanceledException();
		}
		return (float)node.GetProcessDeltaTime();
	}

	/// <summary>
	/// Awaits the next process frame.
	/// Unlike <see cref="M:MegaCrit.Sts2.Core.Nodes.GodotExtensions.NodeUtil.AwaitProcessFrame(Godot.Node,System.Threading.CancellationToken)" />, this merely returns when the cancellation token is cancelled instead of
	/// throwing. Use this in scenarios where cancellation is frequent and expected, as throwing OperationCancelledException
	/// can be very slow when running in debug mode.
	/// This method itself cancels the cancellation token if the node is removed from the scene tree or becomes invalid.
	/// </summary>
	public static async Task AwaitProcessFrameNonThrowing(this Node node, CancellationTokenSource cts)
	{
		if (cts.IsCancellationRequested)
		{
			return;
		}
		SceneTree treeOrNull = node.GetTreeOrNull();
		if (treeOrNull == null)
		{
			await cts.CancelAsync();
			return;
		}
		await treeOrNull.ToSignal(treeOrNull, SceneTree.SignalName.ProcessFrame);
		if (!cts.IsCancellationRequested && (!node.IsValid() || !node.IsInsideTree()))
		{
			await cts.CancelAsync();
		}
	}

	/// <summary>
	/// Returns the node's <see cref="T:Godot.SceneTree" />, or <c>null</c> if the node is not inside a tree.
	/// Unlike <see cref="M:Godot.Node.GetTree" />, this does not trigger a native error print when the tree is null.
	/// Always prefer this over <see cref="M:Godot.Node.GetTree" /> when the node may not be in the tree.
	/// </summary>
	public static SceneTree? GetTreeOrNull(this Node node)
	{
		if (!node.IsInsideTree())
		{
			return null;
		}
		return node.GetTree();
	}

	/// <summary>
	/// Returns true if candidate is located within parent's subtree.
	/// </summary>
	public static bool IsDescendant(Node? parent, Node candidate)
	{
		if (parent == null)
		{
			return false;
		}
		for (Node parent2 = candidate.GetParent(); parent2 != null; parent2 = parent2.GetParent())
		{
			if (parent2 == parent)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Returns true if the node is not null, is valid, and is not deleting.
	/// </summary>
	public static bool IsValid(this Node? node)
	{
		if (node != null && GodotObject.IsInstanceValid(node))
		{
			return !node.IsQueuedForDeletion();
		}
		return false;
	}

	/// <summary>
	/// Checks to see if a controller is detected before we grab the focus of control.
	/// </summary>
	public static void TryGrabFocus(this Control control)
	{
		Control control2 = control;
		if (!NControllerManager.Instance.IsUsingDirectionalNavigation)
		{
			return;
		}
		if (control2.IsVisibleInTree())
		{
			control2.GrabFocus();
			return;
		}
		Callable.From(delegate
		{
			if (control2.IsValid() && control2.IsInsideTree())
			{
				control2.GrabFocus();
			}
		}).CallDeferred();
	}

	/// <summary>
	/// Obtains the nearest ancestor of the given type.
	/// </summary>
	public static T? GetAncestorOfType<T>(this Node node)
	{
		for (Node parent = node.GetParent(); parent != null; parent = parent.GetParent())
		{
			if (parent is T)
			{
				return (T)(object)((parent is T) ? parent : null);
			}
		}
		return default(T);
	}

	/// <summary>
	/// Awaits an arbitrary signal on a <see cref="T:Godot.GodotObject" />, automatically cancelling when the owning node
	/// exits the tree.
	/// </summary>
	/// <remarks>
	/// <c>ToSignal</c> connects via <c>CONNECT_ONE_SHOT</c>, but one-shot cleanup only runs during signal
	/// emission. If the source object is freed without emitting the signal, the <c>SignalAwaiterCallable</c>
	/// and its GC handle linger until the source is destroyed by the garbage collector. This method avoids that
	/// by explicitly disconnecting on both the signal-fired and tree-exiting paths.
	/// </remarks>
	public static Task AwaitSignal(this GodotObject source, StringName signal, Node owner)
	{
		GodotObject source2 = source;
		StringName signal2 = signal;
		Node owner2 = owner;
		if (!GodotObject.IsInstanceValid(source2))
		{
			return Task.CompletedTask;
		}
		TaskCompletionSource tcs = new TaskCompletionSource();
		bool resolved = false;
		Callable callable = default(Callable);
		callable = Callable.From(OnSignal);
		source2.Connect(signal2, callable);
		owner2.TreeExiting += OnExiting;
		return tcs.Task;
		void OnExiting()
		{
			if (!resolved)
			{
				resolved = true;
				if (GodotObject.IsInstanceValid(source2))
				{
					source2.Disconnect(signal2, callable);
				}
				tcs.TrySetCanceled();
			}
		}
		void OnSignal()
		{
			if (!resolved)
			{
				resolved = true;
				if (GodotObject.IsInstanceValid(source2))
				{
					source2.Disconnect(signal2, callable);
				}
				if (GodotObject.IsInstanceValid(owner2))
				{
					owner2.TreeExiting -= OnExiting;
				}
				tcs.TrySetResult();
			}
		}
	}

	/// <summary>
	/// Awaits an arbitrary signal on a <see cref="T:Godot.GodotObject" />, automatically cancelling when the owning node
	/// exits the tree.
	/// </summary>
	/// <remarks>
	/// <c>ToSignal</c> connects via <c>CONNECT_ONE_SHOT</c>, but one-shot cleanup only runs during signal
	/// emission. If the source object is freed without emitting the signal, the <c>SignalAwaiterCallable</c>
	/// and its GC handle linger until the source is destroyed by the garbage collector. This method avoids that
	/// by explicitly disconnecting on both the signal-fired and tree-exiting paths.
	/// </remarks>
	public static Task<T?> AwaitSignal<[MustBeVariant] T>(this GodotObject source, StringName signal, Node owner) where T : class
	{
		GodotObject source2 = source;
		StringName signal2 = signal;
		Node owner2 = owner;
		if (!GodotObject.IsInstanceValid(source2))
		{
			return Task.FromResult<T>(null);
		}
		TaskCompletionSource<T?> tcs = new TaskCompletionSource<T>();
		bool resolved = false;
		Callable callable = default(Callable);
		callable = Callable.From<T>(OnSignal);
		source2.Connect(signal2, callable);
		owner2.TreeExiting += OnExiting;
		return tcs.Task;
		void OnExiting()
		{
			if (!resolved)
			{
				resolved = true;
				if (GodotObject.IsInstanceValid(source2))
				{
					source2.Disconnect(signal2, callable);
				}
				tcs.TrySetCanceled();
			}
		}
		void OnSignal(T obj)
		{
			if (!resolved)
			{
				resolved = true;
				if (GodotObject.IsInstanceValid(source2))
				{
					source2.Disconnect(signal2, callable);
				}
				if (GodotObject.IsInstanceValid(owner2))
				{
					owner2.TreeExiting -= OnExiting;
				}
				tcs.TrySetResult(obj);
			}
		}
	}

	/// <summary>
	/// Obtains all children of a certain type, searching recursively.
	/// </summary>
	[IteratorStateMachine(typeof(_003CGetChildrenRecursive_003Ed__9<>))]
	public static IEnumerable<T> GetChildrenRecursive<T>(this Node node)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetChildrenRecursive_003Ed__9<T>(-2)
		{
			_003C_003E3__node = node
		};
	}
}
