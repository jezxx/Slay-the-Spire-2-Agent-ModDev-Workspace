using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Rooms;

public class RestSiteRoom : AbstractRoom
{
	private RestSiteSynchronizer? _synchronizer;

	public override RoomType RoomType => RoomType.RestSite;

	public override ModelId? ModelId => null;

	public IReadOnlyList<RestSiteOption> Options => _synchronizer?.GetLocalOptions() ?? Array.Empty<RestSiteOption>();

	public override async Task EnterInternal(IRunState? runState, bool isRestoringRoomStackBase)
	{
		if (isRestoringRoomStackBase)
		{
			throw new InvalidOperationException("RestSiteRoom does not support room stack reconstruction.");
		}
		_synchronizer = RunManager.Instance.RestSiteSynchronizer;
		_synchronizer.BeginRestSite();
		if (runState != null)
		{
			await PreloadManager.LoadRoomRestSite(runState.Act, Options);
			ShowRoomNode(runState);
			await Hook.AfterRoomEntered(runState, this);
		}
	}

	public override async Task Exit(IRunState? runState)
	{
		RunManager.Instance.RestSiteSynchronizer.BeforeLocalRestSiteExited();
		NRestSiteRoom.Instance?.BeforeExitingRoom();
		await RunManager.Instance.RestSiteSynchronizer.AfterAllRestSitesCompleted();
		RunManager.Instance.ChecksumTracker.GenerateChecksum("Exiting rest site room", null);
	}

	public override Task Resume(AbstractRoom _, IRunState? runState)
	{
		ShowRoomNode(runState);
		return Task.CompletedTask;
	}

	private void ShowRoomNode(IRunState runState)
	{
		NRun.Instance?.SetCurrentRoom(NRestSiteRoom.Create(this, runState));
	}
}
