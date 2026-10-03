using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Ancients;

/// <summary>
/// Represents the complete dialogue configuration for an ancient, organized by category.
/// Each ancient defines its dialogues explicitly in C# via <see cref="M:MegaCrit.Sts2.Core.Models.AncientEventModel.DefineDialogues" />.
/// Keys use X-Y format where X is the dialogue index within a collection and Y is the line index within a dialogue.
/// Each line key ends with ".ancient" or ".char" to indicate the speaker. The speaker is derived from which key
/// exists in the JSON at PopulateLocKeys time.
///
/// Dialogues that should be added to the "repeating" pool encode their repeating status in the loc keys by appending
/// "r" to the X-Y component.
/// For example, "1-0r.ancient" indicates a repeating dialogue line. When <see cref="M:MegaCrit.Sts2.Core.Entities.Ancients.AncientDialogue.PopulateLines(System.String,System.String,System.Int32)" />
/// detects the "r" suffix on the first line, it automatically sets <see cref="P:MegaCrit.Sts2.Core.Entities.Ancients.AncientDialogue.IsRepeating" /> to true.
///
/// For example:
/// "DARV.talk.IRONCLAD.0-0.ancient": "Dialogue 1, line 1 (ancient speaks)",
/// "DARV.talk.IRONCLAD.0-1.char": "Dialogue 1, line 2 (character speaks)",
/// "DARV.talk.IRONCLAD.0-2.ancient": "Dialogue 1, line 3 (ancient speaks)",
/// "DARV.talk.IRONCLAD.1-0r.ancient": "Dialogue 2, line 1 (ancient speaks, repeating)",
///
/// "Next" button text must also be supplied for every line except the last line in each dialogue.
/// Repeating dialogues use the "r" suffix on their next keys as well.
///
/// For example:
/// "DARV.talk.IRONCLAD.0-0.ancient": "Dialogue 1, line 1",
/// "DARV.talk.IRONCLAD.0-0.next": "Respond",
/// "DARV.talk.IRONCLAD.0-1.char": "Dialogue 1, line 2",
/// "DARV.talk.IRONCLAD.0-1.next": "Continue",
/// "DARV.talk.IRONCLAD.0-2.ancient": "Dialogue 1, line 3",
/// "DARV.talk.IRONCLAD.1-0r.ancient": "Dialogue 2, line 1 (repeating)",
/// "DARV.talk.IRONCLAD.1-0r.next": "Acknowledge",
/// "DARV.talk.IRONCLAD.1-1r.ancient": "Dialogue 2, line 2 (repeating)",
/// </summary>
public class AncientDialogueSet
{
	[CompilerGenerated]
	private sealed class _003CGetAllDialogues_003Ed__12 : IEnumerable<AncientDialogue>, IEnumerable, IEnumerator<AncientDialogue>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private AncientDialogue _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public AncientDialogueSet _003C_003E4__this;

		private Dictionary<string, IReadOnlyList<AncientDialogue>>.ValueCollection.Enumerator _003C_003E7__wrap1;

		private IEnumerator<AncientDialogue> _003C_003E7__wrap2;

		AncientDialogue IEnumerator<AncientDialogue>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetAllDialogues_003Ed__12(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			switch (num)
			{
			case -4:
			case -3:
			case 2:
				try
				{
					if (num == -4 || num == 2)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
				break;
			case -5:
			case 3:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally3();
				}
				break;
			}
			_003C_003E7__wrap1 = default(Dictionary<string, IReadOnlyList<AncientDialogue>>.ValueCollection.Enumerator);
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				AncientDialogueSet ancientDialogueSet = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					if (ancientDialogueSet.FirstVisitEverDialogue != null)
					{
						_003C_003E2__current = ancientDialogueSet.FirstVisitEverDialogue;
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_005b;
				case 1:
					_003C_003E1__state = -1;
					goto IL_005b;
				case 2:
					_003C_003E1__state = -4;
					goto IL_00c8;
				case 3:
					{
						_003C_003E1__state = -5;
						break;
					}
					IL_005b:
					_003C_003E7__wrap1 = ancientDialogueSet.CharacterDialogues.Values.GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00e2;
					IL_00e2:
					if (_003C_003E7__wrap1.MoveNext())
					{
						IReadOnlyList<AncientDialogue> current = _003C_003E7__wrap1.Current;
						_003C_003E7__wrap2 = current.GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00c8;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = default(Dictionary<string, IReadOnlyList<AncientDialogue>>.ValueCollection.Enumerator);
					_003C_003E7__wrap2 = ancientDialogueSet.AgnosticDialogues.GetEnumerator();
					_003C_003E1__state = -5;
					break;
					IL_00c8:
					if (_003C_003E7__wrap2.MoveNext())
					{
						AncientDialogue current2 = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current2;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					goto IL_00e2;
				}
				if (_003C_003E7__wrap2.MoveNext())
				{
					AncientDialogue current3 = _003C_003E7__wrap2.Current;
					_003C_003E2__current = current3;
					_003C_003E1__state = 3;
					return true;
				}
				_003C_003Em__Finally3();
				_003C_003E7__wrap2 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		private void _003C_003Em__Finally3()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<AncientDialogue> IEnumerable<AncientDialogue>.GetEnumerator()
		{
			_003CGetAllDialogues_003Ed__12 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CGetAllDialogues_003Ed__12(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<AncientDialogue>)this).GetEnumerator();
		}
	}

	/// <summary>
	/// First time ANY character visits this ancient (global once-only).
	/// Shown when totalAncientVisits == 0.
	/// If null, this ancient has no first-visit-ever dialogue (for example, The Architect).
	/// Example key: "DARV.talk.firstVisitEver.0-0.ancient"
	/// </summary>
	public required AncientDialogue? FirstVisitEverDialogue { get; init; }

	/// <summary>
	/// Per-character dialogues.
	/// Each <see cref="T:MegaCrit.Sts2.Core.Entities.Ancients.AncientDialogue" /> carries its own <see cref="P:MegaCrit.Sts2.Core.Entities.Ancients.AncientDialogue.VisitIndex" /> and
	/// <see cref="P:MegaCrit.Sts2.Core.Entities.Ancients.AncientDialogue.IsRepeating" /> metadata to determine when it should be shown.
	/// Example key: "DARV.talk.IRONCLAD.0-0.ancient"
	/// </summary>
	public required Dictionary<string, IReadOnlyList<AncientDialogue>> CharacterDialogues { get; init; }

	/// <summary>
	/// Character-agnostic dialogues. Used as fallback when no visit-specific dialogue applies.
	/// Example key: "DARV.talk.ANY.0-0.ancient"
	/// </summary>
	public required IReadOnlyList<AncientDialogue> AgnosticDialogues { get; init; } = Array.Empty<AncientDialogue>();


	/// <summary>
	/// Get every dialogue in this set.
	/// </summary>
	[IteratorStateMachine(typeof(_003CGetAllDialogues_003Ed__12))]
	public IEnumerable<AncientDialogue> GetAllDialogues()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAllDialogues_003Ed__12(-2)
		{
			_003C_003E4__this = this
		};
	}

	/// <summary>
	/// Get valid dialogues for the specified character and visit counts.
	/// Returns a collection of dialogue sequences; the caller picks one randomly.
	///
	/// Priority order:
	/// 1. FirstVisitEverDialogue (totalVisits == 0)
	/// 2. Visit-specific dialogues for this character (matching VisitIndex)
	/// 3. Visit-specific character-agnostic dialogues (matching VisitIndex)
	/// 4. Repeating pool (character-specific + character-agnostic IsRepeating)
	/// </summary>
	/// <param name="characterId">The ID of the character visiting this ancient.</param>
	/// <param name="charVisits">Number of times this character has previously visited this ancient.</param>
	/// <param name="totalVisits">Total number of visits to this ancient across all characters.</param>
	/// <param name="allowAnyCharacterDialogues">
	/// Whether character-agnostic repeating dialogues are allowed.
	/// See <see cref="P:MegaCrit.Sts2.Core.Models.AncientEventModel.AnyCharacterDialogueBlacklist" />.
	/// </param>
	/// <returns>A collection of valid dialogue sequences to choose from.</returns>
	public IEnumerable<AncientDialogue> GetValidDialogues(ModelId characterId, int charVisits, int totalVisits, bool allowAnyCharacterDialogues)
	{
		if (totalVisits == 0 && FirstVisitEverDialogue != null)
		{
			return new global::_003C_003Ez__ReadOnlySingleElementList<AncientDialogue>(FirstVisitEverDialogue);
		}
		IReadOnlyList<AncientDialogue> readOnlyList = null;
		if (CharacterDialogues.TryGetValue(characterId.Entry, out IReadOnlyList<AncientDialogue> value))
		{
			readOnlyList = value;
			List<AncientDialogue> list = readOnlyList.Where((AncientDialogue d) => d.VisitIndex == charVisits).ToList();
			if (list.Count > 0)
			{
				return list;
			}
		}
		if (allowAnyCharacterDialogues)
		{
			List<AncientDialogue> list2 = AgnosticDialogues.Where((AncientDialogue d) => d.VisitIndex == charVisits).ToList();
			if (list2.Count > 0)
			{
				return list2;
			}
		}
		List<AncientDialogue> list3 = new List<AncientDialogue>();
		if (readOnlyList != null)
		{
			AddRepeatingDialogues(readOnlyList, list3, charVisits);
		}
		if (allowAnyCharacterDialogues)
		{
			AddRepeatingDialogues(AgnosticDialogues, list3, charVisits);
		}
		return list3;
	}

	/// <summary>
	/// Auto-derives <see cref="P:MegaCrit.Sts2.Core.Entities.Ancients.AncientDialogueLine.LineText" /> and <see cref="P:MegaCrit.Sts2.Core.Entities.Ancients.AncientDialogueLine.NextButtonText" />
	/// LocStrings for every line based on its position in the dialogue structure.
	/// Keys use X-Y format where X is the dialogue index within a collection and Y is the line index within a dialogue.
	/// </summary>
	public void PopulateLocKeys(string ancientEntry)
	{
		FirstVisitEverDialogue?.PopulateLines(ancientEntry, "firstVisitEver", 0);
		foreach (KeyValuePair<string, IReadOnlyList<AncientDialogue>> characterDialogue in CharacterDialogues)
		{
			characterDialogue.Deconstruct(out var key, out var value);
			string charEntry = key;
			IReadOnlyList<AncientDialogue> readOnlyList = value;
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				readOnlyList[i].PopulateLines(ancientEntry, charEntry, i);
			}
		}
		for (int j = 0; j < AgnosticDialogues.Count; j++)
		{
			AgnosticDialogues[j].PopulateLines(ancientEntry, "ANY", j);
		}
		foreach (AncientDialogue allDialogue in GetAllDialogues())
		{
			for (int k = 0; k < allDialogue.Lines.Count - 1; k++)
			{
				AncientDialogueLine ancientDialogueLine = allDialogue.Lines[k];
				string locEntryKey = ancientDialogueLine.LineText.LocEntryKey;
				string text = locEntryKey.Substring(0, locEntryKey.LastIndexOf('.'));
				string locEntryKey2 = text + ".next";
				ancientDialogueLine.NextButtonText = new LocString("ancients", locEntryKey2);
			}
		}
	}

	private static void AddRepeatingDialogues(IEnumerable<AncientDialogue> source, List<AncientDialogue> destination, int charVisits)
	{
		foreach (AncientDialogue item in source)
		{
			if (item.IsRepeating && (!item.VisitIndex.HasValue || !(charVisits < item.VisitIndex)))
			{
				destination.Add(item);
			}
		}
	}
}
