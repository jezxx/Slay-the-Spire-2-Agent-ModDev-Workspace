using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms;

/// <summary>
/// Handles shop rooms by attempting to buy every affordable item.
/// </summary>
public class ShopRoomHandler : IRoomHandler, IHandler
{
	private const string _roomPath = "/root/Game/RootSceneContainer/Run/RoomContainer/MerchantRoom";

	private readonly Func<Task, CancellationToken, Task> _drainOverlayScreensUntil;

	public RoomType[] HandledTypes => new RoomType[1] { RoomType.Shop };

	public TimeSpan Timeout => TimeSpan.FromSeconds(120L);

	/// <param name="drainOverlayScreensUntil">
	/// Awaits a task while draining any overlay screen it opens. Buying an item can open a
	/// screen, and the purchase does not complete until that screen is resolved, so the shop
	/// cannot wait for the drain that runs between rooms.
	/// </param>
	public ShopRoomHandler(Func<Task, CancellationToken, Task> drainOverlayScreensUntil)
	{
		_drainOverlayScreensUntil = drainOverlayScreensUntil;
	}

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for shop room");
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		NMerchantRoom room = await WaitHelper.ForNode<NMerchantRoom>(root, "/root/Game/RootSceneContainer/Run/RoomContainer/MerchantRoom", ct);
		AutoSlayLog.Action("Opening merchant inventory");
		room.OpenInventory();
		await Task.Delay(500, ct);
		int maxAttempts = 50;
		int attempts = 0;
		while (attempts < maxAttempts)
		{
			ct.ThrowIfCancellationRequested();
			attempts++;
			List<NMerchantSlot> list = (from slot in room.Inventory.GetAllSlots()
				where !(slot is NMerchantCardRemoval)
				where slot.Entry.IsStocked && slot.Entry.EnoughGold
				select slot).ToList();
			if (list.Count == 0)
			{
				AutoSlayLog.Action("No more affordable items to buy");
				break;
			}
			NMerchantSlot nMerchantSlot = random.NextItem(list);
			AutoSlayLog.Action($"Buying {nMerchantSlot.GetType().Name} (cost: {nMerchantSlot.Entry.Cost})");
			await _drainOverlayScreensUntil(nMerchantSlot.Entry.OnTryPurchaseWrapper(room.Inventory.Inventory), ct);
			await Task.Delay(300, ct);
		}
		NBackButton nBackButton = UiHelper.FindFirst<NBackButton>(room);
		if (nBackButton != null)
		{
			AutoSlayLog.Action("Closing inventory");
			await UiHelper.Click(nBackButton);
			await Task.Delay(300, ct);
		}
		NProceedButton proceedButton = room.ProceedButton;
		AutoSlayLog.Action("Clicking proceed");
		await UiHelper.Click(proceedButton);
	}
}
