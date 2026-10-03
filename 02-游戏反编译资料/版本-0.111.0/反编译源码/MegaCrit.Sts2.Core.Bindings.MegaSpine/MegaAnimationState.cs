using System;
using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

/// <summary>
/// C# bindings for SpineAnimationState.
/// </summary>
public class MegaAnimationState : MegaSpineBinding
{
	protected override string SpineClassName => "SpineAnimationState";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlyArray<string>(new string[7] { "add_animation", "add_empty_animation", "apply", "get_current", "set_animation", "set_time_scale", "update" });

	public MegaAnimationState(Variant native)
		: base(native)
	{
	}

	/// <summary>
	/// Queues an animation on the given track. Fire-and-forget; see <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState.SetAnimation(System.String,System.Boolean,System.Int32)" /> for the
	/// calling-thread teardown rationale. Use <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState.AddAnimationTracked(System.String,System.Single,System.Boolean,System.Int32)" /> when you need to
	/// configure the queued entry before it plays.
	/// </summary>
	public void AddAnimation(string animationName, float delay = 0f, bool loop = true, int trackId = 0)
	{
		using (Call("add_animation", animationName, delay, loop, trackId))
		{
		}
	}

	/// <summary>
	/// Queues an animation and returns its track entry so the caller can configure it (e.g. randomize start
	/// time and speed) before it plays. The returned wrapper is the caller's to dispose: wrap it in
	/// <c>using</c> so its native release (and the spine-godot signal disconnect) stays on the calling
	/// thread rather than the .NET finalizer thread (PRG-6985).
	/// </summary>
	public MegaTrackEntry AddAnimationTracked(string animationName, float delay = 0f, bool loop = true, int trackId = 0)
	{
		using Variant native = Call("add_animation", animationName, delay, loop, trackId);
		return new MegaTrackEntry(native);
	}

	public void Apply(MegaSkeleton skeleton)
	{
		Call("apply", skeleton.BoundObject);
	}

	public MegaTrackEntry? GetCurrent(int trackIndex)
	{
		using Variant native = Call("get_current", trackIndex);
		if (native.VariantType != Variant.Type.Object || native.AsGodotObject() == null)
		{
			return null;
		}
		return new MegaTrackEntry(native);
	}

	/// <summary>
	/// Returns the name of the animation currently playing on the given track, or null if no track is
	/// active. Value-only so no transient wrapper escapes; the native reads are kept GC-safe by the
	/// GC.KeepAlive in MegaSpineBinding.Call (PRG-6985).
	/// </summary>
	public string? GetCurrentAnimationName(int trackIndex = 0)
	{
		using MegaTrackEntry megaTrackEntry = GetCurrent(trackIndex);
		return megaTrackEntry?.GetAnimationName();
	}

	/// <summary>
	/// Animation names on the given track in playback order: the one playing now, then everything queued
	/// behind it by AddAnimation. Empty if the track is inactive. Value-only so no transient wrapper
	/// escapes; see <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState.GetCurrentAnimationName(System.Int32)" />.
	/// </summary>
	public IReadOnlyList<string> GetQueuedAnimationNames(int trackIndex = 0)
	{
		using MegaTrackEntry megaTrackEntry = GetCurrent(trackIndex);
		return megaTrackEntry?.GetQueuedAnimationNames() ?? Array.Empty<string>();
	}

	/// <summary>
	/// Returns the duration of the animation currently playing on the given track, or null if no track
	/// is active. See <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState.GetCurrentAnimationName(System.Int32)" />.
	/// </summary>
	public float? GetCurrentAnimationDuration(int trackIndex = 0)
	{
		using MegaTrackEntry megaTrackEntry = GetCurrent(trackIndex);
		return megaTrackEntry?.GetAnimationDuration();
	}

	/// <summary>
	/// Plays an animation on the given track. Fire-and-forget: the SpineTrackEntry that set_animation returns
	/// is a fresh SpineObjectWrapper RefCounted whose ~Object runs the spine-godot signal disconnect. We do
	/// not hand it to callers, so its source Variant is released on the calling thread here, keeping that
	/// disconnect off the .NET finalizer thread (PRG-6985). Call <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState.GetCurrent(System.Int32)" /> afterwards if you
	/// need to configure the now-current entry.
	/// </summary>
	public void SetAnimation(string animationName, bool loop = true, int trackId = 0)
	{
		using (Call("set_animation", animationName, loop, trackId))
		{
		}
	}

	/// <summary>
	/// Queues an empty animation on the given track (fades the track out). Fire-and-forget; see
	/// <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState.SetAnimation(System.String,System.Boolean,System.Int32)" /> for the calling-thread teardown rationale.
	/// </summary>
	public void AddEmptyAnimation(int trackId = 0)
	{
		using (Call("add_empty_animation", trackId, 0, 0))
		{
		}
	}

	public void SetTimeScale(float scale)
	{
		Call("set_time_scale", scale);
	}

	public void Update(float delta)
	{
		Call("update", delta);
	}
}
