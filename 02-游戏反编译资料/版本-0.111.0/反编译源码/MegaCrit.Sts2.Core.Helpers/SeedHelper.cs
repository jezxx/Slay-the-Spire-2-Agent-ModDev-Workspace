using System.Text;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Helpers;

public static class SeedHelper
{
	/// <summary>
	/// Possible characters for the seed.
	/// O and I are not included. They are replaced by 0 and 1.
	/// </summary>
	private const string _characters = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

	public const int seedDefaultLength = 12;

	public static string GetRandomSeed(Rng? rng = null, int length = 12)
	{
		if (rng == null)
		{
			rng = Rng.Chaotic;
		}
		string text;
		do
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append(rng.NextItem("0123456789ABCDEFGHJKLMNPQRSTUVWXYZ"));
			}
			text = stringBuilder.ToString();
		}
		while (BadWordChecker.ContainsBadWord(text));
		return text;
	}

	public static string CanonicalizeSeed(string seed)
	{
		seed = seed.ToUpperInvariant();
		seed = seed.Replace('O', '0');
		seed = seed.Replace('I', '1');
		seed = seed.Trim();
		return seed;
	}
}
