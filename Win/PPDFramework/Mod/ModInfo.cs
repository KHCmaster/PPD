using FlowScriptEngine;
using PPDFramework.Web;
using PPDFrameworkCore;
using PPDPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PPDFramework.Mod
{
    /// <summary>
    /// MODの情報を表すクラスです
    /// </summary>
    public class ModInfo : ModInfoBase
    {
        private ModManager modManager;
        private List<ModSetting> settings;
        private ModSettingManager modSettingManager;
        private WebModInfo matchedWebModInfo;
        private WebModInfoDetail matchedWebModInfoDetail;
        private Guid guid;
        private bool isApplied;

        /// <summary>
        /// ファイル名を取得します
        /// </summary>
        public string FileName
        {
            get
            {
                return Path.GetFileName(ModPath);
            }
        }

        /// <summary>
        /// ユニーク名を取得します。
        /// </summary>
        public string UniqueName
        {
            get
            {
                return String.Format("{0}.{1}", AuthorName, DisplayName);
            }
        }

        /// <summary>
        /// 表示名を取得します
        /// </summary>
        public string DisplayName
        {
            get;
            private set;
        }

        /// <summary>
        /// 作者名を取得します
        /// </summary>
        public string AuthorName
        {
            get;
            private set;
        }

        /// <summary>
        /// バージョンを取得します
        /// </summary>
        public string Version
        {
            get;
            private set;
        }

        /// <summary>
        /// FlowScriptのバージョンを取得します
        /// </summary>
        public Version FlowScriptVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// FlowScriptBasicのバージョンを取得します
        /// </summary>
        public Version FlowScriptBasicVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// FlowScriptPPDのバージョンを取得します
        /// </summary>
        public Version FlowScriptPPDVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// FlowScriptSharpDXのバージョンを取得します
        /// </summary>
        public Version FlowScriptSharpDXVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// FlowScriptBasicExtensionのバージョンを取得します。
        /// </summary>
        public Version FlowScriptBasicExtensionVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// FlowScriptDataのバージョンを取得します。
        /// </summary>
        public Version FlowScriptDataVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// ファイルハッシュを取得します
        /// </summary>
        public byte[] FileHash
        {
            get;
            private set;
        }

        /// <summary>
        /// ファイルハッシュの文字列を取得します。
        /// </summary>
        public string FileHashString
        {
            get;
            private set;
        }

        /// <summary>
        /// データを変更するノードが含まれているかどうかを取得します。
        /// </summary>
        public bool ContainsModifyData
        {
            get;
            private set;
        }

        /// <summary>
        /// 設定を取得します。
        /// </summary>
        public ModSetting[] Settings
        {
            get
            {
                return settings.ToArray();
            }
        }

        /// <summary>
        /// ModSettingManagerを取得します。
        /// </summary>
        public ModSettingManager ModSettingManager
        {
            get
            {
                return modSettingManager;
            }
        }

        /// <summary>
        /// 更新可能かどうかを取得します。
        /// </summary>
        public bool CanUpdate
        {
            get
            {
                return matchedWebModInfo != null && matchedWebModInfoDetail != null &&
                    matchedWebModInfo.Revision > matchedWebModInfoDetail.Revision;
            }
        }

        /// <summary>
        /// GUIDを取得します。
        /// </summary>
        public Guid Guid
        {
            get { return guid; }
        }

        /// <summary>
        /// 適用するかどうかを取得、設定します。
        /// </summary>
        public bool IsApplied
        {
            get { return isApplied; }
            set
            {
                if (isApplied != value)
                {
                    isApplied = value;
                    ModSettingDatabase.Setting.SetIsApplied(UniqueName, isApplied);
                }
            }
        }

        /// <summary>
        /// 更新が終了したときのイベントです。
        /// </summary>
        public event Action UpdateFinished;

        internal ModInfo(string path, ModManager modManager, WebModInfo[] webModInfos)
            : base(path, false)
        {
            settings = new List<ModSetting>();
            this.guid = Guid.NewGuid();
            this.modManager = modManager;
            UpdateInfo();
            UpdateCanUpdate(webModInfos);
            isApplied = ModSettingDatabase.Setting.GetIsApplied(UniqueName);
            IsApplied |= CanApply();
        }

        private void UpdateInfo()
        {
            using (PackReader reader = new PackReader(ModPath))
            {
                DisplayName = ReadString(reader, "Mod\\DisplayName");
                AuthorName = ReadString(reader, "Mod\\AuthorName");
                Version = ReadString(reader, "Mod\\Version");
                FlowScriptVersion = ReadVersion(reader, "Mod\\FlowScriptVersion");
                FlowScriptBasicVersion = ReadVersion(reader, "Mod\\FlowScriptBasicVersion");
                FlowScriptPPDVersion = ReadVersion(reader, "Mod\\FlowScriptPPDVersion");
                FlowScriptSharpDXVersion = ReadVersion(reader, "Mod\\FlowScriptSharpDXVersion");
                FlowScriptBasicExtensionVersion = ReadVersion(reader, "Mod\\FlowScriptBasicExtensionVersion");
                FlowScriptDataVersion = ReadVersion(reader, "Mod\\FlowScriptDataVersion");
                AnalyzeMod(reader);
            }
            using (FileStream fs = File.Open(ModPath, FileMode.Open))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                FileHash = CryptographyUtility.CalcSha256Hash(data);
                FileHashString = CryptographyUtility.Getx2Encoding(FileHash);
            }
        }

        private void AnalyzeMod(PackReader reader)
        {
            var engine = new Engine();
            modSettingManager = new ModSettingManager();
            foreach (string content in reader.FileList)
            {
                using (PPDPackStreamReader ppdpsr = reader.Read(content))
                {
                    if (content.StartsWith("Scripts"))
                    {
                        var manager = engine.Load(ppdpsr, false);
                        manager.Items["ModSettingManager"] = modSettingManager;
                        manager.Initialize();
                    }
                }
            }

            ContainsModifyData = engine.ContainsAttribute(typeof(ModifyDataAttribute));
            engine.Call(ModSettingManager.MODSTART, null);

            settings.Clear();
            settings.AddRange(modSettingManager.ModSettings);

            var dict = new Dictionary<string, ModSetting>();
            foreach (ModSetting setting in settings)
            {
                if (!dict.ContainsKey(setting.Key))
                {
                    dict.Add(setting.Key, setting);
                }
            }
            settings.Clear();
            settings.AddRange(dict.Values);

            var currentSettings = ModSettingDatabase.Setting.GetSetting(UniqueName);
            foreach (ModSetting modSetting in settings)
            {
                if (!currentSettings.ContainsKey(modSetting.Key))
                {
                    currentSettings.Add(modSetting.Key, modSetting.Default);
                }
                else
                {
                    try
                    {
                        var str = modSetting.GetStringValue(currentSettings[modSetting.Key]);
                        if (!modSetting.Validate(str, out object val))
                        {
                            throw new Exception("Validate Failed");
                        }
                    }
                    catch
                    {
                        currentSettings.Remove(modSetting.Key);
                        currentSettings.Add(modSetting.Key, modSetting.Default);
                    }
                }
            }

            ModSettingDatabase.Setting.SetSetting(UniqueName, currentSettings);
            foreach (KeyValuePair<string, object> pair in currentSettings)
            {
                ModSettingManager[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// 設定を保存します。
        /// </summary>
        public void SaveSetting()
        {
            ModSettingDatabase.Setting.SetSetting(UniqueName,
                ModSettingManager.Settings.ToDictionary(pair => pair.Key, pair => pair.Value));
        }

        /// <summary>
        /// 適用可能なMODかどうかを返します
        /// </summary>
        /// <returns></returns>
        public bool CanApply()
        {
            return modManager.FlowScriptVersion >= FlowScriptVersion &&
                modManager.FlowScriptBasicVersion >= FlowScriptBasicVersion &&
                modManager.FlowScriptPPDVersion >= FlowScriptPPDVersion &&
                modManager.FlowScriptSharpDXVersion >= FlowScriptSharpDXVersion &&
                modManager.FlowScriptBasicExtensionVersion >= FlowScriptBasicExtensionVersion &&
                modManager.FlowScriptDataVersion >= FlowScriptDataVersion;
        }

        /// <summary>
        /// 読み込み後に変更されていないかどうかを取得します
        /// </summary>
        /// <returns></returns>
        public bool NotModified()
        {
            using (FileStream fs = File.Open(ModPath, FileMode.Open))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                var SHA256 = new SHA256Managed();
                var hash = SHA256.ComputeHash(data);
                return CompareArray(hash, FileHash);
            }
        }

        /// <summary>
        /// Modを更新します。
        /// </summary>
        public void Update()
        {
            try
            {
                if (matchedWebModInfo != null)
                {
                    var tempPath = Path.GetTempFileName();
#if DEBUG
                    WebManager.Instance.DownloadFile(matchedWebModInfo, tempPath, @"http://projectdxxx.me");
#else
                    WebManager.Instance.DownloadFile(matchedWebModInfo, tempPath);
#endif

                    if (File.Exists(tempPath))
                    {
                        File.Copy(tempPath, ModPath, true);
                        UpdateInfo();
                        matchedWebModInfoDetail = UpdateCanUpdate(matchedWebModInfo);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                OnUpdateFinished();
            }
        }

        private void OnUpdateFinished()
        {
            UpdateFinished?.Invoke();
        }

        private void UpdateCanUpdate(WebModInfo[] infos)
        {
            foreach (var info in infos)
            {
                matchedWebModInfoDetail = UpdateCanUpdate(info);
                if (matchedWebModInfoDetail != null)
                {
                    matchedWebModInfo = info;
                    break;
                }
            }
        }

        private WebModInfoDetail UpdateCanUpdate(WebModInfo info)
        {
            return info.Details.FirstOrDefault(d => CompareArray(d.HashAsBytes, FileHash));
        }

        private bool CompareArray(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }

        private string ReadString(PackReader reader, string key, string defaultValue = "")
        {
            using (PPDPackStreamReader packReader = reader.Read(key))
            {
                if (packReader == null)
                {
                    return defaultValue;
                }
                byte[] bytes = new byte[packReader.Length];
                packReader.Read(bytes, 0, bytes.Length);
                return Encoding.UTF8.GetString(bytes);
            }
        }

        private Version ReadVersion(PackReader reader, string key)
        {
            var str = ReadString(reader, key);
            if (String.IsNullOrEmpty(str))
            {
                return new Version("0.0.0.0");
            }
            return new Version(str);
        }
    }
}
