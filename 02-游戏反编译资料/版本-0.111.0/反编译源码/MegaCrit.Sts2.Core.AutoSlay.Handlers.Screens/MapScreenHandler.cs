using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;

/// <summary>
/// Handles map screen navigation between rooms.
/// </summary>
public class MapScreenHandler : IScreenHandler, IHandler
{
	private TaskCompletionSource? _roomEnteredTcs;

	public Type ScreenType => typeof(NMapScreen);

	public TimeSpan Timeout => TimeSpan.FromSeconds(30L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.EnterScreen("NMapScreen");
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		NRun runNode = root.GetNode<NRun>("/root/Game/RootSceneContainer/Run");
		await WaitHelper.Until(() => runNode.GlobalUi.MapScreen.IsVisibleInTree(), ct, AutoSlayConfig.mapScreenTimeout, "Map screen not visible");
		NMapScreen mapScreen = runNode.GlobalUi.MapScreen;
		NMapPoint nextRoom = null;
		try
		{
			await WaitHelper.Until(() => (nextRoom = SelectNextRoom(mapScreen))?.IsEnabled ?? false, ct, AutoSlayConfig.mapPointEnabledTimeout, "Map point not enabled");
		}
		catch (AutoSlayTimeoutException)
		{
			AutoSlayLog.Info("[AutoSlay] Map point never became travelable: node=" + ((nextRoom == null) ? "none" : nextRoom.Point.coord.ToString()) + ", state=" + ((nextRoom == null) ? "n/a" : nextRoom.State.ToString()) + ", " + $"screenTravelEnabled={mapScreen.IsTravelEnabled}");
			throw;
		}
		AutoSlayLog.Action($"Selecting room at ({nextRoom.Point.coord.row}, {nextRoom.Point.coord.col})");
		_roomEnteredTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		RunManager.Instance.RoomEntered += OnRoomEntered;
		try
		{
			await UiHelper.Click(nextRoom);
			await WaitHelper.ForTask(_roomEnteredTcs.Task, ct, AutoSlayConfig.mapScreenTimeout, "Room not entered after map click");
		}
		finally
		{
			RunManager.Instance.RoomEntered -= OnRoomEntered;
			_roomEnteredTcs = null;
		}
		AutoSlayLog.ExitScreen("NMapScreen");
	}

	/// <summary>
	/// Resolves the next room to travel to from the live map points, or null when the expected node is
	/// not present yet (for example mid act-transition while the new act's map is still being generated).
	/// Returning null keeps the caller polling instead of latching onto a stale node from the prior act.
	/// </summary>
	private static NMapPoint? SelectNextRoom(NMapScreen mapScreen)
	{
		List<NMapPoint> source = UiHelper.FindAll<NMapPoint>(mapScreen);
		RunState runState = RunManager.Instance.DebugOnlyGetState();
		if (runState.VisitedMapCoords.Count == 0)
		{
			return source.FirstOrDefault((NMapPoint mp) => mp.Point.coord.row == 0);
		}
		IReadOnlyList<MapCoord> visitedMapCoords = runState.VisitedMapCoords;
		MapCoord lastCoord = visitedMapCoords[visitedMapCoords.Count - 1];
		MapPoint child = source.FirstOrDefault((NMapPoint mp) => mp.Point.coord.Equals(lastCoord))?.Point.Children.FirstOrDefault();
		if (child != null)
		{
			return source.FirstOrDefault((NMapPoint mp) => mp.Point.coord.Equals(child.coord));
		}
		return null;
	}

	private void OnRoomEntered()
	{
		_roomEnteredTcs?.TrySetResult();
	}
}
