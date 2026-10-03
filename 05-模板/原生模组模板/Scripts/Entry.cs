using System.Reflection;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using ModTemplate.Scripts.Cards;

namespace ModTemplate.Scripts;

/// <summary>
/// 模组入口。游戏会扫描程序集里带 [ModInitializer] 特性的类型，
/// 并调用该特性参数中指定名字的静态初始化方法。
/// 特性里的字符串必须与初始化方法名一致（这里用 nameof 保证一致）。
///
/// 重要时序（实测）：本方法运行在 ModelDb.Init 之前，
/// 此刻 ModelDb 是空的（GetAll 数量为 0，连原版 CARD.BASH 都查不到）。
/// 所以这里只做"挂号"（加卡池），要检查结果请挂 Harmony 到 ModelDb.InitIds 之后。
/// </summary>
[ModInitializer(nameof(Init))]
public static class Entry
{
    /// <summary>模组 ID，必须与 {ModId}.json 里的 id 完全一致。</summary>
    public const string ModId = "ModTemplate";

    /// <summary>Harmony 实例，供打补丁使用。</summary>
    public static readonly Harmony Harmony = new($"sts2.mod.{ModId}");

    /// <summary>初始化入口。会在游戏启动、模组被加载时调用一次。</summary>
    public static void Init()
    {
        try
        {
            // 1) 打 Harmony 补丁：Patch 类都写在本程序集里，PatchAll 会自动收集
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            // 2) 让 .tscn 场景能加载本程序集里的 Godot 脚本（纯 C# 模组也建议保留）
            ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);

            // 3) 注册自定义内容
            //    卡牌不会自己进卡池，必须显式挂号，否则抽不到/买不到。
            //    注意必须在卡池 freeze 之前调用，否则抛
            //    "You must add content before the game is initialized."
            ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(ExampleCard));

            var id = ModelDb.GetId(typeof(ExampleCard));
            Log.Info($"[{ModId}] 初始化完成；ExampleCard 预期 ID = {id}");
        }
        catch (Exception ex)
        {
            // 初始化失败不该拖垮整个游戏，但一定要把原因写进日志方便排查
            Log.Error($"[{ModId}] 初始化失败: {ex}");
        }
    }
}
