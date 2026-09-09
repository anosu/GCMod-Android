# GCMod Android

GCMod 6.2.0 的 LemonLoader Android 移植版，适用于「少女艺术绮谭 R」Unity IL2CPP ARM64 客户端。

## 功能

- 剧情标题、人物名、对话和主页角色台词翻译
- 翻译清单校验、本地缓存与下载失败时的缓存回退
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
    └── tsukuardgothic-std-bold
```

首次运行生成 `UserData/GCMod.cfg`；翻译缓存位于 `UserData/GCMod/cache/zh_Hans/`。具体 base 目录以加载器日志为准。Android 路径区分大小写，请保留字体文件名的小写形式。

## 配置

| 分组 | 配置项 | 默认值 | 说明 |
| --- | --- | --- | --- |
| General | FrameRate | 60 | 正整数；非正数不覆盖游戏帧率 |
| Battle | IsSkipCutin | false | 跳过大招动画 |
| Translation | Enabled | true | 启用翻译 |
| Translation | CDN | 下方翻译仓库的 Raw 地址 | 翻译数据源 |
| Translation | Language | zh_Hans | 翻译语言 |
| Translation | AsyncMode | true | 后台下载；首次进入剧情时翻译可能延迟显示 |
| Translation.Font | AssetBundlePath | GCMod/tsukuardgothic-std-bold | 相对于 UserData，也支持绝对路径 |
| Message.Window | NormalAlpha / CgModeAlpha | 0 / 0 | 普通 / CG 对话框透明度，范围 0–1 |
| Message.Text | Modified | true | 修改文本样式 |
| Message.Text | NameColor / MessageColor | FFFFFFFF | 人物名 / 消息文本颜色 |
| Message.Text | FaceDilate | 0.3 | 字体粗细，范围 −1–1 |
| Message.Text | OutlineColor | 3A3A3AFF | 描边颜色 |
| Message.Text | OutlineWidth / OutlineSoftness | 0.3 / 0.01 | 描边宽度 / 羽化，范围 0–1 |
| Message.Text | CharacterSpacing | 0 | 消息文本字间距 |

翻译数据沿用 [girlscreaionr-translation](https://github.com/anosu/girlscreaionr-translation)，默认 CDN 为 `https://raw.githubusercontent.com/anosu/girlscreaionr-translation/refs/heads/main`。

Android 默认采用异步下载，避免网络等待阻塞游戏主线程。首次下载完成后可重新进入剧情查看翻译。设为 `AsyncMode = false` 可等待翻译后继续加载，但弱网时可能造成界面停顿。缓存回退需要先成功下载过对应翻译，不能代替游戏本身的联网需求。

## 构建

仓库包含构建所需的最小依赖集，克隆后可直接构建：

```powershell
pwsh -NoProfile -File scripts/build-release.ps1
```

输出：`artifacts/release/v6.2.0/GCMod-Android.zip` 和 `SHA256SUMS.txt`。脚本检查版本、归档路径及各文件 SHA-256。

单独编译：

```powershell
dotnet build GCMod/GCMod.csproj -c Release
```

使用 .NET 8 SDK（见 `global.json`），目标框架为 `net6.0`。依赖更新及本地完整 Interop 备份的使用方式见 [dependencies/README.md](dependencies/README.md)。

GitHub Actions 在 push / pull request 时构建验证，推送与项目版本一致的 `v*` 标签时发布 ZIP 与校验文件。

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
