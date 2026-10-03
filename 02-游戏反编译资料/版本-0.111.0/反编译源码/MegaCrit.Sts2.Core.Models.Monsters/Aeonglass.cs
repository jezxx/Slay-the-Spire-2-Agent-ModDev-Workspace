using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Aeonglass : MonsterModel
{
	private const string _attackHeavyTrigger = "AttackHeavy";

	private const string _attackDoubleTrigger = "AttackDouble";

	private const string _aeonglassTrackName = "queen_progress";

	private int _additionalStrength;

	private int _witherUpgradeCount;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 535, 512);

	public override int MaxInitialHp => MinInitialHp;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

	private int EbbDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 22);

	private int EbbBlock => 33;

	private int EyeLasersDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);

	private int EyeLasersRepeat => 2;

	private int IncreasingIntensityBaseStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private int WitherAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);

	private int AdditionalStrength
	{
		get
		{
			return _additionalStrength;
		}
		set
		{
			AssertMutable();
			_additionalStrength = value;
		}
	}

	public int WitherUpgradeCount
	{
		get
		{
			return _witherUpgradeCount;
		}
		set
		{
			AssertMutable();
			_witherUpgradeCount = value;
		}
	}

	private int IncreasingIntensityTotalStrength => IncreasingIntensityBaseStrength + AdditionalStrength;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		NRunMusicController.Instance?.UpdateMusicParameter("queen_progress", 1f);
		foreach (Creature item in base.CombatState.GetOpponentsOf(base.Creature))
		{
			WitheringPresencePower witheringPresencePower = (WitheringPresencePower)ModelDb.Power<WitheringPresencePower>().ToMutable();
			witheringPresencePower.Target = item;
			await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), witheringPresencePower, base.Creature, 6m, base.Creature, null);
		}
		await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), base.Creature, 3m, base.Creature, null);
	}

	public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature != base.Creature)
		{
			return Task.CompletedTask;
		}
		NRunMusicController.Instance?.UpdateMusicParameter("queen_progress", 5f);
		return Task.CompletedTask;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("EBB_MOVE", EbbMove, new SingleAttackIntent(EbbDamage), new DefendIntent());
		MoveState moveState2 = new MoveState("EYE_LASERS_MOVE", EyeLasersMove, new MultiAttackIntent(EyeLasersDamage, EyeLasersRepeat));
		MoveState moveState3 = new MoveState("INCREASING_INTENSITY_MOVE", IncreasingIntensityMove, new StatusIntent(WitherAmount), new BuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task EbbMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(EbbDamage).FromMonster(this).WithAttackerAnim("AttackHeavy", 0.3f)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await CreatureCmd.GainBlock(base.Creature, EbbBlock, ValueProp.Move, null);
	}

	private async Task EyeLasersMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(EyeLasersDamage).FromMonster(this).WithHitCount(EyeLasersRepeat)
			.WithAttackerAnim("AttackDouble", 0.4f)
			.OnlyPlayAnimOnce()
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task IncreasingIntensityMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.4f);
		foreach (Creature target in targets)
		{
			if (target.Player?.PlayerCombatState == null)
			{
				continue;
			}
			foreach (CardModel allCard in target.Player.PlayerCombatState.AllCards)
			{
				if (allCard is Wither wither)
				{
					wither.FakeUpgrade();
				}
			}
		}
		WitherUpgradeCount++;
		await CardPileCmd.AddToCombatAndPreview<Wither>(targets, PileType.Discard, WitherAmount, null);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Creature, IncreasingIntensityTotalStrength, base.Creature, null);
		AdditionalStrength++;
	}

	public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
	{
		if (!(card is Wither wither))
		{
			return Task.CompletedTask;
		}
		MatchWitherToUpgradeCount(wither);
		return Task.CompletedTask;
	}

	public void MatchWitherToUpgradeCount(Wither wither)
	{
		for (int i = 0; i < WitherUpgradeCount; i++)
		{
			wither.FakeUpgrade();
		}
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("wither");
		AnimState animState3 = new AnimState("attack_heavy");
		AnimState animState4 = new AnimState("attack_double");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("AttackHeavy", animState3);
		creatureAnimator.AddAnyState("AttackDouble", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		return creatureAnimator;
	}
}
