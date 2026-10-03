using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class ImitationLearning : CardModel
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<ImitationLearningPower>(2m));

	public ImitationLearning()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardPlay cardPlay2 = cardPlay;
		ArgumentNullException.ThrowIfNull(cardPlay2.Target, "cardPlay.Target");
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
		ImitationLearningPower imitationLearningPower = base.Owner.Creature.Powers.OfType<ImitationLearningPower>().FirstOrDefault((ImitationLearningPower s) => s.PlayerTarget == cardPlay2.Target.Player);
		decimal baseValue = base.DynamicVars["ImitationLearningPower"].BaseValue;
		if (imitationLearningPower != null)
		{
			await PowerCmd.ModifyAmount(choiceContext, imitationLearningPower, baseValue, base.Owner.Creature, this);
			return;
		}
		imitationLearningPower = await PowerCmd.Apply<ImitationLearningPower>(choiceContext, base.Owner.Creature, baseValue, base.Owner.Creature, this);
		if (imitationLearningPower != null)
		{
			imitationLearningPower.PlayerTarget = cardPlay2.Target.Player;
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["ImitationLearningPower"].UpgradeValueBy(1m);
	}
}
