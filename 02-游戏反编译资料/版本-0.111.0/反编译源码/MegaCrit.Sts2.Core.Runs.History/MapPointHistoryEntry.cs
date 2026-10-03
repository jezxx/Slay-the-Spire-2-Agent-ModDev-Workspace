using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs.History;

public class MapPointHistoryEntry : IPacketSerializable
{
	[CompilerGenerated]
	private sealed class _003CGetRoomsOfType_003Ed__16 : IEnumerable<MapPointRoomHistoryEntry>, IEnumerable, IEnumerator<MapPointRoomHistoryEntry>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private MapPointRoomHistoryEntry _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public MapPointHistoryEntry _003C_003E4__this;

		private RoomType roomType;

		public RoomType _003C_003E3__roomType;

		private List<MapPointRoomHistoryEntry>.Enumerator _003C_003E7__wrap1;

		MapPointRoomHistoryEntry IEnumerator<MapPointRoomHistoryEntry>.Current
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
		public _003CGetRoomsOfType_003Ed__16(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
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
			_003C_003E7__wrap1 = default(List<MapPointRoomHistoryEntry>.Enumerator);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				MapPointHistoryEntry mapPointHistoryEntry = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = mapPointHistoryEntry.Rooms.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					MapPointRoomHistoryEntry current = _003C_003E7__wrap1.Current;
					if (current.RoomType == roomType)
					{
						_003C_003E2__current = current;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = default(List<MapPointRoomHistoryEntry>.Enumerator);
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
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<MapPointRoomHistoryEntry> IEnumerable<MapPointRoomHistoryEntry>.GetEnumerator()
		{
			_003CGetRoomsOfType_003Ed__16 _003CGetRoomsOfType_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetRoomsOfType_003Ed__ = this;
			}
			else
			{
				_003CGetRoomsOfType_003Ed__ = new _003CGetRoomsOfType_003Ed__16(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CGetRoomsOfType_003Ed__.roomType = _003C_003E3__roomType;
			return _003CGetRoomsOfType_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MapPointRoomHistoryEntry>)this).GetEnumerator();
		}
	}

	[JsonPropertyName("map_point_type")]
	public MapPointType MapPointType { get; set; }

	/// <summary>
	/// All the rooms entered in this map point. Usually contains just 1, but can contain more when one type of
	/// room transitions into another (like when the Dense Vegetation event transitions into the Wrigglers combat).
	/// Accessing index 0 will usually give you what you want, but keep an eye out for cases where you may care
	/// about other rooms entered in the same map point.
	/// </summary>
	[JsonPropertyName("rooms")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<MapPointRoomHistoryEntry> Rooms { get; set; } = new List<MapPointRoomHistoryEntry>();


	[JsonPropertyName("player_stats")]
	public List<PlayerMapPointHistoryEntry> PlayerStats { get; set; } = new List<PlayerMapPointHistoryEntry>();


	public MapPointHistoryEntry()
	{
	}

	public MapPointHistoryEntry(MapPointType mapPointType, IPlayerCollection playerCollection)
	{
		MapPointType = mapPointType;
		foreach (Player player in playerCollection.Players)
		{
			PlayerStats.Add(new PlayerMapPointHistoryEntry
			{
				PlayerId = player.NetId
			});
		}
	}

	public PlayerMapPointHistoryEntry GetEntry(ulong playerId)
	{
		PlayerMapPointHistoryEntry playerMapPointHistoryEntry = PlayerStats.FirstOrDefault((PlayerMapPointHistoryEntry e) => e.PlayerId == playerId);
		if (playerMapPointHistoryEntry == null)
		{
			throw new InvalidOperationException($"Player with ID {playerId} not found in player stats for this run history! We have {string.Join(",", PlayerStats.Select((PlayerMapPointHistoryEntry p) => p.PlayerId))}");
		}
		return playerMapPointHistoryEntry;
	}

	public bool HasRoomOfType(RoomType roomType)
	{
		foreach (MapPointRoomHistoryEntry room in Rooms)
		{
			if (room.RoomType == roomType)
			{
				return true;
			}
		}
		return false;
	}

	[IteratorStateMachine(typeof(_003CGetRoomsOfType_003Ed__16))]
	public IEnumerable<MapPointRoomHistoryEntry> GetRoomsOfType(RoomType roomType)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetRoomsOfType_003Ed__16(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__roomType = roomType
		};
	}

	public MapPointRoomHistoryEntry? FirstRoomOfType(RoomType roomType)
	{
		foreach (MapPointRoomHistoryEntry room in Rooms)
		{
			if (room.RoomType == roomType)
			{
				return room;
			}
		}
		return null;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(MapPointType);
		writer.WriteList(Rooms);
		writer.WriteList(PlayerStats);
	}

	public void Deserialize(PacketReader reader)
	{
		MapPointType = reader.ReadEnum<MapPointType>();
		Rooms = reader.ReadList<MapPointRoomHistoryEntry>();
		PlayerStats = reader.ReadList<PlayerMapPointHistoryEntry>();
	}

	public MapPointHistoryEntry Anonymized()
	{
		return new MapPointHistoryEntry
		{
			MapPointType = MapPointType,
			Rooms = Rooms,
			PlayerStats = PlayerStats.Select((PlayerMapPointHistoryEntry p) => p.Anonymized()).ToList()
		};
	}
}
