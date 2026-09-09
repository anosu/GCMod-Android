using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Il2CppTMPro;
using MelonLoader;
using Utility.Assets;
using Utility.Notifications;

namespace GCMod.Services;

/// <summary>
/// 翻译管理器：协调翻译数据的加载、缓存和查询。
/// 内部持有所有翻译数据。
/// </summary>
public class TranslationManager
{
    private readonly TranslationCache _cache;
    private readonly AssetBundleLoader<TMP_FontAsset> _font;
    private object _fontCoroutine;
    private Task _sharedTranslations;
    private volatile bool _shutdown;

    public Dictionary<string, string> Names { get; private set; } = [];
    public Dictionary<string, string> Words { get; private set; } = [];
    public ConcurrentDictionary<int, Dictionary<string, string>> Novels { get; } = new();
    public AssetBundleLoader<TMP_FontAsset> Font => _font;

    public TranslationManager(TranslationCache cache, AssetBundleLoader<TMP_FontAsset> font)
    {
        _cache = cache;
        _font = font;
    }

    public void Initialize()
    {
        _fontCoroutine = MelonCoroutines.Start(
            _font.Load(onError: e => Logger.Error($"Font loading failed: {e}"))
        );
        _sharedTranslations = Task.Run(async () =>
        {
            try
            {
                await LoadTranslationAsync();
            }
            catch (Exception e)
            {
                if (!_shutdown)
                    Logger.Error($"Shared translations failed: {e}");
            }
        });
    }

    public void Shutdown()
    {
        _shutdown = true;
        if (_fontCoroutine != null)
        {
            MelonCoroutines.Stop(_fontCoroutine);
            _fontCoroutine = null;
        }
    }

    public async Task LoadTranslationAsync()
    {
        if (!Config.Translation.Value)
            return;

        await _cache.FetchManifestAsync();

        var nameTask = _cache.LoadAsync(TranslationPaths.Names);
        var wordTask = _cache.LoadAsync(TranslationPaths.Words);
        await Task.WhenAll(nameTask, wordTask);
        if (_shutdown)
            return;

        if (nameTask.Result != null)
        {
            Names = nameTask.Result;
            Logger.Info($"Character names translation loaded. Total: {Names.Count}");
        }
        else
        {
            Logger.Warn("Character names translation load failed");
            Toast.Warning("加载失败", "角色名称翻译加载失败");
        }

        if (wordTask.Result != null)
        {
            Words = wordTask.Result;
            Logger.Info($"Character words translation loaded. Total: {Words.Count}");
        }
        else
        {
            Logger.Warn("Character words translation load failed");
            Toast.Warning("加载失败", "角色台词翻译加载失败");
        }
    }

    public Task GetNovelTranslationAsync(int novelId) =>
        Task.Run(async () =>
        {
            if (_shutdown || Novels.ContainsKey(novelId))
                return;

            try
            {
                await _sharedTranslations;
                var translations = await _cache.LoadAsync(
                    TranslationPaths.Novels,
                    novelId.ToString()
                );
                if (_shutdown)
                    return;
                if (translations != null)
                {
                    Novels[novelId] = translations;
                    Logger.Info($"Scenario translation loaded. Total: {translations.Count}");
                }
                else
                {
                    Logger.Warn($"Translations loaded failed: {novelId}");
                    Toast.Warning("加载失败", $"剧本ID: {novelId}");
                }
            }
            catch (Exception e)
            {
                if (!_shutdown)
                    Logger.Error($"Novel {novelId} translation failed: {e}");
            }
        });
}
