using System;
using System.IO;
using System.Net;
using System.Net.Http;
using GCMod.Patches;
using GCMod.Services;
using Il2CppTMPro;
using MelonLoader;
using MelonLoader.Utils;
using Utility.Assets;
using Utility.Diagnostics;
using Utility.Notifications;

[assembly: MelonInfo(
    typeof(GCMod.Core),
    GCMod.ModInfo.Name,
    GCMod.ModInfo.Version,
    GCMod.ModInfo.Author
)]
[assembly: HarmonyDontPatchAll]

namespace GCMod;

/// <summary>LemonLoader Android 入口，负责配置、服务和补丁的生命周期。</summary>
public sealed class Core : MelonMod
{
    private static HttpClient _httpClient;

    public static MelonLogger.Instance Log { get; private set; }
    public static TranslationManager Trans { get; private set; }

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        try
        {
            Logging.SetSink(entry =>
            {
                string message = $"[{entry.Category}] {entry.Message}";
                if (entry.Exception != null)
                    message += $"\n{entry.Exception}";
                switch (entry.Level)
                {
                    case LogLevel.Warning:
                        Log.Warning(message);
                        break;
                    case LogLevel.Error:
                        Log.Error(message);
                        break;
                    default:
                        Log.Msg(message);
                        break;
                }
            });
            Toast.Initialize("GCMod.ToastManager");
            Config.Initialize();
            _httpClient = new HttpClient(
                new SocketsHttpHandler
                {
                    AutomaticDecompression =
                        DecompressionMethods.GZip
                        | DecompressionMethods.Deflate
                        | DecompressionMethods.Brotli,
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                }
            )
            {
                Timeout = TimeSpan.FromSeconds(10),
            };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                $"{ModInfo.Name}/{ModInfo.Version}"
            );
            TranslationCache CreateCache() =>
                new(
                    Config.TranslationCDN.Value,
                    Path.Combine(MelonEnvironment.UserDataDirectory, "GCMod", "translations"),
                    Config.TranslationLanguage.Value,
                    _httpClient
                );
            string fontPath = Config.FontBundlePath.Value;
            if (!Path.IsPathRooted(fontPath))
                fontPath = Path.Combine(MelonEnvironment.UserDataDirectory, fontPath);
            Trans = new TranslationManager(
                CreateCache,
                new AssetBundleLoader<TMP_FontAsset>(fontPath)
            );
            Trans.Initialize();
            MasterDataPatch.JsonRewriter = (attribute, json) =>
                Trans.TranslateMasterData(attribute?.Object, json);
            PatchManager.Initialize();
            Logger.Info($"{ModInfo.Name} {ModInfo.Version} loaded successfully");
            Toast.Success(ModInfo.Name, $"Mod 加载成功，版本: {ModInfo.Version}", duration: 7f);
        }
        catch (Exception e)
        {
            Logger.Error($"Initialization failed: {e}");
            Shutdown();
            throw;
        }
    }

    public override void OnDeinitializeMelon() => Shutdown();

    private static void Shutdown()
    {
        if (!PatchManager.Shutdown())
        {
            Logger.Warn("Shutdown deferred: master data load is still active");
            return;
        }
        MasterDataPatch.JsonRewriter = null;
        Trans?.Dispose();
        _httpClient?.Dispose();
        _httpClient = null;
        Toast.Shutdown();
        Logging.SetSink(null);
    }
}
