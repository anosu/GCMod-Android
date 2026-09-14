using HarmonyLib;
using Il2CppGcObfuscate.Service;

namespace GCMod.Patches;

/// <summary>
/// 签名兼容
/// </summary>
[HarmonyPatch]
public static class SignaturePatch
{
    private const string SIGNATURE =
        "e1c9497149f8f2096fe428641ccea1840b03217cf579c9f9d95d32dc5b665c01";

    /// <summary>返回游戏原始签名哈希</summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AndroidSignature), nameof(AndroidSignature.sha256))]
    public static void PatchSignature(ref string __result)
    {
        __result = SIGNATURE;
    }
}
