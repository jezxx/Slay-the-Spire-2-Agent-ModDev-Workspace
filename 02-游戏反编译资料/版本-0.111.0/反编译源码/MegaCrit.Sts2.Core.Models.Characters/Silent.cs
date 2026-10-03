using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Characters;

public sealed class Silent : CharacterModel
{
	public const string shivTrigger = "Shiv";

	public const string energyColorName = "silent";

	public override Color NameColor => StsColors.green;

	public override CharacterGender Gender => CharacterGender.Feminine;

	/// <remarks>
	/// You'll always unlock Silent by doing a run with Ironclad since he's the only character you start out with, but
	/// technically you unlock her by doing a run with any character.
	/// </remarks>
	protected override CharacterModel? UnlocksAfterRunAs => null;

	public override int StartingHp => 70;

	public override int StartingGold => 99;

	public override CardPoolModel CardPool => ModelDb.CardPool<SilentCardPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<SilentRelicPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<SilentPotionPool>();

	public override IEnumerable<CardModel> StartingDeck => new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[12]
	{
		ModelDb.Card<StrikeSilent>(),
		ModelDb.Card<StrikeSilent>(),
		ModelDb.Card<StrikeSilent>(),
		ModelDb.Card<StrikeSilent>(),
		ModelDb.Card<StrikeSilent>(),
		ModelDb.Card<DefendSilent>(),
		ModelDb.Card<DefendSilent>(),
		ModelDb.Card<DefendSilent>(),
		ModelDb.Card<DefendSilent>(),
		ModelDb.Card<DefendSilent>(),
		ModelDb.Card<Neutralize>(),
		ModelDb.Card<Survivor>()
	});

	public override IReadOnlyList<RelicModel> StartingRelics => new global::_003C_003Ez__ReadOnlySingleElementList<RelicModel>(ModelDb.Relic<RingOfTheSnake>());

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.25f;

	public override Color EnergyLabelOutlineColor => new Color("004f04FF");

	public override Color DialogueColor => new Color("284719");

	public override VfxColor SpeechBubbleColor => VfxColor.Swamp;

	public override Color MapDrawingColor => new Color("2F6729");

	public override Color RemoteTargetingLineColor => new Color("2EBD5EFF");

	public override Color RemoteTargetingLineOutline => new Color("004f04FF");

	protected override List<(AnimState, string)> AnimationStates => base.AnimationStates.Concat<(AnimState, string)>(new global::_003C_003Ez__ReadOnlySingleElementList<(AnimState, string)>((new AnimState("shiv"), "Shiv"))).ToList();

	public override List<string> GetArchitectAttackVfx()
	{
		int num = 4;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "vfx/vfx_dagger_spray";
		num2++;
		span[num2] = "vfx/vfx_flying_slash";
		num2++;
		span[num2] = "vfx/vfx_dramatic_stab";
		num2++;
		span[num2] = "vfx/vfx_dagger_throw";
		return list;
	}
}
