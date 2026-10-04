# 通用脚本

这里保存不依赖特定用户模组名称的脚本。使用前先检查脚本顶部的参数和路径说明，不要假设作者电脑上的盘符仍然存在。

## 资源与取证

- pck-tool.ps1、scan-pck.ps1、read-pck.ps1：查看 PCK 内部条目、目录块和可提取内容。
- decompile-sts2.ps1：对当前游戏 sts2.dll 做按哈希跳过的版本化反编译。它需要用户提供 ILSpy 命令行工具路径。
- decompile-java.ps1：对塔1 class 批量调用 CFR。它需要用户提供 java.exe 和 cfr.jar，不会假设某个本机工具目录。

## 图像与文件

- make-card-375x526.py：固定尺寸卡面处理。使用前按环境预检确认 Python、Pillow 和 numpy。
- make-relic-outlines.ps1、make-relic-big-icons.ps1：从透明图标生成白色外描边和百科大图黑边。
- recolor-marker-outline.ps1：按精确 RGB 替换指定描边颜色，保留 alpha 并支持 dry-run。
- extract-sts1-cards.ps1、extract-sts1-spec.ps1、extract-sts1-upgrade.ps1：从用户指定的塔1 Java 反编译目录提取卡牌字段、升级语句和规格 JSON。
- gen-card-loc.ps1：把提取结果中的常见占位符转成塔2 本地化字段；项目关键字着色必须显式传入。
- clean-loc-whitespace.ps1：报告或清理本地化值里的多余空格，默认只报告不改文件。
- slugify-check.ps1：将资源映射表中的类名与引擎 Slugify 结果对拍。
- verify-pck-load.ps1：根据 CSV 资源清单，用真实 Godot 进程挂载 PCK 并验证 ResourceLoader 是否能加载。
- assert-t1-t2-card-contract.ps1：根据显式传入的 T1 规格、T1/T2 映射和运行时契约，断言卡牌可机械对比的字段。
- hexdump.ps1：检查文件头、编码和二进制结构。
- fix-ps1-bom.ps1：修复 PowerShell 5.1 对无 BOM UTF-8 脚本的误读。

脚本修改后要保持 Windows PowerShell 可解析，并说明所需的 PowerShell、Python、.NET 或其他外部依赖。

塔1 提取工具只负责把源代码中可机械确认的字段整理出来，不会把塔1 效果逻辑自动翻译成塔2 行为；塔2 实现仍需按调用链和运行结果复核。PCK 验证工具需要用户准备资源路径清单，并把“预先存在的原版资源”与真正由模组提供的资源分开看待。
