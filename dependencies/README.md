# GCMod Android 编译依赖

当前引用适配 Unity 6000.3.8f1、MelonLoader 0.7.3.0、Harmony 2.10.2.0 和 Il2CppInterop 1.5.1。`font/tsukuardgothic-std-medium` 是发布资源。

Unity 6 CoreModule 中不完整的 `NullableAttribute` 通过 `UnityCore` 程序集别名隔离；使用 Unity 核心类型的代码应保留对应 `extern alias`。完整 Interop 导出放在 `interop-backup/`，编译只使用 `interop/assemblies/` 中的全部 DLL。

更新和同步方法见 [公共工程规范](https://github.com/anosu/ModEngineering/blob/main/docs/CONVENTIONS.md)。
