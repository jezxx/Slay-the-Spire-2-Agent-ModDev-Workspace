using System;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Animation;

public class CreatureAnimator
{
	public const string idleTrigger = "Idle";

	public const string attackTrigger = "Attack";

	public const string powerUpTrigger = "PowerUp";

	public const string castTrigger = "Cast";

	public const string deathTrigger = "Dead";

	public const string hitTrigger = "Hit";

	public const string reviveTrigger = "Revive";

	private const float _animVariance = 0.1f;

	private readonly MegaSprite _spineController;

	private AnimState _currentState;

	private readonly AnimState _anyState;

	public event Action<string>? BoundsUpdated;

	public CreatureAnimator(AnimState initialState, MegaSprite spineController)
	{
		_anyState = new AnimState("anyState");
		_spineController = spineController;
		_currentState = initialState;
		_spineController.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>(OnAnimationStarted));
		_spineController.ConnectAnimationCompleted(Callable.From<GodotObject, GodotObject, GodotObject>(OnAnimationCompleted));
		_spineController.ConnectAnimationInterrupted(Callable.From<GodotObject, GodotObject, GodotObject>(OnAnimationInterrupted));
		SetNextState(initialState);
		if (!(initialState.Id == "idle_loop"))
		{
			return;
		}
		MegaAnimationState animationState = _spineController.GetAnimationState();
		using MegaTrackEntry megaTrackEntry = animationState.GetCurrent(0);
		if (megaTrackEntry != null)
		{
			megaTrackEntry.SetTrackTime(Rng.Chaotic.NextFloat(megaTrackEntry.GetAnimationEnd()));
			animationState.Update(0f);
			MegaSkeleton skeleton = _spineController.GetSkeleton();
			if (skeleton != null)
			{
				animationState.Apply(skeleton);
			}
		}
	}

	public void AddAnyState(string trigger, AnimState state, Func<bool>? condition = null)
	{
		_anyState.AddBranch(trigger, state, condition);
	}

	public void SetTrigger(string trigger)
	{
		AnimState animState = _anyState.CallTrigger(trigger);
		if (animState == null)
		{
			animState = _currentState.CallTrigger(trigger);
		}
		if (animState != null)
		{
			SetNextState(animState);
		}
	}

	public bool HasTrigger(string trigger)
	{
		return _anyState.HasTrigger(trigger);
	}

	private void SetNextState(AnimState state)
	{
		if (_currentState.BoundsContainer != null)
		{
			this.BoundsUpdated?.Invoke(_currentState.BoundsContainer);
		}
		_currentState = state;
		if (!_spineController.HasAnimation(_currentState.Id))
		{
			string value = (_spineController.BoundObject as Node)?.Name.ToString() ?? "unknown";
			Log.Warn($"could not find '{_currentState.Id}' animation on '{value}'");
			return;
		}
		MegaAnimationState animationState = _spineController.GetAnimationState();
		animationState.SetAnimation(_currentState.Id, _currentState.IsLooping);
		if (_currentState.IsLooping)
		{
			using MegaTrackEntry megaTrackEntry = animationState.GetCurrent(0);
			if (megaTrackEntry != null)
			{
				OffsetLoopingAnimation(megaTrackEntry);
			}
		}
		if (state.BoundsContainer != null)
		{
			this.BoundsUpdated?.Invoke(state.BoundsContainer);
		}
		AnimState nextState = _currentState.GetNextState();
		if (nextState != null)
		{
			AddNextState(nextState);
		}
	}

	private void AddNextState(AnimState? state)
	{
		if (state == null)
		{
			return;
		}
		if (!_spineController.HasAnimation(state.Id))
		{
			string value = (_spineController.BoundObject as Node)?.Name.ToString() ?? "unknown";
			Log.Warn($"could not find '{state.Id}' animation (queued) on '{value}'");
			return;
		}
		MegaAnimationState animationState = _spineController.GetAnimationState();
		if (state.IsLooping)
		{
			using MegaTrackEntry track = animationState.AddAnimationTracked(state.Id, 0f, state.IsLooping);
			OffsetLoopingAnimation(track);
		}
		else
		{
			animationState.AddAnimation(state.Id, 0f, state.IsLooping);
		}
		AnimState nextState = state.GetNextState();
		if (nextState != null)
		{
			AddNextState(nextState);
		}
	}

	private void OnAnimationStarted(GodotObject _, GodotObject __, GodotObject ___)
	{
		AnimState currentState = _currentState;
		if (currentState != null && !currentState.HasLooped && currentState.BoundsContainer != null)
		{
			this.BoundsUpdated?.Invoke(_currentState.BoundsContainer);
		}
	}

	private void OnAnimationCompleted(GodotObject _, GodotObject __, GodotObject ___)
	{
		AnimState currentState = _currentState;
		if (currentState != null && !currentState.HasLooped && currentState.BoundsContainer != null)
		{
			this.BoundsUpdated?.Invoke(_currentState.BoundsContainer);
		}
		currentState = _currentState;
		if (currentState != null && currentState.IsLooping && !currentState.HasLooped)
		{
			_currentState.MarkHasLooped();
		}
		AnimState nextState = _currentState.GetNextState();
		if (nextState != null)
		{
			_currentState = nextState;
		}
	}

	private void OnAnimationInterrupted(GodotObject _, GodotObject __, GodotObject ___)
	{
		if (_currentState.BoundsContainer != null)
		{
			this.BoundsUpdated?.Invoke(_currentState.BoundsContainer);
		}
	}

	private void OffsetLoopingAnimation(MegaTrackEntry track)
	{
		track.SetTimeScale(Rng.Chaotic.NextFloat(0.9f, 1.1f));
		float animationEnd = track.GetAnimationEnd();
		track.SetTrackTime((animationEnd + Rng.Chaotic.NextFloat(-0.1f, 0.1f)) % animationEnd);
	}
}
