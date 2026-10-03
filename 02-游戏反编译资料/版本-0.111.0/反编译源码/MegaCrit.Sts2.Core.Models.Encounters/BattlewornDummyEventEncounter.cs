using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public abstract class BattlewornDummyEventEncounter : EncounterModel
{
	private const string _ranOutOfTimeKey = "RanOutOfTime";

	private bool _ranOutOfTime;

	public bool RanOutOfTime
	{
		get
		{
			return _ranOutOfTime;
		}
		set
		{
			AssertMutable();
			_ranOutOfTime = value;
		}
	}

	public override Dictionary<string, string> SaveCustomState()
	{
		return new Dictionary<string, string> { ["RanOutOfTime"] = RanOutOfTime.ToString() };
	}

	public override void LoadCustomState(Dictionary<string, string> state)
	{
		RanOutOfTime = bool.Parse(state["RanOutOfTime"]);
	}
}
