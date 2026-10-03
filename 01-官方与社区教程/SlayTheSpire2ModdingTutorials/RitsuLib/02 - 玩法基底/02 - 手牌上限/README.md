## 实现功能

基础库提供了一个修改手牌上限的接口，只要接上`IMaxHandSizeModifier`并实现就行。

在你有修改手牌上限能力的`AbstractModel`类中添加接口并实现，例如给`PowerModel`：

（对于卡牌，如果抽牌导致有更改手牌上限的卡牌入手，那次抽牌的结果不会变化。请自行实现。）

```csharp
[RegisterPower]
public class TestPower : ModPowerTemplate, IMaxHandSizeModifier // 添加该接口
{
    // 其余省略


    // 实现该方法或者实现ModifyMaxHandSizeLate。Late比这个后执行。
    public int ModifyMaxHandSize(Player player, int currentMaxHandSize)
    {
        // 健康的实现：需要判断是否是当前玩家
        if (player != Owner.Player)
            return currentMaxHandSize;
        // 手牌上限+2
        return currentMaxHandSize + 2;
    }
}
```

如果你想获取一个玩家的手牌上限，请使用`RitsuLibFramework.GetMaxHandSize(player)`而不是`10`。

## 说明

* 返回的值是修改后的手牌上限。如果你想设置成一个固定值建议用`ModifyMaxHandSizeLate`。注意Hook的顺序（例如每日特效和单例会在最后触发，查看`IterateHookListeners`了解）

* 不会少于0，最后会兜底。