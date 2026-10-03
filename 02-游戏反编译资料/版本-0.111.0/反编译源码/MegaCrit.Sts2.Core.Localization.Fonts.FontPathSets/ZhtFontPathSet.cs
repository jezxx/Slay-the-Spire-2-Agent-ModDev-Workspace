using System;

namespace MegaCrit.Sts2.Core.Localization.Fonts.FontPathSets;

public class ZhtFontPathSet : FontPathSet
{
	private const string _regular = "res://themes/fonts/zht/noto_sans_mono_cjktc_regular_shared.tres";

	private const string _bold = "res://themes/fonts/zht/source_han_serif_tc_bold_shared.tres";

	private const string _italic = "res://themes/fonts/zht/source_han_serif_tc_medium_shared.tres";

	public override string GetPath(FontType type)
	{
		return type switch
		{
			FontType.Regular => "res://themes/fonts/zht/noto_sans_mono_cjktc_regular_shared.tres", 
			FontType.Bold => "res://themes/fonts/zht/source_han_serif_tc_bold_shared.tres", 
			FontType.Italic => "res://themes/fonts/zht/source_han_serif_tc_medium_shared.tres", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
