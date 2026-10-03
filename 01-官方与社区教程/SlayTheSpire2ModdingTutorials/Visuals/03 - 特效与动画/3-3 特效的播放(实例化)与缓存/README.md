## 特效的实例化与播放

特效建好之后，我们现在需要解决如何将其播放的问题。

注意到游戏本体使用了`AttackCommand`的`WithHitFX()`等方法来播放特效，我们不建议使用，因为这个方法只能播放游戏本体路径下的特效，而我们的特效是存放在mod子目录下，除非你Patch游戏本体，或者使用前置插件等。

同时游戏本体的场景缓存策略会清除其他场景，所以我们最好是内置自己的方法。

### 使用工具类实例化

推荐创建一个 `VFXUtil` 工具类，提供统一的实例化入口，优先从 Mod 的独立缓存加载：

```csharp
public static class VFXUtil {
    // Mod 独立的场景缓存（避免被 PreloadManager 清理）
    public static readonly ConcurrentDictionary<string, PackedScene> ModSceneCache = new();

    public static Node2D GenVFXNode(string scenePath) {
        if (ModSceneCache.TryGetValue(scenePath, out var modScene)) {
            return modScene.Instantiate<Node2D>();
        }
        return PreloadManager.Cache.GetScene(scenePath).Instantiate<Node2D>();
    }

    public static T GenVFXNode<T>(string scenePath) where T : Node2D {
        if (ModSceneCache.TryGetValue(scenePath, out var modScene)) {
            return modScene.Instantiate<T>();
        }
        return PreloadManager.Cache.GetScene(scenePath).Instantiate<T>();
    }
    
    public static Node2D? PlaySimple(string scenePath, Vector2 position, float lifetime = 2f) {
        if (!TestMode.IsOn && NCombatRoom.Instance != null) {
            Node2D node2D = GenVFXNode(scenePath);
            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(node2D);
            node2D.GlobalPosition = position;
            
            // 创建定时器，超时后销毁
            SceneTreeTimer timer = node2D.GetTree().CreateTimer(lifetime);
            timer.Timeout += () => {
                if (GodotObject.IsInstanceValid(node2D)) {
                    node2D.QueueFreeSafely();
                }
            };
            return node2D;
        }
        return null;
    }
}
```

VFXUtil.PlaySimple()方法简单实例化一个节点，并在给定的时间（秒）后自动销毁，省去了写销毁脚本的步骤。

```csharp
// 使用方式
VFXUtil.PlaySimple("res://YourMod/scenes/vfx/glow.tscn", position, 2f);
```
**复杂使用示例**（工厂方法）：
你的特效可能挂载了自定义脚本，可以使用GenVFXNode<T>()方法来更精确的实例化，控制位置等。

以下是万象辉星Mod中创世之柱动画的创建方法。(需要放到角色背后，并且不销毁)

```csharp
public static Pillar? Spawn(Creature creature, Vector2 position) {
    if (TestMode.IsOn) return null;

    var pillar = VFXUtil.GenVFXNode<Pillar>("res://YourMod/scenes/vfx/pillar.tscn");
    Node? parent = NCombatRoom.Instance?.BackCombatVfxContainer;
    if (parent == null) {
        Logger.Warn("[Pillar] No BackCombatVfxContainer available");
        pillar.QueueFree();
        return null;
    }
    
    parent.AddChildSafely(pillar);
    pillar.Create(position); // 触发动画逻辑
    return pillar;
}
```


### 场景缓存与自动销毁

如果不建立缓存，会遇到一个问题，所有特效第一次播放时会卡顿以下。

#### 为什么需要 Mod 独立缓存？

STS2 的 `PreloadManager`的缓存逻辑 在场景切换时会 `UnloadAssets()`，只保留了本体的特效，导致 Mod 的特效场景被意外清理。

我们可以通过HarmonyPatch解决这些问题，但比较复杂，本文暂不讨论，而是简单的创建独立的缓存：

```csharp
public static readonly ConcurrentDictionary<string, PackedScene> ModSceneCache = new();
```

#### 预加载场景

在 Mod 初始化时(Entry.cs中)预先加载所有 VFX 场景：

```csharp
static void LoadScenes() {
    //你的场景字符串列表
    var paths = new List<string> {
        "res://.../my_effect_1.tscn",
        "res://.../my_effect_2.tscn",
        "res://.../my_effect_3.tscn",
        "res://.../my_effect_4.tscn",
    };
    foreach (var path in paths) {
        if (ModSceneCache.ContainsKey(path)) continue;
        var scene = ResourceLoader.Load<PackedScene>(path, null, ResourceLoader.CacheMode.Reuse);
        if (scene != null) {
            ModSceneCache[path] = scene;
        }
    }
}
```

### 特效位置参考
考虑到特效的位置，需要了解游戏中NCombatRoom的结构


#### 场景结构

```
NCombatRoom (Control)
├── %CombatUi                    // UI 层
├── %CombatSceneContainer        // 场景容器
│   ├── %AllyContainer           // 盟友容器（左侧）
│   │   └── NCreature (Player)   // 玩家角色节点
│   │       ├── Body             // 身体/视觉节点
│       ├── Visuals              // 视觉容器
│       ├── Hitbox               // 点击区域
│       ├── IntentContainer      // 意图显示
│       └── OrbManager           // 球体管理器
│   ├── %EnemyContainer          // 敌人容器（右侧）
│   │   └── NCreature (Enemy)    // 敌人角色节点
│   └── EncounterSlots           // 遭遇场景槽位（如果有）
├── %BgContainer                 // 背景容器 (ZIndex = -20)
├── %BackCombatVfxContainer      // 后台特效容器
├── %CombatVfxContainer          // 前台特效容器 (ZIndex = -9)
└── RadialBlur                   // 径向模糊效果
```

**选择原则**：

- **前台特效**（`CombatVfxContainer`）：短暂存在的攻击特效、命中效果、粒子爆发。需要覆盖在角色上方。
- **后台特效**（`BackCombatVfxContainer`）：持续存在的状态特效、背景元素。需要显示在角色后方。

如果特效之间也有覆盖关系，可以捅过代码修改ZIndex来调整。

```csharp
// 前台特效 - 飞刀
Node? parent = NCombatRoom.Instance?.CombatVfxContainer;
parent.AddChildSafely(blade);

// 后台特效 - 黑洞
Node? backVfx = NCombatRoom.Instance?.BackCombatVfxContainer;
backVfx.AddChildSafely(blackhole);
```

#### 获取角色位置

```csharp
// 获取角色节点
NCreature? ownerNode = NCombatRoom.Instance?.GetCreatureNode(owner);

// 获取特效生成位置(生物的中心)
Vector2 spawnPos = ownerNode.VfxSpawnPosition;

// 获取角色全局位置(一般在生物的脚底)
Vector2 globalPos = ownerNode.GlobalPosition;

```

#### 获取角色朝向

在对战大螃蟹boss的时候，角色可能会朝向左边，如果特效播放的位置是角色前方/后方的话，需要动态获取角色朝向

这里给VFXUtil写了个工具方法判断
```csharp
//VFXUtil.cs
public static bool IsCharacterFacingRight(Creature creature) {
    Node2D? body = NCombatRoom.Instance?.GetCreatureNode(creature)?.Body;
    if (body == null) return true;
    return body.Scale.X > 0;
}

// 获取角色朝向（用于镜像翻转）
bool facingRight = VFXUtil.IsCharacterFacingRight(creature);
int xFac = facingRight ? 1 : -1;
//在角色前方位置播放特效
Vector2 position = Creature.VfxSpawnPosition + new Vector2(100f * xFac, 0f);
//播放特效...
```