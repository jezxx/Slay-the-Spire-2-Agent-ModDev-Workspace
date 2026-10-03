using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Misery : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(7m, ValueProp.Move));

	public Misery()
		: base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		Dictionary<PowerModel, int> debuffAmounts = (from p in cardPlay.Target.Powers
			where p.TypeForCurrentAmount == PowerType.Debuff
			select ((PowerModel)p.ClonePreservingMutability(), Amount: p.Amount)).ToDictionary();
		foreach (KeyValuePair<PowerModel, int> item in debuffAmounts)
		{
			PowerModel key = item.Key;
			ITemporaryPower temporaryPower = key as ITemporaryPower;
			if (temporaryPower != null)
			{
				KeyValuePair<PowerModel, int> keyValuePair = debuffAmounts.FirstOrDefault<KeyValuePair<PowerModel, int>>((KeyValuePair<PowerModel, int> p) => p.Key.Id == temporaryPower.InternallyAppliedPower.Id);
				if (keyValuePair.Key != null)
				{
					debuffAmounts[keyValuePair.Key] += item.Value;
				}
			}
		}
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		foreach (Creature enemy in base.CombatState.HittableEnemies)
		{
			if (enemy == cardPlay.Target)
			{
				continue;
			}
			foreach (KeyValuePair<PowerModel, int> item2 in debuffAmounts)
			{
				if (item2.Value != 0)
				{
					PowerModel powerModel = PowerCmd.FindExistingInstanceForStacking(item2.Key, enemy, item2.Key.Applier);
					if (powerModel != null)
					{
						await PowerCmd.ModifyAmount(choiceContext, powerModel, item2.Value, item2.Key.Applier, this);
						continue;
					}
					PowerModel power = (PowerModel)item2.Key.ClonePreservingMutability();
					await PowerCmd.Apply(choiceContext, power, enemy, item2.Value, item2.Key.Applier, this);
				}
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
		AddKeyword(CardKeyword.Retain);
	}
}
