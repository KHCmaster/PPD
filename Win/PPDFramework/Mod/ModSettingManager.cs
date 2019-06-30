using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDFramework.Mod
{
    /// <summary>
    /// Modの設定を扱うクラスです。
    /// </summary>
    public class ModSettingManager
    {
        /// <summary>
        /// Modの設定の初期化時に使用する関数名です。
        /// </summary>
        public const string MODSTART = "PPD_MOD_START";

        Dictionary<string, object> settings;
        List<ModSetting> modSettings;

        /// <summary>
        /// MODの設定値を取得、設定します。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                return settings[key];
            }
            set
            {
                settings[key] = value;
            }
        }

        /// <summary>
        /// Modの設定を取得します。
        /// </summary>
        public ModSetting[] ModSettings
        {
            get
            {
                return modSettings.ToArray();
            }
        }

        /// <summary>
        /// 設定のリストを取得します。
        /// </summary>
        public KeyValuePair<string, object>[] Settings
        {
            get
            {
                return settings.ToArray();
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public ModSettingManager()
        {
            settings = new Dictionary<string, object>();
            modSettings = new List<ModSetting>();
        }

        /// <summary>
        /// 文字列の設定を追加します。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        public void AddStringSetting(string key, string name, string description, string defaultValue)
        {
            AddStringSetting(key, name, description, defaultValue, Int32.MaxValue);
        }

        /// <summary>
        /// 文字列の設定を追加します。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <param name="maxLength"></param>
        public void AddStringSetting(string key, string name, string description, string defaultValue, int maxLength)
        {
            modSettings.Add(new StringModSetting(key, name, description, defaultValue, maxLength));
        }

        /// <summary>
        /// 整数の設定を追加します。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        public void AddInt32Setting(string key, string name, string description, int defaultValue)
        {
            AddInt32Setting(key, name, description, defaultValue, Int32.MinValue, Int32.MaxValue);
        }

        /// <summary>
        /// 整数の設定を追加します。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public void AddInt32Setting(string key, string name, string description, int defaultValue, int minValue, int maxValue)
        {
            modSettings.Add(new Int32ModSetting(key, name, description, defaultValue, minValue, maxValue));
        }

        /// <summary>
        /// 単精度浮動小数の設定を追加します。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        public void AddFloatSetting(string key, string name, string description, float defaultValue)
        {
            AddFloatSetting(key, name, description, defaultValue, float.MinValue, float.MaxValue);
        }

        /// <summary>
        /// 単精度浮動小数の設定を追加します。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public void AddFloatSetting(string key, string name, string description, float defaultValue, float minValue, float maxValue)
        {
            modSettings.Add(new FloatModSetting(key, name, description, defaultValue, minValue, maxValue));
        }

        /// <summary>
        /// 倍精度浮動小数の設定を追加します。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        public void AddDoubleSetting(string key, string name, string description, double defaultValue)
        {
            AddDoubleSetting(key, name, description, defaultValue, double.MinValue, double.MaxValue);
        }

        /// <summary>
        /// 倍精度浮動小数の設定を追加します。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public void AddDoubleSetting(string key, string name, string description, double defaultValue, double minValue, double maxValue)
        {
            modSettings.Add(new DoubleModSetting(key, name, description, defaultValue, minValue, maxValue));
        }

        /// <summary>
        /// 色の設定を追加します。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        public void AddColor4Setting(string key, string name, string description, Color4 defaultValue)
        {
            modSettings.Add(new Color4ModSetting(key, name, description, defaultValue));
        }

        /// <summary>
        /// 真偽値の設定を追加します。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        public void AddBooleanSetting(string key, string name, string description, bool defaultValue)
        {
            modSettings.Add(new BooleanModSetting(key, name, description, defaultValue));
        }
    }
}
