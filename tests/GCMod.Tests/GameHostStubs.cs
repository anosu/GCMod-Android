using System.Collections;

// 隔离 Unity/Harmony 宿主；剧情跟踪、刷新、查询与缓存执行实际生产代码。
namespace GCMod
{
    public sealed class TestSetting<T>(T value)
    {
        public T Value = value;
    }

    public static class Config
    {
        public static readonly TestSetting<bool> Translation = new(true);
        public static readonly TestSetting<bool> AsyncMode = new(false);
        public static readonly TestSetting<bool> ModifyText = new(false);
        public static readonly TestSetting<string[]> MasterDataTables = new([]);
        public static readonly TestSetting<float> CharacterSpacing = new(0);
    }

    public static class Core
    {
        public static Services.TranslationManager Trans;
    }
}

namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class HarmonyPatch : Attribute
    {
        public HarmonyPatch(params object[] arguments) { }
    }

    public sealed class HarmonyPrefix : Attribute { }

    public sealed class Harmony
    {
        public Harmony(string id) { }

        public void PatchAll(Type type) { }

        public void UnpatchSelf() { }
    }
}

namespace MelonLoader
{
    public static class MelonCoroutines
    {
        public static object Start(IEnumerator iterator) => new();

        public static void Stop(object coroutine) { }
    }
}

namespace Il2CppTMPro
{
    public sealed class TextMeshProUGUI
    {
        public TMP_FontAsset font;
    }

    public sealed class TMP_FontAsset
    {
        public string name;
    }

    public static class TMP_Settings
    {
        public static readonly List<TMP_FontAsset> fallbackFontAssets = new();
    }
}

namespace Utility.Assets
{
    public sealed class AssetBundleLoader<T>
    {
        public T Asset;
        public bool IsLoaded => false;

        public IEnumerator Load(Action complete, Action<Exception> error)
        {
            yield break;
        }
    }
}

namespace GCMod.Patches
{
    public sealed class EnhancePatch { }

    public sealed class VisualPatch { }

    public sealed class SignaturePatch { }

    public static class MasterDataPatch
    {
        public static void Install() { }

        public static bool TryUninstall() => true;
    }
}

namespace Il2CppDMM.OLG.Unity.Engine.Internal
{
    public sealed class ScriptObjectManager
    {
        public void Create() { }
    }
}

namespace Il2CppDMM.OLG.Unity.Extensions.Novel
{
    public sealed class Label
    {
        public string text;
    }

    public sealed class EventTitle
    {
        public readonly Label _TitleMain = new();

        public void ShowBlurEffect() { }
    }

    public sealed class EventMessage
    {
        public void SetName() { }
    }

    public sealed class EventText
    {
        public float fontSpacing;

        public void Parse() { }
    }
}
