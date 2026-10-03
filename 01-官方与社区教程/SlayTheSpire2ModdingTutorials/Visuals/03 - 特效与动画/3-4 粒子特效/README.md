
## 粒子特效 (GPUParticles2D)

`GPUParticles2D` 是 Godot 4 的高性能粒子系统，配合 `ParticleProcessMaterial` 可实现爆炸、烟雾、火花等效果。

你可以手动绘画一些简单的圆形、方形、星形png透明图片，然后将其提供给ai，并让其设计简单的粒子特效。

在设计特效流程中，如果你不知道该提供什么，可以让AI问你。举例来说，你让AI用粒子系统设计一个下雨特效，AI就会找你要雨点的素材。
### 基础粒子场景结构

以烟雾消散效果为例：

```tscn
[node name="vfx_poof" type="GPUParticles2D"]
rotation = 3.14159
amount = 1
texture = ExtResource("2_62oo3")          ; 粒子纹理
lifetime = 0.75
one_shot = true                           ; 只播放一次
fixed_fps = 60
local_coords = true
process_material = SubResource("ParticleProcessMaterial_o3l8p")
```

**关键属性**：

| 属性 | 作用 | 典型值 |
|------|------|--------|
| `one_shot` | 播放一次后停止 | `true` |
| `amount` | 粒子数量 | 1 ~ 100 |
| `lifetime` | 粒子存活时间 | 0.1 ~ 3.0 |
| `emitting` | 是否正在发射 | 手动触发时设为 `false`，播放时设为 `true` |
| `explosiveness` | 爆发度 (0~1) | `1.0` = 瞬间全部发射 |
| `fixed_fps` | 固定帧率 | 60 |

#### ParticleProcessMaterial 配置

```tscn
[sub_resource type="ParticleProcessMaterial" id="ParticleProcessMaterial_o3l8p"]
particle_flag_align_y = true
particle_flag_disable_z = true
direction = Vector3(0, 1, 0)              ; 向上发射
spread = 0.0                              ; 无扩散
initial_velocity_min = 500.0
initial_velocity_max = 500.0
gravity = Vector3(0, 0, 0)                ; 无重力
damping_min = 0.5                         ; 阻力减速
scale_min = 0.75
scale_max = 0.75
scale_curve = SubResource("CurveXYZTexture_a1fhn")  ; 缩放变化曲线
alpha_curve = SubResource("CurveTexture_kk5o2")     ; 透明度淡出曲线
```

**常用粒子参数**：

- **发射形状**：`emission_shape = 6` + `emission_ring_radius` 实现环形发射
- **速度控制**：`initial_velocity` + `radial_accel` + `tangential_accel`
- **湍流效果**：`turbulence_enabled = true` + `turbulence_noise_strength`
- **颜色渐变**：`color_ramp` 实现粒子生命周期内的颜色变化

#### 在游戏中触发粒子(使用3-3中的VFXUtil)

```csharp
// 实例化后手动触发
VFXUtil.PlaySimple("res://YourMod/scenes/vfx/burst.tscn", _targetPosition);
```