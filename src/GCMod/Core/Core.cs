using System;
using System.Collections;
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
    private static AssetBundleLoader<TMP_FontAsset> _font;
    private static IEnumerator _fontLoad;
    private static object _fontCoroutine;

    public static MelonLogger.Instance Log { get; private set; }
    internal static TranslationManager Trans { get; private set; }
    internal static TMP_FontAsset FontAsset => _font?.Asset;

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
                () =>
                    new TranslationSettings(
                        Config.Translation.Value,
                        Config.AsyncMode.Value,
                        Config.MasterDataTables.Value
                    )
            );
            _font = new AssetBundleLoader<TMP_FontAsset>(fontPath);
            _fontLoad = _font.Load(
                () =>
                {
                    if (!TMP_Settings.fallbackFontAssets.Contains(_font.Asset))
                        TMP_Settings.fallbackFontAssets.Add(_font.Asset);
                    Logger.Info($"Font loaded: {_font.Asset.name}");
                },
                error => Logger.Error($"Font load failed: {error.Message}")
            );
            _fontCoroutine = MelonCoroutines.Start(_fontLoad);
            _ = Trans.LoadTranslationAsync();
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
        Trans = null;
        Config.Shutdown();
        if (_fontCoroutine != null)
            MelonCoroutines.Stop(_fontCoroutine);
        (_fontLoad as IDisposable)?.Dispose();
        if (_font?.IsLoaded == true)
            TMP_Settings.fallbackFontAssets.Remove(_font.Asset);
        _font = null;
        _fontLoad = null;
        _fontCoroutine = null;
        _httpClient?.Dispose();
        _httpClient = null;
        Toast.Shutdown();
        Logging.SetSink(null);
    }
}
