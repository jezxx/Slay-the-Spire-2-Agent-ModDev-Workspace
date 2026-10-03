using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public sealed class MockRecordCardChangedPilesPower : PowerModel
{
	/// <summary>
	/// Records every AfterCardChangedPiles call this power receives, as (oldPileType, newPileType).
	/// Static so a test can read it regardless of canonical/mutable model copying. Test-only: clear before use.
	/// </summary>
	public static readonly List<(PileType oldPileType, PileType? newPileType)> records = new List<(PileType, PileType?)>();

	public override bool IsMock => true;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		records.Add((oldPileType, card.Pile?.Type));
		return Task.CompletedTask;
	}
}
