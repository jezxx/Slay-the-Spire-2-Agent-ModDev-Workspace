using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class StunIntent : AbstractIntent
{
	private const string _intentPrefix = "STUN";

	private const string _atlasName = "intent_atlas";

	private const string _spriteName = "intent_stun";

	private const string _spritePath = "atlases/intent_atlas.sprites/intent_stun.tres";

	public override IntentType IntentType => IntentType.Stun;

	protected override string IntentPrefix => "STUN";

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_stun.tres";

	/// <summary>
	/// A special HoverTip override for the StunIntent so that it can be used in a static context
	/// ie: for the Whistle card.
	/// </summary>
	/// <remarks>
	/// The intent sprite is fetched straight from the resident intent atlas rather than through
	/// <see cref="P:MegaCrit.Sts2.Core.Assets.PreloadManager.Cache" />. This tip is stored on the canonical Whistle card model
	/// and re-shown across room transitions (e.g. Tanx's Whistle boss relic preview). Loading it
	/// through the cache registers it as a missed-cache asset, which the next room transition
	/// disposes out from under the stored tip, throwing ObjectDisposedException on hover (PRG-7151).
	/// The intent atlas is loaded at startup and never unloaded, and AtlasManager self-heals
	/// disposed sprite wrappers, so the reference stays valid for the process lifetime.
	/// </remarks>
	public static HoverTip GetStaticHoverTip()
	{
		if (!AtlasManager.IsAtlasLoaded("intent_atlas"))
		{
			AtlasManager.LoadAtlas("intent_atlas");
		}
		Texture2D sprite = AtlasManager.GetSprite("intent_atlas", "intent_stun");
		LocString description = new LocString("intents", "STUN.description");
		return new HoverTip(new LocString("intents", "STUN.title"), description, sprite);
	}
}
