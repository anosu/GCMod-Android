# GCMod Android

GCMod 7.0.0 的 LemonLoader Android 移植版，适用于「少女艺术绮谭 R」Unity IL2CPP ARM64 客户端。

## 功能

- 剧情标题、人物名、对话和主数据翻译（包括主页角色台词）
- 按数据表开关主数据翻译，支持普通字符串、语言数组和带分隔符的字符串
- 翻译清单校验、并发请求合并、失败重试及本地缓存回退
- Android 中文 TMP 字体、文本颜色、描边及字间距设置
- 普通剧情与 CG 剧情对话框透明度设置
- 帧率设置、跳过大招动画和 Toast 通知

桌面版 F3–F10 快捷键未移植。安卓通过配置文件调整功能，修改后重启游戏生效。

## 安装

1. 为游戏安装与当前版本匹配的 LemonLoader ARM64 环境。模组 ZIP 不包含加载器，也不是 APK；原始游戏 APK 需要先由 LemonLoader Patcher 处理。
2. 将 `GCMod-Android.zip` 解压到游戏的 **MelonLoader base 目录**，保留以下路径：

```text
MelonLoader/
├── Mods/GCMod/
│   ├── GCMod.dll
│   └── Utility.dll
└── UserData/GCMod/
    └── tsukuardgothic-std-medium
```

首次运行生成 `UserData/GCMod.cfg`；翻译缓存位于 `UserData/GCMod/translations/zh-Hans/`。具体 base 目录以加载器日志为准。Android 路径区分大小写，请保留字体文件名的小写形式。

旧 `cache` 目录保留，新版使用新的翻译结构重新下载；也可以提前将新仓库的 `translations/zh-Hans/` 放入上述缓存目录。

## 配置

| 分组 | 配置项 | 默认值 | 说明 |
| --- | --- | --- | --- |
| General | FrameRate | 60 | 正整数；非正数不覆盖游戏帧率 |
| Battle | IsSkipCutin | false | 跳过大招动画 |
| Translation | Enabled | true | 启用翻译 |
| Translation | CDN | 下方翻译仓库的 Raw 地址 | 翻译数据源 |
| Translation | Language | zh-Hans | 翻译语言 |
| Translation | AsyncMode | true | 后台下载；首次进入剧情时翻译可能延迟显示 |
| Translation.MasterData | EnabledTables | ["*"] | 全部表开启；指定表名数组可限制范围，[] 全部关闭 |
| Translation.Font | AssetBundlePath | GCMod/tsukuardgothic-std-medium | 相对于 UserData，也支持绝对路径 |
| Message.Window | NormalAlpha / CgModeAlpha | 0 / 0 | 普通 / CG 对话框透明度，范围 0–1 |
| Message.Text | Modified | true | 修改文本样式 |
| Message.Text | NameColor / MessageColor | FFFFFFFF | 人物名 / 消息文本颜色 |
| Message.Text | FaceDilate | 0.3 | 字体粗细，范围 −1–1 |
| Message.Text | OutlineColor | 3A3A3AFF | 描边颜色 |
| Message.Text | OutlineWidth / OutlineSoftness | 0.3 / 0.01 | 描边宽度 / 羽化，范围 0–1 |
| Message.Text | CharacterSpacing | 0 | 消息文本字间距 |

翻译数据沿用 [girlscreation-translation](https://github.com/anosu/girlscreation-translation)，默认 CDN 为 `https://raw.githubusercontent.com/anosu/girlscreation-translation/refs/heads/main`。

资源地址为 `<CDN>/translations/<Language>/{manifest,names,master}.json` 和 `novels/<id>.json`。

Android 默认异步加载剧情，首次下载完成后可重新进入剧情查看翻译。设为 `AsyncMode = false` 时最多等待 10 秒，超时保留原文并继续后台加载。

主数据在解密解压后、缓存和反序列化前替换。远程翻译未就绪时先用本地缓存；没有可用缓存时最多等待 10 秒，与 `AsyncMode` 无关。已经被游戏消费的主数据不会自动重新翻译，需要重新加载或重启游戏。

`master.json` 按「数据表名 → 字段名 → 原文/译文映射」组织。字段名后缀 `[]` 替换数组第一项，`|` 替换第一个分隔符前的文本，无后缀则替换整个字符串。例如只启用两张表：

```toml
[Translation.MasterData]
EnabledTables = ["mItems", "mUnits"]
```

表名区分大小写。独立的 Home Words 翻译已移除，统一由主数据提供。

## 构建

Utility 通过固定提交的 Git submodule 与 `ProjectReference` 从源码构建，不再维护公共库 DLL 副本。

```powershell
git submodule update --init --recursive
pwsh -NoProfile -File scripts/build-release.ps1
```

输出位于 `artifacts/release/v<version>/`。本地共享开发目录、依赖升级和 CI 配置见 [docs/BUILDING.md](docs/BUILDING.md)。游戏和加载器编译引用见 [dependencies/README.md](dependencies/README.md)。

翻译逻辑的独立回归测试：

```sh
dotnet test tests/GCMod.Tests/GCMod.Tests.csproj -c Release
```

测试隔离了 Unity/Harmony 宿主；ARM64 原生钩子仍需在游戏中验证。安装成功时日志输出 `MasterDataPatch installed`，实际替换时输出 `Master data translated: <表名>`。

## 代码格式化

使用仓库固定版本的 CSharpier 格式化 C# 和项目文件：

```powershell
dotnet tool restore
dotnet tool run csharpier format .
```

仅检查格式、不修改文件：

```powershell
dotnet tool run csharpier check .
```

CI 在构建前执行格式检查。规则为 4 空格缩进、100 列换行宽度和 LF 换行；依赖、本地备份及构建产物通过 `.csharpierignore` 排除。编辑器基础规则由 `.editorconfig` 提供。

## 统一工程入口

源码已迁移到 `src/`，独立测试位于 `tests/`。构建、VS 联调和发布方式以 [docs/BUILDING.md](docs/BUILDING.md) 为准；项目差异配置在 `mod.json`，公共实现来自固定的 `shared/ModEngineering`。
