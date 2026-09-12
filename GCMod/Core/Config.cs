extern alias UnityCore;

using System;
using System.Collections.Generic;
using System.IO;
using UnityCore::UnityEngine;
using MelonLoader;
using MelonLoader.Utils;
using Utility.Notifications;

namespace GCMod
{
    /// <summary>
    /// 全局配置管理器。
    /// 负责初始化所有配置项并绑定事件监听。
    /// </summary>
    public static class Config
    {
        public static readonly string FilePath = Path.Combine(
            MelonEnvironment.UserDataDirectory,
            "GCMod.cfg"
        );
        private static readonly Dictionary<string, MelonPreferences_Category> Categories = new();
        private static bool _initializing;
        public static Color NameTextColor { get; private set; } = Color.white;
        public static Color MessageTextColor { get; private set; } = Color.white;
        public static Color OutlineColor { get; private set; } = new(0.235f, 0.235f, 0.235f);

        #region General
        public static MelonPreferences_Entry<int> FrameRate;
        #endregion

        #region Battle
        public static MelonPreferences_Entry<bool> IsSkipCutin;
        #endregion

        #region Translation
        public static MelonPreferences_Entry<bool> Translation;
        public static MelonPreferences_Entry<string> TranslationCDN;
        public static MelonPreferences_Entry<string> TranslationLanguage;
        public static MelonPreferences_Entry<bool> AsyncMode;
        public static MelonPreferences_Entry<string[]> MasterDataTables;
        #endregion

        #region Font
        public static MelonPreferences_Entry<string> FontBundlePath;
        #endregion

        #region MessageWindow
        public static MelonPreferences_Entry<float> NormalAlpha;
        public static MelonPreferences_Entry<float> CgModeAlpha;
        #endregion

        #region MessageText
        public static MelonPreferences_Entry<bool> ModifyText;
        public static MelonPreferences_Entry<string> NameTextColorHex;
        public static MelonPreferences_Entry<string> MessageTextColorHex;
        public static MelonPreferences_Entry<float> FaceDilate;
        public static MelonPreferences_Entry<string> OutlineColorHex;
        public static MelonPreferences_Entry<float> OutlineWidth;
        public static MelonPreferences_Entry<float> OutlineSoftness;
        public static MelonPreferences_Entry<float> CharacterSpacing;
        #endregion

        /// <summary>
        /// 初始化配置系统。
        /// </summary>
        public static void Initialize()
        {
            _initializing = true;
            try
            {
                BindAllEntries();
                foreach (var category in Categories.Values)
                    category.LoadFromFile(false);
                BindColorEntries();
                foreach (var category in Categories.Values)
                    category.SaveToFile(false);
            }
            finally
            {
                _initializing = false;
            }
        }

        private static void BindAllEntries()
        {
            #region General
            FrameRate = Bind("General", "FrameRate", 60, "游戏帧率（正整数）");
            #endregion

            #region Battle
            IsSkipCutin = Bind(
                "Battle",
                "IsSkipCutin",
                false,
                "是否跳过大招动画（包括变身和释放动画）"
            );
            #endregion

            #region Translation
            Translation = Bind(
                "Translation",
                "Enabled",
                true,
                "是否开启游戏内翻译（剧情与主数据）"
            );
            TranslationCDN = Bind(
                "Translation",
                "CDN",
                "https://raw.githubusercontent.com/anosu/girlscreation-translation/refs/heads/main",
                "翻译仓库或本地服务的根地址，自动拼接 /translations/<Language>/"
            );
            TranslationLanguage = Bind(
                "Translation",
                "Language",
                "zh-Hans",
                "翻译语言，取值范围：[zh-Hans]"
            );
            AsyncMode = Bind(
                "Translation",
                "AsyncMode",
                true,
                "异步请求剧情翻译；关闭时最多等待 10 秒。主数据无缓存时始终最多等待 10 秒，超时保留原文并继续后台加载"
            );
            MasterDataTables = Bind(
                "Translation.MasterData",
                "EnabledTables",
                new[] { "*" },
                "启用翻译的主数据表名数组；默认 [\"*\"] 全部开启，[] 全部关闭。区分大小写，修改后重启生效"
            );
            #endregion

            #region Font
            FontBundlePath = Bind(
                "Translation.Font",
                "AssetBundlePath",
                "GCMod/tsukuardgothic-std-medium",
                "TMP字体AssetBundle路径，相对于MelonLoader/UserData，也可使用绝对路径；修改后重启生效"
            );
            #endregion

            #region MessageWindow
            NormalAlpha = Bind(
                "Message.Window",
                "NormalAlpha",
                0f,
                "普通剧情中的对话框透明度，默认完全透明"
            );
            CgModeAlpha = Bind(
                "Message.Window",
                "CgModeAlpha",
                0f,
                "寝室剧情中的对话框透明度，默认完全透明"
            );
            #endregion

            #region MessageText
            ModifyText = Bind(
                "Message.Text",
                "Modified",
                true,
                "是否更改对话框文本样式（用于对话框透明时提高对比度）"
            );
            NameTextColorHex = Bind(
                "Message.Text",
                "NameColor",
                "FFFFFFFF",
                "对话框人物名文本颜色"
            );
            MessageTextColorHex = Bind(
                "Message.Text",
                "MessageColor",
                "FFFFFFFF",
                "对话框消息文本颜色"
            );
            FaceDilate = Bind("Message.Text", "FaceDilate", 0.3f, "字体粗细，取值范围：[-1, 1]");
            OutlineColorHex = Bind(
                "Message.Text",
                "OutlineColor",
                "3A3A3AFF",
                "文本描边颜色，十六进制格式"
            );
            OutlineWidth = Bind(
                "Message.Text",
                "OutlineWidth",
                0.3f,
                "文本描边宽度，取值范围：[0, 1]"
            );
            OutlineSoftness = Bind(
                "Message.Text",
                "OutlineSoftness",
                0.01f,
                "文本描边羽化程度，取值范围：[0, 1]"
            );
            CharacterSpacing = Bind(
                "Message.Text",
                "CharacterSpacing",
                0f,
                "字间距，仅对消息文本设置，不应用于人物名"
            );
            #endregion
        }

        /// <summary>
        /// 绑定配置变更日志输出。
        /// </summary>
        private static MelonPreferences_Entry<T> Bind<T>(
            string section,
            string key,
            T defaultValue,
            string description
        )
        {
            if (!Categories.TryGetValue(section, out var category))
            {
                category = MelonPreferences.CreateCategory(section);
                category.SetFilePath(FilePath, false, false);
                Categories.Add(section, category);
            }
            var entry = category.CreateEntry(key, defaultValue, description, description);
            entry.OnEntryValueChanged.Subscribe(
                (_, value) =>
                {
                    if (_initializing)
                        return;
                    category.SaveToFile(false);
                    Logger.Info($"[{section}] {key} => {value}");
                    Toast.Info($"[{section}]", $"{key} => {value}");
                }
            );
            return entry;
        }

        private static void BindColorEntries()
        {
            var bindings = new (MelonPreferences_Entry<string> Entry, Action<Color> Setter)[]
            {
                (NameTextColorHex, c => NameTextColor = c),
                (MessageTextColorHex, c => MessageTextColor = c),
                (OutlineColorHex, c => OutlineColor = c),
            };

            foreach (var (entry, setter) in bindings)
            {
                ParseAndSetColor(entry, setter);
                entry.OnEntryValueChanged.Subscribe((_, _) => ParseAndSetColor(entry, setter));
            }
        }

        private static void ParseAndSetColor(
            MelonPreferences_Entry<string> entry,
            Action<Color> setter
        )
        {
            var hex = NormalizeHexColor(entry.Value);
            if (ColorUtility.TryParseHtmlString(hex, out var color))
                setter(color);
        }

        private static string NormalizeHexColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return "#FFFFFF";
            hex = hex.Trim().TrimStart('#');
            return "#" + hex;
        }
    }
}
