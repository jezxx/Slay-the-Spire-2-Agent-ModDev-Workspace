using Godot;

namespace MegaCrit.Sts2.Core.RichTextTags;

public static class RichTextUtil
{
	public static readonly Variant colorKey;

	public static readonly Variant visibleKey;

	static RichTextUtil()
	{
		string from = "color";
		colorKey = Variant.From(in from);
		from = "visible";
		visibleKey = Variant.From(in from);
	}
}
