using PPDConfiguration;
using SharpDX.Direct3D9;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PPDFramework
{
    /// <summary>
    /// PPDの設定クラス
    /// </summary>
    public class PPDSetting
    {
        string settingFileName = "PPD.ini";
        static PPDSetting setting;
        static string[] defaultsounds = { "sounds\\cursor1.wav", "sounds\\cursor2.wav", "sounds\\cursor3.wav", "sounds\\cursor4.wav" };
        private PPDSetting(bool isDebug, string songDir, string langIso, bool disableExpansion, bool disableShader)
        {
            SongDir = "songs";
            FontName = "IPAGothic";
            LangISO = "jp";
            FontSizeRatio = 30;
            BaseSize = 32;
            CoolArea = 0.04f;
            GoodArea = 0.08f;
            SafeArea = 0.12f;
            SadArea = 0.15f;
            if (File.Exists(settingFileName))
            {
                var sr = new StreamReader(settingFileName);
                var s = sr.ReadToEnd();
                sr.Close();
                var setting = new SettingReader(s);
                MultiSample = (MultisampleType)GetValue(setting.ReadString("multisample"), 0);
                SkinName = setting.ReadString("skinname");
                AdjustGapTime = GetValue(setting.ReadString("adjustgaptime"), 0);
                FontName = setting.ReadString("fontname");
                FontSizeRatio = (int)GetValue(setting.ReadString("fontsize"), 32);
                Width = (int)GetValue(setting.ReadString("width"), 800);
                Height = (int)GetValue(setting.ReadString("height"), 450);
                MovieLatency = GetValue(setting.ReadString("movielatency"), 0);
                MoviePlayType = (MoviePlayType)((int)GetValue(setting.ReadString("movieplaytype"), 0));
                Token = setting.ReadString("token");
                TokenSecret = setting.ReadString("tokensecret");
                SongDir = setting.ReadString("songdir");
                LangISO = setting.ReadString("Language");
                TextBoxDisabled = setting.ReadString("textboxdisabled") == "1";
                DrawSameColorAtSameTimingDisabled = setting.ReadString("drawsamecoloratsametimingdisabled") == "1";
                FullScreen = setting.ReadString("fullscreen") == "1";
                if (SongDir == "")
                {
                    SongDir = "songs";
                }
                MovieExtensions = setting.ReadString("movieextensions").Split(';');
                MusicExtensions = setting.ReadString("musicextensions").Split(';');
                AllowedToUseMuchMemory = setting.ReadString("allowedtousemuchmemory") == "1";
                IsDebug = setting.ReadString("IsDebug") == "1";
                DrawConnectUnderAllMark = setting.ReadString("drawconnectunderallmark") == "1";
                EveryFramePollingDisabled = setting.ReadString("everyframepollingdisabled") == "1";
                MenuMoviePreviewDisabled = setting.ReadString("menumoviepreviewdisabled") == "1";
                AccurateInputSleepTime = (int)GetValue(setting.ReadString("accurateinputsleeptime"), 0);
                CollectLog = setting.ReadString("collectlog") == "1";
                RunExpansion = setting.ReadString("runexpansion") == "1";
                ExpansionWaitPort = setting.ReadInt("expansionwaitport", 54320);
                AutoAdjustLatencyDisabled = setting.ReadBoolean("autoadjustlatencydisabled");
                ShaderDisabled = setting.ReadBoolean("shaderdisabled");
                FontScaleDisabled = setting.ReadBoolean("fontscaledisabled");
                HighResolutionImageDisabled = setting.ReadBoolean("highresolutionimagedisabled");
                TextureCharMode = setting.ReadEnum<TextureCharMode>("texturecharmode", TextureCharMode.D2D);
                FixedFPSDisabled = setting.ReadString("fixedfpsdisabled") == "1";
            }
            else
            {
                MovieExtensions = new string[0];
                MusicExtensions = new string[0];
            }
            MovieExtensions = MovieExtensions.Select(e => e.ToLower()).ToArray();
            MusicExtensions = MusicExtensions.Select(e => e.ToLower()).ToArray();
            if (isDebug)
            {
                IsDebug = true;
                if (Directory.Exists(songDir))
                {
                    SongDir = songDir;
                }
            }
            RunExpansion &= !disableExpansion;
            ShaderDisabled |= disableShader;
            if (langIso != null)
            {
                LangISO = langIso;
            }
        }

        /// <summary>
        /// 対象ファイルが動画かどうかを返します。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsMovie(string path)
        {
            var extension = Path.GetExtension(path).ToLower();
            return Array.IndexOf(MovieExtensions, extension.Length == 0 ? "" : extension.Substring(1)) >= 0;
        }

        private float GetValue(string word, float errorvalue)
        {
            if (!float.TryParse(word, NumberStyles.Float, CultureInfo.InvariantCulture, out float ret)) ret = errorvalue;
            return ret;
        }

        /// <summary>
        /// 文字サイズを調整します
        /// </summary>
        /// <param name="height">高さ</param>
        /// <returns></returns>
        public float GetAdjustedFontSize(float height)
        {
            return Setting.FontSizeRatio * height / Setting.BaseSize;
        }

        /// <summary>
        /// 設定
        /// </summary>
        public static PPDSetting Setting
        {
            get
            {
                if (setting == null)
                {
                    Initialize(false, "", null, false);
                }
                return setting;
            }
        }

        /// <summary>
        /// 現在の生の設定を取得します。
        /// </summary>
        public string CurrentRaw
        {
            get
            {
                return String.Join("\n", typeof(PPDSetting).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.Name != nameof(CurrentRaw)).Select(p =>
               {
                   return $"[{p.Name}]{p.GetValue(this)}";
               }));
            }
        }

        /// <summary>
        /// デバグかどうか
        /// </summary>
        public bool IsDebug
        {
            get;
            private set;
        }

        /// <summary>
        /// デフォルトのサウンド
        /// 0:move,1:decide,2:cancel,3:toggle
        /// </summary>
        public static string[] DefaultSounds
        {
            get { return defaultsounds; }
        }

        /// <summary>
        /// フォント名
        /// </summary>
        public string FontName
        {
            get;
            private set;
        }

        /// <summary>
        /// フォントサイズ比
        /// </summary>
        public int FontSizeRatio
        {
            get;
            private set;
        }

        /// <summary>
        /// ベースサイズ
        /// </summary>
        public int BaseSize
        {
            get;
            private set;
        }

        /// <summary>
        /// 高精度入力時のスリープ時間
        /// </summary>
        public int AccurateInputSleepTime
        {
            get;
            private set;
        }

        /// <summary>
        /// クールエリア
        /// </summary>
        public float CoolArea
        {
            get;
            private set;
        }

        /// <summary>
        /// グッドエリア
        /// </summary>
        public float GoodArea
        {
            get;
            private set;
        }

        /// <summary>
        /// セーフエリア
        /// </summary>
        public float SafeArea
        {
            get;
            private set;
        }

        /// <summary>
        /// サッドエリア
        /// </summary>
        public float SadArea
        {
            get;
            private set;
        }

        /// <summary>
        /// モニタ遅延調整
        /// </summary>
        public float AdjustGapTime
        {
            get;
            private set;
        }

        /// <summary>
        /// マルチサンプル
        /// </summary>
        public MultisampleType MultiSample
        {
            get;
            private set;
        }

        /// <summary>
        /// スキン名
        /// </summary>
        public string SkinName
        {
            get;
            private set;
        }

        /// <summary>
        /// ウィンドウ幅
        /// </summary>
        public int Width
        {
            get;
            private set;
        }

        /// <summary>
        /// ウィンドウ高さ
        /// </summary>
        public int Height
        {
            get;
            private set;
        }

        /// <summary>
        /// 動画遅延
        /// </summary>
        public float MovieLatency
        {
            get;
            private set;
        }

        /// <summary>
        /// 動画再生タイプ
        /// </summary>
        public MoviePlayType MoviePlayType
        {
            get;
            private set;
        }

        /// <summary>
        /// 検索用のテキストボックスが無効化されているか
        /// </summary>
        public bool TextBoxDisabled
        {
            get;
            private set;
        }

        /// <summary>
        /// 同じ時間に押されるマークを同じ色にしないか
        /// </summary>
        public bool DrawSameColorAtSameTimingDisabled
        {
            get;
            private set;
        }

        /// <summary>
        /// 言語設定
        /// </summary>
        public string LangISO
        {
            get;
            private set;
        }

        private string Token
        {
            get;
            set;
        }

        private string TokenSecret
        {
            get;
            set;
        }

        /// <summary>
        /// 譜面のディレクトリです
        /// </summary>
        public string SongDir
        {
            get;
            private set;
        }

        /// <summary>
        /// フルスクリーンかどうか
        /// </summary>
        public bool FullScreen
        {
            get;
            private set;
        }

        /// <summary>
        /// ログを集めるかどうか
        /// </summary>
        public bool CollectLog
        {
            get;
            private set;
        }

        /// <summary>
        /// 全てのマークの下にコネクトを描画するかどうか
        /// </summary>
        public bool DrawConnectUnderAllMark
        {
            get;
            private set;
        }

        /// <summary>
        /// PPDExpansionを起動するかどうか
        /// </summary>
        public bool RunExpansion
        {
            get;
            private set;
        }

        /// <summary>
        /// PPDExpansionの待機ポート
        /// </summary>
        public int ExpansionWaitPort
        {
            get;
            private set;
        }

        internal bool AllowedToUseMuchMemory
        {
            get;
            set;
        }

        internal string[] MovieExtensions
        {
            get;
            private set;
        }

        internal string[] MusicExtensions
        {
            get;
            private set;
        }

        /// <summary>
        /// 毎フレームのポーリングが無効化されたかどうかを取得します。
        /// </summary>
        public bool EveryFramePollingDisabled
        {
            get;
            private set;
        }

        /// <summary>
        /// メニューでの動画の再生を無効化されたかどうかを取得します。
        /// </summary>
        public bool MenuMoviePreviewDisabled
        {
            get;
            private set;
        }

        /// <summary>
        /// 自動レンテンシー調整が無効化されているかどうかを取得します。
        /// </summary>
        public bool AutoAdjustLatencyDisabled
        {
            get;
            private set;
        }

        /// <summary>
        /// シェーダを無効化するかどうかを取得、設定します。
        /// </summary>
        public bool ShaderDisabled
        {
            get;
            set;
        }

        /// <summary>
        /// 高解像度画像が無効化されているかどうかを取得します。
        /// </summary>
        public bool HighResolutionImageDisabled
        {
            get;
            private set;
        }

        /// <summary>
        /// 60FPS固定が無効化されているかどうかをしゅとくします。
        /// </summary>
        public bool FixedFPSDisabled
        {
            get;
            private set;
        }

        internal bool FontScaleDisabled
        {
            get;
            private set;
        }

        internal TextureCharMode TextureCharMode
        {
            get;
            private set;
        }

        internal bool CharacterTexturePackingDisabled
        {
            get;
            private set;
        } = false;

        /// <summary>
        /// 初期化を行います。
        /// </summary>
        /// <param name="isDebug">デバッグかどうか。</param>
        /// <param name="songDir">譜面フォルダ。</param>
        /// <param name="langIso">言語。</param>
        /// <param name="disableExpansion">Expansionを無効化するかどうか。</param>
        /// <param name="disableShader">シェーダーを無効化するかどうか。</param>
        public static void Initialize(bool isDebug = false, string songDir = "", string langIso = null, bool disableExpansion = false, bool disableShader = false)
        {
            setting = new PPDSetting(isDebug, songDir, langIso, disableExpansion, disableShader);
        }
    }
}
