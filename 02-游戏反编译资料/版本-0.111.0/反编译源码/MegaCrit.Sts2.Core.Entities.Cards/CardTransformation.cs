using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// Represents a transformation of one card in a pile to another card which replaces the original card in the same pile.
/// If Replacement is set, then it will be used as a direct replacement.
/// If ReplacementOptions is set, then a random card will be chosen from one of those options.
/// </summary>
public readonly struct CardTransformation
{
	[CompilerGenerated]
	private sealed class _003CYield_003Ed__16 : IEnumerable<CardTransformation>, IEnumerable, IEnumerator<CardTransformation>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private CardTransformation _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public CardTransformation _003C_003E4__this;

		public CardTransformation _003C_003E3___003C_003E4__this;

		CardTransformation IEnumerator<CardTransformation>.Current
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
		public _003CYield_003Ed__16(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E4__this;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<CardTransformation> IEnumerable<CardTransformation>.GetEnumerator()
		{
			_003CYield_003Ed__16 _003CYield_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CYield_003Ed__ = this;
			}
			else
			{
				_003CYield_003Ed__ = new _003CYield_003Ed__16(0);
			}
			_003CYield_003Ed__._003C_003E4__this = _003C_003E3___003C_003E4__this;
			return _003CYield_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<CardTransformation>)this).GetEnumerator();
		}
	}

	public CardModel Original { get; }

	public CardModel? Replacement { get; }

	public IEnumerable<CardModel>? ReplacementOptions { get; }

	/// <summary>
	/// Whether the original card is in combat at construction time. Captured here because the card
	/// may be removed from its pile before GetReplacement is called, which would make CombatState null.
	/// </summary>
	public bool IsInCombat { get; }

	public CardTransformation(CardModel original)
	{
		AssertTransformable(original);
		Original = original;
		ReplacementOptions = null;
		Replacement = null;
		IsInCombat = original.CombatState != null;
	}

	public CardTransformation(CardModel original, IEnumerable<CardModel> options)
	{
		AssertTransformable(original);
		Original = original;
		ReplacementOptions = options;
		Replacement = null;
		IsInCombat = original.CombatState != null;
	}

	public CardTransformation(CardModel original, CardModel replacement)
	{
		AssertTransformable(original);
		Original = original;
		Replacement = replacement;
		ReplacementOptions = null;
		IsInCombat = original.CombatState != null;
	}

	public CardModel? GetReplacement(Rng? rng)
	{
		if (Replacement != null)
		{
			return Replacement;
		}
		if (rng == null)
		{
			throw new ArgumentException("RNG must be passed when replacement options is set!");
		}
		if (!Original.IsTransformable)
		{
			return null;
		}
		if (ReplacementOptions == null)
		{
			return CardFactory.CreateRandomCardForTransform(Original, IsInCombat, rng);
		}
		return CardFactory.CreateRandomCardForTransform(Original, ReplacementOptions, IsInCombat, rng);
	}

	[IteratorStateMachine(typeof(_003CYield_003Ed__16))]
	public IEnumerable<CardTransformation> Yield()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CYield_003Ed__16(-2)
		{
			_003C_003E3___003C_003E4__this = this
		};
	}

	private static void AssertTransformable(CardModel card)
	{
		if (!card.IsTransformable)
		{
			throw new InvalidOperationException("Non-removable cards cannot be transformed!");
		}
	}
}
