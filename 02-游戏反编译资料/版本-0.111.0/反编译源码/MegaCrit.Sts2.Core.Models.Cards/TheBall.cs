using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class TheBall : CardModel
{
	private const string _increaseKey = "Increase";

	private decimal _extraDamageFromPlays;

	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	/// <summary>
	/// Required so we can restore the extra damage amount after a downgrade (ie Magiknight)
	/// </summary>
	private decimal ExtraDamageFromPlays
	{
		get
		{
			return _extraDamageFromPlays;
		}
		set
		{
			AssertMutable();
			_extraDamageFromPlays = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(10m, ValueProp.Move),
		new DynamicVar("Increase", 10m)
	});

	public TheBall()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		base.DynamicVars.Damage.BaseValue += base.DynamicVars["Increase"].BaseValue;
		ExtraDamageFromPlays += base.DynamicVars["Increase"].BaseValue;
	}

	protected override CardLocation GetResultLocationForCardPlay()
	{
		CardLocation resultLocationForCardPlay = base.GetResultLocationForCardPlay();
		if (base.CombatState == null)
		{
			return resultLocationForCardPlay;
		}
		List<Creature> list = (from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
			where c != null && c.IsAlive && c.IsPlayer && c.Player != base.Owner
			select c).ToList();
		if (list.Count == 0)
		{
			return resultLocationForCardPlay;
		}
		resultLocationForCardPlay.player = base.Owner.RunState.Rng.CombatTargets.NextItem(list).Player;
		if (resultLocationForCardPlay.pileType == PileType.Discard)
		{
			resultLocationForCardPlay.pileType = PileType.Draw;
			resultLocationForCardPlay.position = CardPilePosition.Random;
		}
		return resultLocationForCardPlay;
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		base.DynamicVars.Damage.BaseValue += ExtraDamageFromPlays;
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Increase"].UpgradeValueBy(5m);
	}
}
