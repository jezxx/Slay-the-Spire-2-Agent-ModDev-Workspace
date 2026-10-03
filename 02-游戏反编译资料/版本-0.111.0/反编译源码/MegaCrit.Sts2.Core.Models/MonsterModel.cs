using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models;

public abstract class MonsterModel : AbstractModel
{
	[CompilerGenerated]
	private sealed class _003CGetAllMoves_003Ed__97 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private MonsterMoveStateMachine machine;

		public MonsterMoveStateMachine _003C_003E3__machine;

		private Dictionary<string, MonsterState>.Enumerator _003C_003E7__wrap1;

		string IEnumerator<string>.Current
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
		public _003CGetAllMoves_003Ed__97(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = System.Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = default(Dictionary<string, MonsterState>.Enumerator);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = machine.States.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					KeyValuePair<string, MonsterState> current = _003C_003E7__wrap1.Current;
					if (current.Value is MoveState)
					{
						_003C_003E2__current = current.Key;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = default(Dictionary<string, MonsterState>.Enumerator);
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

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CGetAllMoves_003Ed__97 _003CGetAllMoves_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == System.Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetAllMoves_003Ed__ = this;
			}
			else
			{
				_003CGetAllMoves_003Ed__ = new _003CGetAllMoves_003Ed__97(0);
			}
			_003CGetAllMoves_003Ed__.machine = _003C_003E3__machine;
			return _003CGetAllMoves_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	private static readonly string _fallbackVisualsPath = SceneHelper.GetScenePath("creature_visuals/fallback");

	public static readonly Vector2 defaultDeathVfxPadding = 1.2f * Vector2.One;

	public const string stunnedMoveId = "STUNNED";

	protected const string _locTableName = "monsters";

	private Rng? _rng;

	private RunRngSet? _runRng;

	private bool _isPerformingMove;

	private Creature? _creature;

	private MonsterMoveStateMachine? _moveStateMachine;

	private bool _spawnedThisTurn;

	private MonsterModel _canonicalInstance;

	public override bool ShouldReceiveCombatHooks => true;

	public virtual LocString Title => L10NMonsterLookup(base.Id.Entry + ".name");

	public abstract int MinInitialHp { get; }

	public abstract int MaxInitialHp { get; }

	public virtual bool IsHealthBarVisible => true;

	/// <summary>
	/// At time of death, a SubViewport is created with the bounds of the creature's spine body in its current pose.
	/// If the creature needs additional padding (usually because the death animation changes its bounds), this property
	/// can be adjusted so that the spawned SubViewport is larger.
	/// </summary>
	public virtual Vector2 ExtraDeathVfxPadding => defaultDeathVfxPadding;

	/// <summary>
	/// For specific creatures that are very close, this value can be made some positive value so that their health bars
	/// do not overlap each other.
	/// </summary>
	public virtual float HpBarSizeReduction => 0f;

	protected virtual string VisualsPath => SceneHelper.GetScenePath("creature_visuals/" + base.Id.Entry.ToLowerInvariant());

	public virtual IEnumerable<string> AssetPaths
	{
		get
		{
			int num = 1;
			List<string> list = new List<string>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<string> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = VisualsPath;
			List<string> list2 = list;
			foreach (AbstractIntent intent in GetIntents())
			{
				list2.AddRange(intent.AssetPaths);
			}
			return list2;
		}
	}

	/// <summary>
	/// This RNG should be used for things that are nice to have synced between players. It should be deterministic as
	/// long as you only use it in deterministic contexts.
	/// It should ONLY be set in <see cref="M:MegaCrit.Sts2.Core.Combat.CombatState.CreateCreature(MegaCrit.Sts2.Core.Models.MonsterModel,MegaCrit.Sts2.Core.Combat.CombatSide,System.String)" />.
	/// </summary>
	public Rng Rng
	{
		get
		{
			if (!base.IsMutable)
			{
				return MegaCrit.Sts2.Core.Random.Rng.Chaotic;
			}
			return _rng;
		}
		set
		{
			AssertMutable();
			_rng = value;
		}
	}

	/// <summary>
	/// Run-scoped RNG set for the run that this monster is in.
	/// </summary>
	public RunRngSet RunRng
	{
		get
		{
			return _runRng;
		}
		set
		{
			AssertMutable();
			if (_runRng != null)
			{
				throw new InvalidOperationException("RunRng has already been set!");
			}
			_runRng = value;
		}
	}

	public bool IsPerformingMove
	{
		get
		{
			return _isPerformingMove;
		}
		private set
		{
			AssertMutable();
			_isPerformingMove = value;
		}
	}

	protected virtual string AttackSfx => $"event:/sfx/enemy/enemy_attacks/{base.Id.Entry.ToLowerInvariant()}/{base.Id.Entry.ToLowerInvariant()}_attack";

	protected virtual string CastSfx => $"event:/sfx/enemy/enemy_attacks/{base.Id.Entry.ToLowerInvariant()}/{base.Id.Entry.ToLowerInvariant()}_cast";

	public virtual string DeathSfx => $"event:/sfx/enemy/enemy_attacks/{base.Id.Entry.ToLowerInvariant()}/{base.Id.Entry.ToLowerInvariant()}_die";

	public virtual bool HasDeathSfx => true;

	public virtual string? HurtSfx => null;

	public virtual bool HasHurtSfx => HurtSfx != null;

	protected virtual bool HasPhobiaSpineSkin => false;

	/// <summary>
	/// Should the default fade animation be played after this monster dies?
	/// Usually true, but false for monsters with certain special death animations (Decimillipede, etc.) or that we want
	/// to keep around after death (Osty, etc.).
	/// </summary>
	public virtual bool ShouldFadeAfterDeath => true;

	public virtual bool ShouldDisappearFromDoom => true;

	public virtual float HurtAnimationTrackOffsetForDoom => 0.1f;

	public virtual bool ShouldShowInCompendium => true;

	/// <summary>
	/// Used for creatures where we can't rely on the length of their death animations to delay the rewards screen.
	/// For example, <see cref="T:MegaCrit.Sts2.Core.Models.Monsters.Crusher" />'s and <see cref="T:MegaCrit.Sts2.Core.Models.Monsters.Rocket" />'s visuals are part of the combat background.
	/// </summary>
	public virtual float DeathAnimLengthOverride => 0f;

	public bool HasDeathAnimLengthOverride => DeathAnimLengthOverride > 0f;

	public virtual bool CanChangeScale => true;

	public virtual DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	public virtual string TakeDamageSfx => "event:/sfx/enemy/enemy_impact_enemy_size/enemy_impact_" + StringHelper.Slugify(TakeDamageSfxType.ToString()).ToLowerInvariant();

	/// <summary>
	/// The creature that represents this monster in combat.
	/// Will technically be null on a canonical monster model, but we should never be checking that, so we leave this as
	/// non-nullable for convenience.
	/// </summary>
	public Creature Creature
	{
		get
		{
			return _creature ?? throw new InvalidOperationException("Creature was accessed before it was set.");
		}
		set
		{
			AssertMutable();
			if (_creature != null)
			{
				throw new InvalidOperationException("Monster " + base.Id.Entry + " already has a creature.");
			}
			_creature = value;
		}
	}

	/// <summary>
	/// The CombatState that this monster's creature exists in.
	/// </summary>
	public ICombatState CombatState => Creature.CombatState ?? NullCombatState.Instance;

	/// <summary>
	/// Get this monster's move state machine.
	/// Null for canonical monster models.
	/// </summary>
	public MonsterMoveStateMachine? MoveStateMachine
	{
		get
		{
			return _moveStateMachine;
		}
		private set
		{
			AssertMutable();
			if (MoveStateMachine != null)
			{
				throw new InvalidOperationException(base.Id.Entry + "'s move state machine has already been set");
			}
			_moveStateMachine = value;
		}
	}

	public MoveState NextMove { get; private set; } = new MoveState();


	public bool IntendsToAttack => NextMove.Intents.Any(delegate(AbstractIntent intent)
	{
		IntentType intentType = intent.IntentType;
		return (intentType == IntentType.Attack || intentType == IntentType.DeathBlow) ? true : false;
	});

	public bool SpawnedThisTurn
	{
		get
		{
			return _spawnedThisTurn;
		}
		private set
		{
			AssertMutable();
			_spawnedThisTurn = value;
		}
	}

	public MonsterModel CanonicalInstance
	{
		get
		{
			if (!base.IsMutable)
			{
				return this;
			}
			return _canonicalInstance;
		}
		private set
		{
			AssertMutable();
			_canonicalInstance = value;
		}
	}

	public NCreatureVisuals CreateVisuals()
	{
		try
		{
			return PreloadManager.Cache.GetScene(VisualsPath).Instantiate<NCreatureVisuals>(PackedScene.GenEditState.Disabled);
		}
		catch (Exception ex)
		{
			Log.Error($"Encountered exception loading the creature visuals for {_creature?.Name}. Falling back to error scene. Exception: {ex}");
			SentryService.CaptureException(ex);
			return CreateFallbackVisuals();
		}
	}

	private NCreatureVisuals CreateFallbackVisuals()
	{
		return PreloadManager.Cache.GetScene(_fallbackVisualsPath).Instantiate<NCreatureVisuals>(PackedScene.GenEditState.Disabled);
	}

	private List<AbstractIntent> GetIntents()
	{
		List<AbstractIntent> list = new List<AbstractIntent>();
		MonsterMoveStateMachine monsterMoveStateMachine = GenerateMoveStateMachine();
		foreach (MonsterState value in monsterMoveStateMachine.States.Values)
		{
			if (value.IsMove && value is MoveState moveState)
			{
				list.AddRange(moveState.Intents);
			}
		}
		return list;
	}

	/// <summary>
	/// Logic to run after the monster is first added to the combat room. No-op by default.
	/// </summary>
	public virtual Task AfterAddedToRoom()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Logic to run after the monster is removed from the combat room. No-op by default.
	/// </summary>
	public virtual void BeforeRemovedFromRoom()
	{
	}

	public virtual List<BestiaryMonsterMove> GenerateBestiaryMoveList(NCreatureVisuals? creatureVisuals)
	{
		List<BestiaryMonsterMove> list = new List<BestiaryMonsterMove>();
		foreach (string allMove in GetAllMoves(MoveStateMachine))
		{
			if (!ShouldShowMoveInBestiary(allMove))
			{
				continue;
			}
			string text = allMove;
			if (text.EndsWith("_MOVE"))
			{
				string text2 = text;
				text = text2.Substring(0, text2.Length - 5);
			}
			LocString bestiaryMoveName = GetBestiaryMoveName(text);
			if (!bestiaryMoveName.Exists())
			{
				if (!text.EndsWith('2') && !text.EndsWith('3') && !text.EndsWith('4'))
				{
					Log.Warn("No loc for move " + allMove + " in monster " + Title.GetFormattedText());
					list.Add(BestiaryMonsterMove.FromState(allMove));
				}
			}
			else
			{
				list.Add(BestiaryMonsterMove.FromState(bestiaryMoveName, allMove));
			}
		}
		MegaSkeletonDataResource megaSkeletonDataResource = creatureVisuals?.SpineBody?.GetSkeleton()?.GetData();
		if (megaSkeletonDataResource != null && megaSkeletonDataResource.HasAnimation("revive"))
		{
			list.Add(BestiaryMonsterMove.FromAnim("revive", null));
		}
		if (megaSkeletonDataResource != null && megaSkeletonDataResource.HasAnimation("hurt"))
		{
			list.Add(BestiaryMonsterMove.FromAnim("hurt", TakeDamageSfx).StopOtherSfx());
		}
		if (megaSkeletonDataResource != null && megaSkeletonDataResource.HasAnimation("die"))
		{
			list.Add(BestiaryMonsterMove.FromAnim("die", DeathSfx).StopOtherSfx());
		}
		return list;
	}

	protected virtual bool ShouldShowMoveInBestiary(string moveStateId)
	{
		return true;
	}

	[IteratorStateMachine(typeof(_003CGetAllMoves_003Ed__97))]
	private IEnumerable<string> GetAllMoves(MonsterMoveStateMachine machine)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAllMoves_003Ed__97(-2)
		{
			_003C_003E3__machine = machine
		};
	}

	/// <summary>
	/// Destroys the monster's move state machine.
	/// </summary>
	public void ResetStateMachine()
	{
		_moveStateMachine = null;
	}

	public static LocString L10NMonsterLookup(string entryName)
	{
		return new LocString("monsters", entryName);
	}

	public MonsterModel ToMutable()
	{
		AssertCanonical();
		MonsterModel monsterModel = (MonsterModel)MutableClone();
		monsterModel.CanonicalInstance = this;
		return monsterModel;
	}

	protected abstract MonsterMoveStateMachine GenerateMoveStateMachine();

	public void SetUpForCombat()
	{
		MoveStateMachine = GenerateMoveStateMachine();
		SpawnedThisTurn = true;
	}

	public void RollMove(IEnumerable<Creature> targets)
	{
		NextMove = MoveStateMachine.RollMove(targets, Creature, RunRng.MonsterAi);
	}

	public void SetMoveImmediate(MoveState state, bool forceTransition = false)
	{
		if (NextMove.CanTransitionAway || forceTransition)
		{
			NextMove = state;
			MoveStateMachine.ForceCurrentState(state);
			NCreature creatureNode = Creature.GetCreatureNode();
			if (creatureNode != null && CombatState.IsLiveCombat())
			{
				TaskHelper.RunSafely(creatureNode.RefreshIntents());
			}
		}
	}

	public async Task PerformMove()
	{
		if (CombatState != null)
		{
			ICombatState combatState = CombatState;
			await Cmd.CustomScaledWait(0.1f, 0.2f);
			IsPerformingMove = true;
			MoveState move = NextMove;
			IReadOnlyList<Creature> targets = combatState.PlayerCreatures;
			if (TestMode.IsOff)
			{
				Log.Info("Monster " + base.Id.Entry + " performing move " + move.Id);
			}
			await move.PerformMove(targets);
			MoveStateMachine?.OnMovePerformed(move);
			CombatManager.Instance.History.MonsterPerformedMove(combatState, this, move, targets);
			IsPerformingMove = false;
			if (Creature.IsDead && Hook.ShouldCreatureBeRemovedFromCombatAfterDeath(combatState, Creature))
			{
				combatState.RemoveCreature(Creature);
			}
			await Cmd.CustomScaledWait(0.1f, 0.4f);
		}
	}

	public virtual void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
	{
	}

	public virtual CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		return creatureAnimator;
	}

	public void OnSideSwitch()
	{
		AssertMutable();
		SpawnedThisTurn = false;
	}

	/// <summary>
	/// Called when this monster is about to die to Doom.
	/// Primarily used set up the creature visuals for the Doom vfx (ie hiding nodes that have additive materials)
	/// </summary>
	public virtual void OnDieToDoom()
	{
	}

	/// <summary>
	/// Helper to get the move name label for the Bestiary entry.
	/// </summary>
	protected LocString GetBestiaryMoveName(string moveId)
	{
		return new LocString("monsters", base.Id.Entry + ".moves." + moveId + ".title");
	}

	public void OnPhobiaModeToggled(bool isOn, MegaSprite spine, MegaSkeleton skeleton)
	{
		if (HasPhobiaSpineSkin)
		{
			MegaSkin megaSkin = spine.NewSkin("custom-skin");
			MegaSkeletonDataResource data = skeleton.GetData();
			megaSkin.AddSkin(data.FindSkin(isOn ? "phobia" : "normal"));
			skeleton.SetSkin(megaSkin);
			skeleton.SetSlotsToSetupPose();
		}
	}
}
