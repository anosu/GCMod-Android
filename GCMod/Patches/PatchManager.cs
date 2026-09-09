using System.Collections.Generic;
using HarmonyLib;
using Il2CppTMPro;

namespace GCMod.Patches;

/// <summary>
/// Harmony 补丁管理器。负责初始化所有子补丁类、提供共享工具方法。
/// </summary>
public static class PatchManager
{
    private static HarmonyLib.Harmony _harmony;

    /// <summary>当前加载的剧情 Novel ID。</summary>
    public static int NovelId;

    /// <summary>
    /// 创建并注册所有 Harmony 补丁。
    /// </summary>
    public static void Initialize()
    {
        _harmony = new HarmonyLib.Harmony("GCMod.Android");
        _harmony.PatchAll(typeof(SignaturePatch));
        _harmony.PatchAll(typeof(EnhancePatch));
        _harmony.PatchAll(typeof(TranslationPatch));
        _harmony.PatchAll(typeof(VisualPatch));
        _harmony.PatchAll(typeof(HomePatch));
    }

    public static void Shutdown()
    {
        _harmony?.UnpatchSelf();
        _harmony = null;
        NovelId = 0;
    }

    /// <summary>
    /// 尝试获取当前 Novel ID 对应的翻译字典。
    /// </summary>
    public static bool TryGetCurrentNovel(out Dictionary<string, string> translation)
    {
        translation = null;
        return Config.Translation.Value && Core.Trans.Novels.TryGetValue(NovelId, out translation);
    }

    /// <summary>
    /// 对 TMP 文本组件应用翻译字体（带空安全检查）。
    /// </summary>
    public static void ApplyTranslationFont(TextMeshProUGUI text)
    {
        if (text != null && Core.Trans.Font.IsLoaded)
            text.font = Core.Trans.Font.Asset;
    }
}
