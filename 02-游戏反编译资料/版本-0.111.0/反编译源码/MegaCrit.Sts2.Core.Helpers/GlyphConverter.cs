using Godot;

namespace MegaCrit.Sts2.Core.Helpers;

public static class GlyphConverter
{
	private static TextServer GetTextServer()
	{
		return TextServerManager.Singleton.GetPrimaryInterface();
	}

	public static uint CharToGlyphIdx(Rid font, char c)
	{
		return (uint)GetTextServer().FontGetGlyphIndex(font, 1L, c, 0L);
	}

	public static char GlyphIdxToChar(CharFXTransform charFx)
	{
		return (char)GetTextServer().FontGetCharFromGlyphIndex(charFx.Font, 1L, charFx.GlyphIndex);
	}
}
