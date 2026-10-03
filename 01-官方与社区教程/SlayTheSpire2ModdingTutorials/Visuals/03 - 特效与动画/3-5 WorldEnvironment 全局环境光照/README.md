## World Environment 全局环境节点

根据Godot官方文档

> WorldEnvironment节点控制整个场景的默认环境属性，后期处理效果、照明和背景设置。
> 
> **WorldEnvironment** 节点用于为场景配置默认的 [Environment](https://docs.godotengine.org/zh-cn/4.x/classes/class_environment.html#class-environment)。
> 
> **WorldEnvironment** 中定义的参数可以被设置为当前的 [Camera3D](https://docs.godotengine.org/zh-cn/4.x/classes/class_camera3d.html#class-camera3d) 上所设置的 [Environment](https://docs.godotengine.org/zh-cn/4.x/classes/class_environment.html#class-environment) 资源覆盖。此外，在一个给定场景中，同一时间只能实例化一个 **WorldEnvironment**。
> 
> **WorldEnvironment** 允许用户指定默认的照明参数（例如环境照明）、各种后处理效果（例如 SSAO、DOF、色调映射）、以及如何绘制背景（例如纯色、天空盒）。通常，添加这些是为了提高场景的真实感/色彩平衡。

在`NGame.Instance.ActivateWorldEnvironment()`我们可以获取游戏本体中的WorldEnvironment节点

调用它就可以在适当的时候，修改整个游戏的亮度、曝光、对比度等属性。

比如说你需要制作一个核爆特效，那么在爆炸时可以将其`TonemapExposure`调高，可以使用Tween来补间淡入插值。

请注意，过度曝光可能会影响体验，产生光污染，光敏癫痫风险，请慎用。

以下是一个简易的Util。


```csharp
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Nodes;

/// <summary>
/// 全屏特效工具类，基于 NGame 中的 WorldEnvironment 节点实现。
/// 支持发光（Glow/Bloom）、饱和度、亮度、对比度、曝光等后处理效果的动态调整。
/// </summary>
public static class WorldEnvironmentUtil
{
    private static WorldEnvironment? _cachedEnv;

    public const bool ENABLE_EXPOSURE = true;

    /// <summary>
    /// 获取当前激活的 WorldEnvironment 节点。
    /// 如果没有激活，会自动调用 NGame.Instance.ActivateWorldEnvironment()。
    /// </summary>
    public static WorldEnvironment? GetOrActivateEnvironment()
    {
        if (_cachedEnv != null && GodotObject.IsInstanceValid(_cachedEnv))
        {
            return _cachedEnv;
        }

        if (NGame.Instance == null)
        {
            return null;
        }

        _cachedEnv = NGame.Instance.ActivateWorldEnvironment();
        return _cachedEnv;
    }

    /// <summary>
    /// 关闭 WorldEnvironment 特效节点。
    /// </summary>
    public static void DeactivateEnvironment()
    {
        if (NGame.Instance == null)
        {
            Entry.Logger.Warn("NGame.Instance is null, cannot deactivate WorldEnvironment.");
            return;
        }

        NGame.Instance.DeactivateWorldEnvironment();
        _cachedEnv = null;
    }

    /// <summary>
    /// 设置发光（Bloom）强度。需要 Environment 开启 Glow 才能看到效果。
    /// </summary>
    /// <param name="intensity">发光强度，默认 0.8，范围建议 0~3</param>
    public static void SetGlowIntensity(float intensity)
    {
        var env = GetOrActivateEnvironment();
        if (env == null) return;

        env.Environment.GlowIntensity = intensity;
    }

    /// <summary>
    /// 设置曝光（Tonemap Exposure）。
    /// </summary>
    public static void SetExposure(float exposure)
    {
        var env = GetOrActivateEnvironment();
        if (env == null) return;

        env.Environment.TonemapExposure = exposure;
    }

    /// <summary>
    /// 设置亮度调整（Adjustment Brightness）。
    /// </summary>
    public static void SetBrightness(float brightness)
    {
        var env = GetOrActivateEnvironment();
        if (env == null) return;

        env.Environment.AdjustmentBrightness = brightness;
    }

    /// <summary>
    /// 设置对比度调整（Adjustment Contrast）。
    /// </summary>
    public static void SetContrast(float contrast)
    {
        var env = GetOrActivateEnvironment();
        if (env == null) return;

        env.Environment.AdjustmentContrast = contrast;
    }


    /// <summary>
    /// 设置饱和度调整（Adjustment Saturation）。
    /// </summary>
    public static void SetSaturation(float saturation)
    {
        var env = GetOrActivateEnvironment();
        if (env == null) return;

        env.Environment.AdjustmentSaturation = saturation;
    }
    
    /// <summary>
    /// 重置所有调整参数为默认值（曝光1，亮度1，对比度1，饱和度1，发光强度0.8）。
    /// 不会自动 DeactivateEnvironment，如需关闭请手动调用。
    /// </summary>
    public static void ResetToDefaults()
    {
        var env = GetOrActivateEnvironment();
        if (env == null) return;

        env.Environment.TonemapExposure = 1f;
        env.Environment.AdjustmentBrightness = 1f;
        env.Environment.AdjustmentContrast = 1f;
        env.Environment.AdjustmentSaturation = 1f;
        env.Environment.GlowIntensity = 0.8f;
    }
}

```