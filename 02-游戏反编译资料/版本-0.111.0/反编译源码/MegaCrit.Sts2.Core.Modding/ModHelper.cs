using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Modding;

/// <summary>
/// Some helper methods for modding.
/// </summary>
public static class ModHelper
{
	private class ModPoolContent
	{
		public bool isFrozen;

		public List<Type>? modelsToAdd;
	}

	private class ModRunHookSubscriber
	{
		public required string id;

		public required RunHookSubscriptionDelegate del;
	}

	private class ModCombatHookSubscriber
	{
		public required string id;

		public required CombatHookSubscriptionDelegate del;
	}

	[CompilerGenerated]
	private sealed class _003CIterateAllCombatStateSubscribers_003Ed__12 : IEnumerable<AbstractModel>, IEnumerable, IEnumerator<AbstractModel>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private AbstractModel _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private CombatState combatState;

		public CombatState _003C_003E3__combatState;

		private List<ModCombatHookSubscriber>.Enumerator _003C_003E7__wrap1;

		private IEnumerator<AbstractModel> _003C_003E7__wrap2;

		AbstractModel IEnumerator<AbstractModel>.Current
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
		public _003CIterateAllCombatStateSubscribers_003Ed__12(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || num == 1)
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
			_003C_003E7__wrap1 = default(List<ModCombatHookSubscriber>.Enumerator);
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -4;
					goto IL_009c;
				}
				_003C_003E1__state = -1;
				_003C_003E7__wrap1 = _combatHookSubscribers.GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_00b6;
				IL_009c:
				while (_003C_003E7__wrap2.MoveNext())
				{
					AbstractModel current = _003C_003E7__wrap2.Current;
					if (current != null)
					{
						_003C_003E2__current = current;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally2();
				_003C_003E7__wrap2 = null;
				goto IL_00b6;
				IL_00b6:
				IEnumerable<AbstractModel> enumerable;
				do
				{
					if (_003C_003E7__wrap1.MoveNext())
					{
						ModCombatHookSubscriber current2 = _003C_003E7__wrap1.Current;
						enumerable = current2.del(combatState);
						continue;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = default(List<ModCombatHookSubscriber>.Enumerator);
					return false;
				}
				while (enumerable == null);
				_003C_003E7__wrap2 = enumerable.GetEnumerator();
				_003C_003E1__state = -4;
				goto IL_009c;
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
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<AbstractModel> IEnumerable<AbstractModel>.GetEnumerator()
		{
			_003CIterateAllCombatStateSubscribers_003Ed__12 _003CIterateAllCombatStateSubscribers_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CIterateAllCombatStateSubscribers_003Ed__ = this;
			}
			else
			{
				_003CIterateAllCombatStateSubscribers_003Ed__ = new _003CIterateAllCombatStateSubscribers_003Ed__12(0);
			}
			_003CIterateAllCombatStateSubscribers_003Ed__.combatState = _003C_003E3__combatState;
			return _003CIterateAllCombatStateSubscribers_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<AbstractModel>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CIterateAllRunStateSubscribers_003Ed__11 : IEnumerable<AbstractModel>, IEnumerable, IEnumerator<AbstractModel>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private AbstractModel _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private RunState runState;

		public RunState _003C_003E3__runState;

		private List<ModRunHookSubscriber>.Enumerator _003C_003E7__wrap1;

		private IEnumerator<AbstractModel> _003C_003E7__wrap2;

		AbstractModel IEnumerator<AbstractModel>.Current
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
		public _003CIterateAllRunStateSubscribers_003Ed__11(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || num == 1)
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
			_003C_003E7__wrap1 = default(List<ModRunHookSubscriber>.Enumerator);
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -4;
					goto IL_009c;
				}
				_003C_003E1__state = -1;
				_003C_003E7__wrap1 = _runHookSubscribers.GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_00b6;
				IL_009c:
				while (_003C_003E7__wrap2.MoveNext())
				{
					AbstractModel current = _003C_003E7__wrap2.Current;
					if (current != null)
					{
						_003C_003E2__current = current;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally2();
				_003C_003E7__wrap2 = null;
				goto IL_00b6;
				IL_00b6:
				IEnumerable<AbstractModel> enumerable;
				do
				{
					if (_003C_003E7__wrap1.MoveNext())
					{
						ModRunHookSubscriber current2 = _003C_003E7__wrap1.Current;
						enumerable = current2.del(runState);
						continue;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = default(List<ModRunHookSubscriber>.Enumerator);
					return false;
				}
				while (enumerable == null);
				_003C_003E7__wrap2 = enumerable.GetEnumerator();
				_003C_003E1__state = -4;
				goto IL_009c;
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
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<AbstractModel> IEnumerable<AbstractModel>.GetEnumerator()
		{
			_003CIterateAllRunStateSubscribers_003Ed__11 _003CIterateAllRunStateSubscribers_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CIterateAllRunStateSubscribers_003Ed__ = this;
			}
			else
			{
				_003CIterateAllRunStateSubscribers_003Ed__ = new _003CIterateAllRunStateSubscribers_003Ed__11(0);
			}
			_003CIterateAllRunStateSubscribers_003Ed__.runState = _003C_003E3__runState;
			return _003CIterateAllRunStateSubscribers_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<AbstractModel>)this).GetEnumerator();
		}
	}

	private static readonly Dictionary<Type, ModPoolContent> _moddedContentForPools = new Dictionary<Type, ModPoolContent>();

	private static readonly List<ModRunHookSubscriber> _runHookSubscribers = new List<ModRunHookSubscriber>();

	private static readonly List<ModCombatHookSubscriber> _combatHookSubscribers = new List<ModCombatHookSubscriber>();

	/// <summary>
	/// Called by mods to add their models to a pool.
	/// Throws if the pool has already concatenated models to that pool (i.e. the mod was too late in calling this method).
	/// </summary>
	/// <typeparam name="TPoolType">The pool type to add to.</typeparam>
	/// <typeparam name="TModelType">The model to add to the pool.</typeparam>
	public static void AddModelToPool<TPoolType, TModelType>() where TPoolType : AbstractModel, IPoolModel where TModelType : AbstractModel
	{
		AddModelToPool(typeof(TPoolType), typeof(TModelType));
	}

	public static void AddModelToPool(Type poolType, Type modelType)
	{
		if (!_moddedContentForPools.TryGetValue(poolType, out ModPoolContent value))
		{
			value = new ModPoolContent
			{
				modelsToAdd = new List<Type>()
			};
			_moddedContentForPools.Add(poolType, value);
		}
		if (value.isFrozen)
		{
			throw new InvalidOperationException($"Tried to add model {modelType} to pool {poolType}, but it's too late! You must add content before the game is initialized.");
		}
		value.modelsToAdd.Add(modelType);
	}

	/// <summary>
	/// Called by a pool when it is ready to add modded content to its pool of content.
	/// </summary>
	/// <param name="poolModel">The pool that is consuming the content.</param>
	/// <param name="pool">The current pool of content.</param>
	/// <typeparam name="TModelType">The model type of the content that should have been added by mods.</typeparam>
	/// <returns>A new pool with all modded content concatenated to pool.</returns>
	public static IEnumerable<TModelType> ConcatModelsFromMods<TModelType>(IPoolModel poolModel, IEnumerable<TModelType> pool) where TModelType : AbstractModel
	{
		Type type = poolModel.GetType();
		if (!_moddedContentForPools.TryGetValue(type, out ModPoolContent value))
		{
			value = new ModPoolContent();
			_moddedContentForPools.Add(type, value);
		}
		value.isFrozen = true;
		if (value.modelsToAdd == null)
		{
			return pool;
		}
		IEnumerable<TModelType> second = value.modelsToAdd.Select((Type t) => ModelDb.GetById<TModelType>(ModelDb.GetId(t)));
		return pool.Concat(second);
	}

	/// <summary>
	/// Called by mods when they wish to provide custom model types to a RunState when IterateHookListeners is called.
	/// </summary>
	/// <param name="id">An ID to identify the subscription. Usually the name of the mod.</param>
	/// <param name="del">The delegate provided by the mod, which returns the models to iterate over.</param>
	public static void SubscribeForRunStateHooks(string id, RunHookSubscriptionDelegate del)
	{
		string id2 = id;
		if (_runHookSubscribers.Any((ModRunHookSubscriber s) => s.id == id2))
		{
			Log.Error("Tried to subscribe for RunState hooks with id " + id2 + ", but it's already been used! Ignoring subscription");
			return;
		}
		_runHookSubscribers.Add(new ModRunHookSubscriber
		{
			id = id2,
			del = del
		});
		_runHookSubscribers.Sort((ModRunHookSubscriber x, ModRunHookSubscriber y) => string.CompareOrdinal(x.id, y.id));
	}

	/// <summary>
	/// Called by mods when they wish to provide custom model types to a CombatState when IterateHookListeners is called.
	/// </summary>
	/// <param name="id">An ID to identify the subscription. Usually the name of the mod.</param>
	/// <param name="del">The delegate provided by the mod, which returns the models to iterate over.</param>
	public static void SubscribeForCombatStateHooks(string id, CombatHookSubscriptionDelegate del)
	{
		string id2 = id;
		if (_combatHookSubscribers.Any((ModCombatHookSubscriber s) => s.id == id2))
		{
			Log.Error("Tried to subscribe for CombatState hooks with id " + id2 + ", but it's already been used! Ignoring subscription");
			return;
		}
		_combatHookSubscribers.Add(new ModCombatHookSubscriber
		{
			id = id2,
			del = del
		});
		_combatHookSubscribers.Sort((ModCombatHookSubscriber x, ModCombatHookSubscriber y) => string.CompareOrdinal(x.id, y.id));
	}

	/// <summary>
	/// Called by RunState when it is iterating hook listeners, so that custom mod AbstractModel types have hooks called
	/// on them.
	/// </summary>
	/// <param name="runState">The RunState which is iterating hook listeners.</param>
	/// <returns>All the model types from all subscribing mods who wish to receive hooks.</returns>
	[IteratorStateMachine(typeof(_003CIterateAllRunStateSubscribers_003Ed__11))]
	public static IEnumerable<AbstractModel> IterateAllRunStateSubscribers(RunState runState)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CIterateAllRunStateSubscribers_003Ed__11(-2)
		{
			_003C_003E3__runState = runState
		};
	}

	/// <summary>
	/// Called by CombatState when it is iterating hook listeners, so that custom mod AbstractModel types have hooks
	/// called on them.
	/// </summary>
	/// <param name="combatState">The CombatState which is iterating hook listeners.</param>
	/// <returns>All the model types from all subscribing mods who wish to receive hooks.</returns>
	[IteratorStateMachine(typeof(_003CIterateAllCombatStateSubscribers_003Ed__12))]
	public static IEnumerable<AbstractModel> IterateAllCombatStateSubscribers(CombatState combatState)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CIterateAllCombatStateSubscribers_003Ed__12(-2)
		{
			_003C_003E3__combatState = combatState
		};
	}
}
