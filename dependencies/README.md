# GCMod Android 构建依赖

仓库只跟踪构建与打包所需的依赖，CI 不需要完整游戏导出或本地备份。

| 路径 | 用途 | Git 跟踪 |
| --- | --- | --- |
| `interop/assemblies/` | 项目显式引用的最小 IL2CPP 代理程序集集合 | 是 |
| `melonloader/net6/` | MelonLoader、Harmony、Il2CppInterop.Runtime 编译引用 | 是 |
| `font/tsukuardgothic-std-medium` | Android 中文字体 AssetBundle | 是 |
| `interop-backup/` | 完整 Interop 导出及生成清单，仅供本地补充引用 | 否 |

当前依赖适配 Unity 6000.3.8f1、MelonLoader 0.7.3.0、Harmony 2.10.2.0 和 Il2CppInterop 1.5.1。加载器与游戏代理仅用于编译，不放入模组发布包。

Unity 6 CoreModule 中不完整的 NullableAttribute 通过 `UnityCore` 程序集别名隔离，使用 Unity 核心类型的代码应保留对应 `extern alias`。

## 补充引用

1. 在本地将完整 Interop 导出保存到 `dependencies/interop-backup/`，包括生成清单（如有）。该目录已加入 `.gitignore`，克隆仓库时不会下载。
2. 需要引用新的游戏程序集时，将对应 DLL 从备份复制到 `dependencies/interop/assemblies/`，并在 `src/GCMod/GCMod.csproj` 添加带 `GameInteropReferenceDirectory` 路径的显式 `Reference`，设置 `Private="false"`。
3. 运行发布构建验证，将项目文件和新增的必要 DLL 一起提交。构建不得依赖备份目录。

## 更新依赖

游戏或加载器更新后，使用匹配版本的完整导出更新本地备份，再同步项目声明的最小引用集：

```powershell
pwsh -NoProfile -File scripts/sync-dependencies.ps1 `
    -InteropDirectory dependencies/interop-backup `
    -MelonLoaderDirectory <LemonLoader的net6目录>

pwsh -NoProfile -File scripts/build-release.ps1
```

源目录不能与对应目标目录相同。同步脚本按照项目引用列表复制 DLL，并清除目标引用目录中未被引用的文件；完整导出应保存在备份目录。更新游戏版本时应整体替换本地备份，避免混用不同版本的 DLL。

可通过 `GameInteropReferenceDirectory` 和 `MelonLoaderReferenceDirectory` 覆盖 Mod 的编译引用位置。公共库的源码选择见 [构建说明](../docs/BUILDING.md)。

## 共享库

Utility 通过 `shared/Utility` 源码子模块和 `ProjectReference` 构建。更新共享库时更新源码和子模块指针，或使用本地源码覆盖配置，具体见 [构建说明](../docs/BUILDING.md)。

构建会自动将共享库 DLL 复制到 Mod 输出目录，发布脚本从该目录打包。`dependencies/managed/` 已废弃，不参与编译或打包，整目录忽略 Git；旧克隆中的该目录可直接删除，无需重新复制 DLL。

共享库使用自身固定的编译依赖，不依赖完整 Interop 备份。清理旧 DLL 副本时应保留游戏／加载器的必要引用、字体资源和完整 Interop 本地备份。
