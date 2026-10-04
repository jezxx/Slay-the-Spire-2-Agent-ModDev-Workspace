# 工作流脚本

这里的脚本面向公开工作区，不假设用户使用作者的盘符、私人模组名称或本地框架。

## 编译前环境预检

按任务路线检查依赖：

    .\check-environment.ps1

原生 C# 模组需要 .NET 9 SDK 和游戏数据程序集。图片脚本需要时再用 -RequirePython 检查 Python 3、numpy 和 Pillow；Godot/PCK 或场景任务需要时再用 -RequireGodot 检查 Godot。脚本只检测，不安装、不升级、不删除软件。

检查指定游戏目录：

    .\check-environment.ps1 -GameDir "<杀戮尖塔2游戏目录>"

## 新建原生 C# 模组

在公开工作区根目录打开 PowerShell：

    Set-Location "<工作区>\04-工具\工作流"
    .\new-mod.ps1 -ModId MyFirstMod -ModName "我的模组" -Author "作者"

脚本会在 09-用户项目区\MyFirstMod 创建工程、清单、项目档案、记录目录和示例本地化文件。

## 编译并部署

    .\build-native-mod.ps1 -ProjectDir "<工作区>\09-用户项目区\MyFirstMod" -GameDir "<杀戮尖塔2游戏目录>"

只编译、不复制到游戏目录：

    .\build-native-mod.ps1 -ProjectDir "<模组目录>" -GameDir "<游戏目录>" -NoDeploy

如果 .NET SDK 在中文工作区或受限临时目录下报 NuGetScratch、临时目录或编码错误，可把临时目录显式指定为 ASCII 路径：

    .\build-native-mod.ps1 -ProjectDir "<模组目录>" -GameDir "<游戏目录>" -NoDeploy -DotnetExe "C:\Program Files\dotnet\dotnet.exe" -TempRoot "D:\temp-dotnet"

查询工具需要游戏程序集，完整构建命令为：

    .\build-query-tools.ps1 -GameDir "<杀戮尖塔2游戏目录>" -DotnetExe "C:\Program Files\dotnet\dotnet.exe" -TempRoot "D:\temp-dotnet"

查询工具和 API 导出脚本也支持同名的 `-TempRoot` 参数。公共脚本不会把任何作者电脑的临时目录写死。脚本还会在受限环境缺少 APPDATA、ProgramFiles 等标准变量时按 USERPROFILE 和系统盘推导它们。

脚本会：

- 检查游戏数据目录和同名清单是否存在。
- 把游戏目录和数据目录作为 MSBuild 参数传入，不把作者电脑的盘符写进工程。
- 检查游戏是否正在运行，避免 DLL 被锁定后产生假失败。
- 编译后在 bin\<Configuration> 下递归定位同名 DLL，并检查 DLL 没有早于最近源码修改时间。
- 仅在明确部署时复制 DLL、清单和模组资源目录到 <游戏目录>\mods\<ModId>。


## 资源、查询和 API

资源项目需要 Godot/PCK 或场景导出时，先通过预检确认 Godot，再执行：

    .\pack-pck.ps1 -ProjectDir "<模组目录>" -GameDir "<杀戮尖塔2游戏目录>" -ModId MyFirstMod -GodotExe "<Godot.exe>" -Deploy

查询工具源码需要当前游戏程序集，使用：

    .\build-query-tools.ps1 -GameDir "<杀戮尖塔2游戏目录>" -DotnetExe "<dotnet.exe>" -TempRoot "D:\temp-dotnet"

按当前游戏版本导出 API 索引，不覆盖旧版本：

    .\export-api.ps1 -GameDir "<杀戮尖塔2游戏目录>" -DotnetExe "<dotnet.exe>" -TempRoot "D:\temp-dotnet"

## 完整开发循环

原生代码、资源和运行验证都需要时，可以使用：

    .\dev.ps1 -ModId MyFirstMod -GameDir "<杀戮尖塔2游戏目录>" -ProjectDir "<模组目录>" -DotnetExe "<dotnet.exe>" -GodotExe "<Godot.exe>" -TempRoot "D:\temp-dotnet"

它会依次编译、导出 PCK、部署、重启游戏并检查真实初始化日志。只改代码或只想编译时，使用 compile-check.ps1；只改资源时仍要重新导出 PCK。

## 发布暂存

准备不含源码的本地创意工坊目录：

    .\prepare-workshop-package.ps1 -ProjectDir "<模组目录>" -OutputDir "<暂存目录>" -ModId MyFirstMod

这个脚本只整理已经构建并验证过的清单、DLL、PCK 和必要资源，不上传、不读取账号凭据。最终上传前仍由用户检查版权、说明、预览图和工坊设置。
## 边界

这两个脚本只覆盖原生 C# 模组。需要 PCK、Godot 导出、第三方框架、Steam Workshop 打包或版本适配时，必须先阅读对应教程和经验文档，不能把本脚本当作万能构建器。

如果游戏版本不是 0.111.0，先保留公开包里的基线资料，再按 02-游戏反编译资料 的版本流程重新导出并核对。
