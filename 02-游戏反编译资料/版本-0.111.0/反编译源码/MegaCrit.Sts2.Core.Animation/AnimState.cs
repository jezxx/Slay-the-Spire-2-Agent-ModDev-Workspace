using System;
using System.Collections.Generic;
using System.Linq;

namespace MegaCrit.Sts2.Core.Animation;

public class AnimState
{
	private struct Branch
	{
		public AnimState state;

		public Func<bool>? condition;
	}

	public const string attackAnim = "attack";

	public const string castAnim = "cast";

	public const string dieAnim = "die";

	public const string hurtAnim = "hurt";

	public const string idleAnim = "idle_loop";

	public const string lowHealthIdleAnim = "low_health_loop";

	public const string reviveAnim = "revive";

	public const string stunAnim = "stun";

	private readonly Dictionary<string, List<Branch>> _triggerBranchedStates;

	private readonly List<Branch> _nextStates;

	public string Id { get; }

	/// <summary>
	/// Is this a looping animation?
	/// </summary>
	public bool IsLooping { get; }

	/// <summary>
	/// If this is a looping animation, has it already looped at least once?
	/// </summary>
	public bool HasLooped { get; private set; }

	/// <summary>
	/// For states that immediately transition to another state on completion.
	/// TODO: replace this so that everything just uses AddNextState() and _nextStates instead
	/// </summary>
	public AnimState? NextState { get; set; }

	public string? BoundsContainer { get; init; }

	public AnimState? GetNextState()
	{
		foreach (Branch nextState in _nextStates)
		{
			Func<bool>? condition = nextState.condition;
			if (condition == null || condition())
			{
				return nextState.state;
			}
		}
		return NextState;
	}

	public AnimState(string id, bool isLooping = false)
	{
		Id = id;
		IsLooping = isLooping;
		_triggerBranchedStates = new Dictionary<string, List<Branch>>();
		_nextStates = new List<Branch>();
	}

	public void AddNextState(AnimState state)
	{
		AddNextState(state, () => true);
	}

	/// <summary>
	/// An animation transition that happens after the current animation is finished.
	/// Can interrupt an animation
	/// </summary>
	/// <param name="state">The state to transition to</param>
	/// <param name="condition">the required condition to perform the transition</param>
	public void AddNextState(AnimState state, Func<bool>? condition)
	{
		Branch branch = default(Branch);
		branch.state = state;
		branch.condition = condition;
		Branch item = branch;
		_nextStates.Add(item);
	}

	/// <summary>
	/// An animation transition caused by a trigger.
	/// Can interrupt an animation
	/// </summary>
	/// <param name="trigger">The trigger for the transition</param>
	/// <param name="state">The state to transition to</param>
	/// <param name="condition">the required condition to perform the transition</param>
	public void AddBranch(string trigger, AnimState state, Func<bool>? condition = null)
	{
		Branch branch = default(Branch);
		branch.state = state;
		branch.condition = condition;
		Branch item = branch;
		if (!_triggerBranchedStates.TryGetValue(trigger, out List<Branch> value))
		{
			value = new List<Branch>();
			_triggerBranchedStates[trigger] = value;
		}
		value.Add(item);
	}

	public void RemoveBranch(string trigger, string stateId)
	{
		string stateId2 = stateId;
		if (_triggerBranchedStates.TryGetValue(trigger, out List<Branch> branches))
		{
			List<Branch> list = branches.Where((Branch b) => b.state.Id == stateId2).ToList();
			list.ForEach(delegate(Branch b)
			{
				branches.Remove(b);
			});
		}
	}

	public AnimState? CallTrigger(string trigger)
	{
		if (_triggerBranchedStates.TryGetValue(trigger, out List<Branch> value))
		{
			foreach (Branch item in value)
			{
				Func<bool>? condition = item.condition;
				if (condition == null || condition())
				{
					return item.state;
				}
			}
		}
		return null;
	}

	public bool HasTrigger(string trigger)
	{
		return _triggerBranchedStates.ContainsKey(trigger);
	}

	/// <summary>
	/// Mark that this animation has looped at least once.
	/// </summary>
	public void MarkHasLooped()
	{
		HasLooped = true;
	}
}
