using System.Threading.Tasks;
using HarmonyLib;
using Il2CppDMM.OLG.Unity.Engine.Internal;
using Il2CppDMM.OLG.Unity.Extensions.Novel;

namespace GCMod.Patches;

/// <summary>
/// 剧情翻译补丁：标题、人名、对话文本的翻译注入。
/// </summary>
[HarmonyPatch]
public static class TranslationPatch
{
    /// <summary>
    /// 剧情加载时获取对应 Novel ID，触发翻译数据预加载。
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScriptObjectManager), nameof(ScriptObjectManager.Create))]
    public static void SetupTranslation(string prefix, string id)
    {
        if (!Config.Translation.Value)
            return;

        Logger.Info($"Prefix: {prefix}, Id: {id}");
        if (!int.TryParse(id, out int novelId))
        {
            PatchManager.NovelId = 0;
            Logger.Warn($"Unsupported novel ID: {id}");
            return;
        }
        PatchManager.NovelId = novelId;

        if (!Core.Trans.Novels.ContainsKey(PatchManager.NovelId))
        {
            Task task = Core.Trans.GetNovelTranslationAsync(novelId);
            if (!Config.AsyncMode.Value)
                task.GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// 翻译章节标题。
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(EventTitle), nameof(EventTitle.ShowBlurEffect))]
    public static void SetMessageTitle(EventTitle __instance)
    {
        if (!Config.Translation.Value)
            return;

        if (PatchManager.TryGetCurrentNovel(out var translation))
        {
            if (translation.TryGetValue(__instance._TitleMain.text, out string title))
                __instance._TitleMain.text = title;
        }
    }

    /// <summary>
    /// 翻译说话人名。
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(EventMessage), nameof(EventMessage.SetName))]
    public static void SetMessageName(ref string text)
    {
        if (!Config.Translation.Value)
            return;

        if (PatchManager.TryGetCurrentNovel(out _))
        {
            if (Core.Trans.Names.TryGetValue(text, out string name))
                text = name;
        }
    }

    /// <summary>
    /// 翻译对话文本，并应用字间距设置。
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(EventText), nameof(EventText.Parse))]
    public static void SetMessageText(EventText __instance, ref string message)
    {
        if (Config.Translation.Value && PatchManager.TryGetCurrentNovel(out var translation))
        {
            if (translation.TryGetValue(message, out string text))
                message = text;
        }
        if (Config.ModifyText.Value)
            __instance.fontSpacing = Config.CharacterSpacing.Value;
    }
}
