using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ModTemplate.Scripts.Cards;

/// <summary>
/// 自定义卡牌示例（零第三方依赖，纯原生 API）。
///
/// 已实测确认的四个硬性规则：
///  1. 类名决定 ID：Slugify(类名) 得到条目名 → <c>EXAMPLE_CARD</c>；
///     类别由继承链决定（父类为 CardModel）→ <c>CARD</c>；完整 ID 是 <c>CARD.EXAMPLE_CARD</c>。
///     本地化键只取**条目**部分：<c>EXAMPLE_CARD.title</c> / <c>EXAMPLE_CARD.description</c>。
///  2. 构造函数必须**公开无参**（ModelDb 用 Activator.CreateInstance 实例化）。
///  3. 必须自己把它加进卡池，否则不会出现在奖励/商店里（见 Entry.Init）。
///  4. 加池必须在卡池 freeze 之前（即模组初始化阶段）。
/// </summary>
public class ExampleCard : CardModel
{
    public ExampleCard()
        : base(
            canonicalEnergyCost: 1,
            type: CardType.Attack,
            rarity: CardRarity.Common,
            targetType: TargetType.AnyEnemy,
            shouldShowInCardLibrary: true)
    {
    }

    /// <summary>基础数值。键名 "Damage" 就是描述里 {Damage:diff()} 的变量名。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move)
    ];

    /// <summary>打出时的效果。</summary>
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    /// <summary>升级效果。</summary>
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
