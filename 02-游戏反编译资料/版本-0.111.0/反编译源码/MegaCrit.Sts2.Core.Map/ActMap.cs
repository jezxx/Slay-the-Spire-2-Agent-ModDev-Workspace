using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MegaCrit.Sts2.Core.Map;

/// <summary>
/// Handles the creation of map points for a map.
/// </summary>
public abstract class ActMap
{
	[CompilerGenerated]
	private sealed class _003CGetAllMapPoints_003Ed__11 : IEnumerable<MapPoint>, IEnumerable, IEnumerator<MapPoint>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private MapPoint _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public ActMap _003C_003E4__this;

		private int _003Cc_003E5__2;

		private int _003Cr_003E5__3;

		MapPoint IEnumerator<MapPoint>.Current
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
		public _003CGetAllMapPoints_003Ed__11(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			ActMap actMap = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_0062;
			}
			_003C_003E1__state = -1;
			_003Cc_003E5__2 = 0;
			goto IL_0096;
			IL_0072:
			if (_003Cr_003E5__3 < actMap.Grid.GetLength(1))
			{
				MapPoint mapPoint = actMap.Grid[_003Cc_003E5__2, _003Cr_003E5__3];
				if (mapPoint != null)
				{
					_003C_003E2__current = mapPoint;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0062;
			}
			_003Cc_003E5__2++;
			goto IL_0096;
			IL_0062:
			_003Cr_003E5__3++;
			goto IL_0072;
			IL_0096:
			if (_003Cc_003E5__2 < actMap.GetColumnCount())
			{
				_003Cr_003E5__3 = 0;
				goto IL_0072;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<MapPoint> IEnumerable<MapPoint>.GetEnumerator()
		{
			_003CGetAllMapPoints_003Ed__11 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CGetAllMapPoints_003Ed__11(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MapPoint>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetPointsInRow_003Ed__12 : IEnumerable<MapPoint>, IEnumerable, IEnumerator<MapPoint>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private MapPoint _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private int row;

		public int _003C_003E3__row;

		public ActMap _003C_003E4__this;

		private int _003Cc_003E5__2;

		MapPoint IEnumerator<MapPoint>.Current
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
		public _003CGetPointsInRow_003Ed__12(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			ActMap actMap = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_0076;
			}
			_003C_003E1__state = -1;
			if (row >= 0 && row < actMap.Grid.GetLength(1))
			{
				_003Cc_003E5__2 = 0;
				goto IL_0086;
			}
			goto IL_0094;
			IL_0086:
			if (_003Cc_003E5__2 < actMap.GetColumnCount())
			{
				MapPoint mapPoint = actMap.Grid[_003Cc_003E5__2, row];
				if (mapPoint != null)
				{
					_003C_003E2__current = mapPoint;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0076;
			}
			goto IL_0094;
			IL_0094:
			return false;
			IL_0076:
			_003Cc_003E5__2++;
			goto IL_0086;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<MapPoint> IEnumerable<MapPoint>.GetEnumerator()
		{
			_003CGetPointsInRow_003Ed__12 _003CGetPointsInRow_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetPointsInRow_003Ed__ = this;
			}
			else
			{
				_003CGetPointsInRow_003Ed__ = new _003CGetPointsInRow_003Ed__12(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CGetPointsInRow_003Ed__.row = _003C_003E3__row;
			return _003CGetPointsInRow_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MapPoint>)this).GetEnumerator();
		}
	}

	public readonly HashSet<MapPoint> startMapPoints = new HashSet<MapPoint>();

	public abstract MapPoint BossMapPoint { get; }

	public abstract MapPoint StartingMapPoint { get; }

	/// <summary>
	/// The second boss map point for Double Boss mode (Ascension 10+).
	/// Null if not in double boss mode.
	/// </summary>
	public virtual MapPoint? SecondBossMapPoint => null;

	protected abstract MapPoint?[,] Grid { get; }

	public int GetColumnCount()
	{
		return Grid.GetLength(0);
	}

	public int GetRowCount()
	{
		return Grid.GetLength(1);
	}

	[IteratorStateMachine(typeof(_003CGetAllMapPoints_003Ed__11))]
	public IEnumerable<MapPoint> GetAllMapPoints()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAllMapPoints_003Ed__11(-2)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CGetPointsInRow_003Ed__12))]
	public IEnumerable<MapPoint> GetPointsInRow(int row)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetPointsInRow_003Ed__12(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__row = row
		};
	}

	public virtual MapPoint? GetPoint(MapCoord coord)
	{
		return GetPoint(coord.col, coord.row);
	}

	public MapPoint? GetPoint(int col, int row)
	{
		if (col == BossMapPoint.coord.col && row == BossMapPoint.coord.row)
		{
			return BossMapPoint;
		}
		if (SecondBossMapPoint != null && col == SecondBossMapPoint.coord.col && row == SecondBossMapPoint.coord.row)
		{
			return SecondBossMapPoint;
		}
		if (col == StartingMapPoint.coord.col && row == StartingMapPoint.coord.row)
		{
			return StartingMapPoint;
		}
		if (col >= 0 && col < Grid.GetLength(0) && row >= 0 && row < Grid.GetLength(1))
		{
			return Grid[col, row];
		}
		return null;
	}

	public bool IsInMap(MapPoint mapPoint)
	{
		if (mapPoint.PointType == MapPointType.Ancient || mapPoint.PointType == MapPointType.Boss)
		{
			return true;
		}
		int col = mapPoint.coord.col;
		int row = mapPoint.coord.row;
		if (col < 0 || col >= Grid.GetLength(0) || row < 0 || row >= Grid.GetLength(1))
		{
			return false;
		}
		return Grid[col, row] != null;
	}

	public bool HasPoint(MapCoord coord)
	{
		if (coord.col == BossMapPoint.coord.col && coord.row == BossMapPoint.coord.row)
		{
			return true;
		}
		if (SecondBossMapPoint != null && coord.col == SecondBossMapPoint.coord.col && coord.row == SecondBossMapPoint.coord.row)
		{
			return true;
		}
		if (coord.col == StartingMapPoint.coord.col && coord.row == StartingMapPoint.coord.row)
		{
			return true;
		}
		if (coord.col < 0 || coord.col >= Grid.GetLength(0))
		{
			return false;
		}
		if (coord.row < 0 || coord.row >= Grid.GetLength(1))
		{
			return false;
		}
		return Grid[coord.col, coord.row] != null;
	}
}
