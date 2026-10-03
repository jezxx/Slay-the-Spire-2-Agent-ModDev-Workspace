using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

/// <summary>
/// C# bindings for SpineTrackEntry.
/// </summary>
public class MegaTrackEntry : MegaSpineBinding
{
	/// <summary>
	/// Cap on how far <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaTrackEntry.GetQueuedAnimationNames" /> will walk. Real queues are a handful of entries
	/// deep, so hitting this means something queued far more than intended.
	/// </summary>
	private const int _maxQueueDepth = 32;

	protected override string SpineClassName => "SpineTrackEntry";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlyArray<string>(new string[11]
	{
		"get_animation", "get_animation_end", "get_track_complete", "get_track_time", "is_complete", "get_loop", "get_next", "set_loop", "set_time_scale", "set_track_time",
		"set_mix_duration"
	});

	public MegaTrackEntry(Variant native)
		: base(native)
	{
	}

	private MegaAnimation GetAnimation()
	{
		using Variant native = Call("get_animation");
		return new MegaAnimation(native);
	}

	/// <summary>
	/// Name of this entry's animation. Returns the value rather than the wrapper so no transient
	/// <see cref="T:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimation" /> escapes; the native read is kept GC-safe by the GC.KeepAlive in
	/// MegaSpineBinding.Call (PRG-6985).
	/// </summary>
	public string GetAnimationName()
	{
		using MegaAnimation megaAnimation = GetAnimation();
		return megaAnimation.GetName();
	}

	/// <summary>
	/// Duration of this entry's animation. See <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaTrackEntry.GetAnimationName" />.
	/// </summary>
	public float GetAnimationDuration()
	{
		using MegaAnimation megaAnimation = GetAnimation();
		return megaAnimation.GetDuration();
	}

	private MegaTrackEntry? GetNext()
	{
		using Variant native = Call("get_next");
		if (native.VariantType != Variant.Type.Object || native.AsGodotObject() == null)
		{
			return null;
		}
		return new MegaTrackEntry(native);
	}

	/// <summary>
	/// Animation names for this entry and every entry queued behind it on its track, in playback order.
	/// Returns values rather than wrappers so no transient <see cref="T:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaTrackEntry" /> escapes, and disposes
	/// every entry it walks on the calling thread, on any exit path (PRG-6985).
	/// See <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaTrackEntry.GetAnimationName" />.
	/// </summary>
	internal IReadOnlyList<string> GetQueuedAnimationNames()
	{
		List<string> list = new List<string>();
		MegaTrackEntry megaTrackEntry = this;
		try
		{
			int num = 0;
			while (megaTrackEntry != null)
			{
				if (num >= 32)
				{
					Log.Warn($"spine track queue is at least {32} entries deep; reporting only the first {32}");
					break;
				}
				list.Add(megaTrackEntry.GetAnimationName());
				MegaTrackEntry next = megaTrackEntry.GetNext();
				DisposeIfOwned(megaTrackEntry);
				megaTrackEntry = next;
				num++;
			}
		}
		finally
		{
			DisposeIfOwned(megaTrackEntry);
		}
		return list;
	}

	/// <summary>
	/// Releases a wrapper the walk minted. `this` belongs to the caller and is never disposed here.
	/// </summary>
	private void DisposeIfOwned(MegaTrackEntry? entry)
	{
		if (entry != null && entry != this)
		{
			entry.Dispose();
		}
	}

	public float GetAnimationEnd()
	{
		return Call("get_animation_end").AsSingle();
	}

	public float GetTrackComplete()
	{
		return Call("get_track_complete").AsSingle();
	}

	public float GetTrackTime()
	{
		return Call("get_track_time").AsSingle();
	}

	public bool IsComplete()
	{
		return Call("is_complete").AsBool();
	}

	public bool IsLoop()
	{
		return Call("get_loop").AsBool();
	}

	public void SetLoop(bool loop)
	{
		Call("set_loop", loop);
	}

	public void SetTimeScale(float scale)
	{
		Call("set_time_scale", scale);
	}

	public void SetTrackTime(float time)
	{
		Call("set_track_time", time);
	}

	public void SetMixDuration(float time)
	{
		Call("set_mix_duration", time);
	}
}
