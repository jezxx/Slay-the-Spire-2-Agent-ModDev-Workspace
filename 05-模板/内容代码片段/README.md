# 原生内容代码片段

这里的片段不是完整模组，也不会自动编译。使用时把片段复制到由原生模组模板创建的项目中，替换命名空间、类名、ID 和本地化，再按当前 API 与游戏内行为验证。

这些片段只展示对象的最小结构，不替代完整的效果逻辑、资源、注册和测试。

## 使用顺序

1. 先用 04-工具/工作流/new-mod.ps1 创建项目。
2. 复制需要的片段，替换 YourMod 和 Example 名称。
3. 在 Entry.Init 中注册需要进入池子的模型。
4. 创建 localization/zhs 下对应的 JSON。
5. 代码、资源和本地化分别走编译、PCK 和运行验证。

## 最小遗物

文件名建议为 Scripts/Relics/ExampleRelic.cs：

    using MegaCrit.Sts2.Core.Entities.Relics;
    using MegaCrit.Sts2.Core.Models;

    namespace YourMod.Scripts.Relics;

    public sealed class ExampleRelic : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Common;
    }

进入共享遗物池的注册示例：

    using MegaCrit.Sts2.Core.Modding;
    using MegaCrit.Sts2.Core.Models.RelicPools;
    using YourMod.Scripts.Relics;

    ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(ExampleRelic));

## 最小 Power

文件名建议为 Scripts/Powers/ExamplePower.cs：

    using MegaCrit.Sts2.Core.Entities.Powers;
    using MegaCrit.Sts2.Core.Models;

    namespace YourMod.Scripts.Powers;

    public sealed class ExamplePower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
    }

Power 通常不进入卡池，而是由卡牌、遗物、药水或补丁通过当前版本的 PowerCmd.Apply 施加。施加前必须查当前签名和目标 Creature/Player 语义。

## 最小药水

文件名建议为 Scripts/Potions/ExamplePotion.cs：

    using System.Threading.Tasks;
    using MegaCrit.Sts2.Core.Entities.Cards;
    using MegaCrit.Sts2.Core.Entities.Creatures;
    using MegaCrit.Sts2.Core.Entities.Potions;
    using MegaCrit.Sts2.Core.GameActions.Multiplayer;
    using MegaCrit.Sts2.Core.Models;

    namespace YourMod.Scripts.Potions;

    public sealed class ExamplePotion : PotionModel
    {
        public override PotionRarity Rarity => PotionRarity.Common;
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override TargetType TargetType => TargetType.Self;

        protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            return Task.CompletedTask;
        }
    }

进入共享药水池的注册示例：

    using MegaCrit.Sts2.Core.Modding;
    using MegaCrit.Sts2.Core.Models.PotionPools;
    using YourMod.Scripts.Potions;

    ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(ExamplePotion));

## 本地化键

原生模型的本地化键使用模型条目名，不把类别前缀写进键：

    {
      "EXAMPLE_RELIC.title": "示例遗物",
      "EXAMPLE_RELIC.description": "这是一个示例遗物。",
      "EXAMPLE_RELIC.flavor": "用于确认本地化路径。",
      "EXAMPLE_POWER.title": "示例能力",
      "EXAMPLE_POWER.description": "这是一个示例能力。",
      "EXAMPLE_POTION.title": "示例药水",
      "EXAMPLE_POTION.description": "这是一瓶示例药水。"
    }

## 为什么没有把事件、人物和 UI 压缩成一个通用片段

事件、人物和 UI 的注册、场景、资源、选项页、战斗恢复、动画和 Hook 覆盖范围都高度依赖当前版本和选择的框架。把某个项目里的几百行实现删成“看起来通用”的几行，反而会让 Agent 误用。

这些方向的正确入口是：

- 先读 01-官方与社区教程/ 中对应专题。
- 再读 02-游戏反编译资料/ 当前版本源码。
- 记录对象、资源、注册、运行时行为和验证证据。
- 完成后再把真正跨项目的部分提炼成新的公共模板或示例。
