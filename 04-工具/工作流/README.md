# 工作流脚本

这里的脚本面向公开工作区，不假设用户使用作者的盘符、私人模组名称或本地框架。

## 新建原生 C# 模组

在公开工作区根目录打开 PowerShell：

    Set-Location "<工作区>\04-工具\工作流"
    .\new-mod.ps1 -ModId MyFirstMod -ModName "我的模组" -Author "作者"

脚本会在 09-用户项目区\MyFirstMod 创建工程、清单、项目档案、记录目录和示例本地化文件。

## 编译并部署

    .\build-native-mod.ps1 -ProjectDir "<工作区>\09-用户项目区\MyFirstMod" -GameDir "<杀戮尖塔2游戏目录>"

只编译、不复制到游戏目录：

    .\build-native-mod.ps1 -ProjectDir "<模组目录>" -GameDir "<游戏目录>" -Deploy:$false

脚本会：

- 检查游戏数据目录和同名清单是否存在。
- 把游戏目录和数据目录作为 MSBuild 参数传入，不把作者电脑的盘符写进工程。
- 检查游戏是否正在运行，避免 DLL 被锁定后产生假失败。
- 编译后通过 TargetPath 定位 DLL，并检查 DLL 没有早于最近源码修改时间。
- 仅在明确部署时复制 DLL、清单和模组资源目录到 <游戏目录>\mods\<ModId>。

## 边界

这两个脚本只覆盖原生 C# 模组。需要 PCK、Godot 导出、第三方框架、Steam Workshop 打包或版本适配时，必须先阅读对应教程和经验文档，不能把本脚本当作万能构建器。

如果游戏版本不是 0.111.0，先保留公开包里的基线资料，再按 02-游戏反编译资料 的版本流程重新导出并核对。
