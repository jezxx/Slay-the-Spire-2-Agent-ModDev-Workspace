using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

/// <summary>
/// Handles entering combat from events.
/// This isn't a synchronizer per se, as it doesn't pass any messages.
/// </summary>
public class EventCombatSynchronizer
{
	private class EventCombatState
	{
		public required EncounterModel canonicalEncounter;

		public required IReadOnlyList<Reward> extraRewards;

		public bool shouldResumeAfterCombat;
	}

	private readonly IRunState _runState;

	private readonly List<EventCombatState?> _states = new List<EventCombatState>();

	private readonly Logger _logger = new Logger("EventCombatSynchronizer", LogType.GameSync);

	private EventModel? _canonicalEvent;

	/// <summary>
	/// The mutable encounter that will be used when we transition to combat.
	/// This is only set if the event used to initialize the object has LayoutType == EventLayoutType.Combat.
	/// </summary>
	public EncounterModel? MutableEncounterForLayout { get; private set; }

	/// <summary>
	/// The combat state that will be used when we transition to combat.
	/// This is only set if the event used to initialize the object has LayoutType == EventLayoutType.Combat.
	/// </summary>
	public CombatState? CombatStateForLayout { get; private set; }

	public EventCombatSynchronizer(IPlayerCollection playerCollection, IRunState runState)
	{
		_runState = runState;
		foreach (Player player in playerCollection.Players)
		{
			_states.Add(null);
		}
	}

	/// <summary>
	/// Should be called whenever an event is entered that can transition to combat.
	/// </summary>
	public void InitializeForEvent(EventModel localEvent)
	{
		_canonicalEvent = localEvent.CanonicalInstance;
		if (localEvent.LayoutType != EventLayoutType.Combat)
		{
			return;
		}
		if (localEvent.CanonicalEncounter == null)
		{
			throw new InvalidOperationException($"Canonical encounter is not set for event {localEvent.Id} with combat layout!");
		}
		MutableEncounterForLayout = localEvent.CanonicalEncounter.ToMutable();
		MutableEncounterForLayout.GenerateMonstersWithSlots(_runState);
		CombatStateForLayout = CreateCombatState(MutableEncounterForLayout);
		foreach (Player player in _runState.Players)
		{
			CombatStateForLayout.AddPlayer(player);
		}
		foreach (var monstersWithSlot in CombatStateForLayout.Encounter.MonstersWithSlots)
		{
			MonsterModel item = monstersWithSlot.Item1;
			string item2 = monstersWithSlot.Item2;
			Creature creature = CombatStateForLayout.CreateCreature(item, CombatSide.Enemy, item2);
			CombatStateForLayout.AddCreature(creature);
		}
	}

	/// <summary>
	/// Called when an individual event instance is prepared to enter combat.
	/// When a shared event is in-progress, there is one EventModel instance per player. Each one calls this method
	/// individually to match the semantics of how events normally work. Once all event instances have called in, this
	/// object automatically does the transition to combat.
	/// </summary>
	/// <param name="canonicalEncounter">The canonical encounter to transition to. This must match among all calls to
	/// this method in a single shared event.</param>
	/// <param name="player">The player to which the event belongs.</param>
	/// <param name="extraRewards">Any extra rewards that should be granted to the player.</param>
	/// <param name="shouldResumeAfterCombat">Whether to resume the event post-combat or not. Must be the same among all
	/// calls to this method in a single shared event.</param>
	public void ReadyToEnterCombat(EncounterModel canonicalEncounter, Player player, IReadOnlyList<Reward> extraRewards, bool shouldResumeAfterCombat)
	{
		_logger.Debug($"Player {player.NetId} is ready to enter combat {canonicalEncounter.Id} (extra rewards: {extraRewards.Count}, should resume after: {shouldResumeAfterCombat})");
		int playerSlotIndex = _runState.GetPlayerSlotIndex(player);
		if (_states[playerSlotIndex] != null)
		{
			throw new InvalidOperationException($"Player {player.NetId} became ready to enter combat {canonicalEncounter.Id},but they are already set to ready for {_states[playerSlotIndex].canonicalEncounter.Id}!");
		}
		_states[playerSlotIndex] = new EventCombatState
		{
			canonicalEncounter = canonicalEncounter,
			extraRewards = extraRewards,
			shouldResumeAfterCombat = shouldResumeAfterCombat
		};
		if (_states.All((EventCombatState s) => s != null))
		{
			EnterCombat();
		}
	}

	/// <summary>
	/// Called after all event instances call <see cref="M:MegaCrit.Sts2.Core.Multiplayer.Game.EventCombatSynchronizer.ReadyToEnterCombat(MegaCrit.Sts2.Core.Models.EncounterModel,MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Rewards.Reward},System.Boolean)" />.
	/// Does the actual transition into combat.
	/// </summary>
	private void EnterCombat()
	{
		if (_canonicalEvent == null)
		{
			throw new InvalidOperationException("GenerateInternalCombatState must be called before EnterCombat!");
		}
		EncounterModel canonicalEncounter = _states[0].canonicalEncounter;
		bool shouldResumeAfterCombat = _states[0].shouldResumeAfterCombat;
		for (int i = 0; i < _states.Count; i++)
		{
			if (_states[i].shouldResumeAfterCombat != shouldResumeAfterCombat)
			{
				throw new InvalidOperationException($"Event for player {_runState.Players[i].NetId} tried to start event combatwith shouldResumeAfterCombat set to {_states[i].shouldResumeAfterCombat}, but the host says it should be {shouldResumeAfterCombat}!");
			}
			if (_states[i].canonicalEncounter != canonicalEncounter)
			{
				throw new InvalidOperationException($"Event for player {_runState.Players[i].NetId} tried to start event combatwith encounter {_states[i].canonicalEncounter.Id}, but the host says it should be {canonicalEncounter.Id}!");
			}
		}
		_logger.Debug($"Entering combat {canonicalEncounter.Id} from event");
		CombatState combatState = CombatStateForLayout ?? CreateCombatState(canonicalEncounter.ToMutable());
		CombatRoom combatRoom = new CombatRoom(combatState)
		{
			ShouldCreateCombat = (_canonicalEvent.LayoutType != EventLayoutType.Combat),
			ShouldResumeParentEventAfterCombat = shouldResumeAfterCombat,
			ParentEventId = _canonicalEvent.Id
		};
		foreach (EventCombatState state in _states)
		{
			foreach (Reward extraReward in state.extraRewards)
			{
				combatRoom.AddExtraReward(extraReward.Player, extraReward);
			}
		}
		TaskHelper.RunSafely(RunManager.Instance.EnterRoomWithoutExitingCurrentRoom(combatRoom, _canonicalEvent.LayoutType != EventLayoutType.Combat));
	}

	/// <summary>
	/// Resets the combat synchronizer state for the next combat.
	/// This must be called before another event combat can begin.
	/// </summary>
	public void ResetState()
	{
		foreach (Creature item in CombatStateForLayout?.Creatures ?? Array.Empty<Creature>())
		{
			CombatStateForLayout?.RemoveCreature(item);
		}
		_canonicalEvent = null;
		MutableEncounterForLayout = null;
		CombatStateForLayout = null;
		for (int i = 0; i < _states.Count; i++)
		{
			_states[i] = null;
		}
	}

	private CombatState CreateCombatState(EncounterModel mutableEncounter)
	{
		return new CombatState(mutableEncounter, _runState, _runState.Modifiers, _runState.BadgeModels, _runState.MultiplayerScalingModel);
	}
}
