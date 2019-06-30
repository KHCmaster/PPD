using PPDFramework.Mod;
using PPDFramework.Web;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDFramework
{
    /// <summary>
    /// PPDGameユーティリティ
    /// </summary>
    public class PPDGameUtility
    {
        List<ItemInfo> items;
        ModInfo[] appliedMods;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PPDGameUtility()
        {
            items = new List<ItemInfo>();
        }

        /// <summary>
        /// 譜面情報
        /// </summary>
        public SongInformation SongInformation
        {
            get;
            set;
        }

        /// <summary>
        /// 難易度
        /// </summary>
        public Difficulty Difficulty
        {
            get;
            set;
        }

        /// <summary>
        /// 難易度文字列
        /// </summary>
        public string DifficultString
        {
            get;
            set;
        }

        /// <summary>
        /// プロファイル
        /// </summary>
        public Profile Profile
        {
            get;
            set;
        }

        /// <summary>
        /// オートモード
        /// </summary>
        public AutoMode AutoMode
        {
            get;
            set;
        }

        /// <summary>
        /// ランダム
        /// </summary>
        public bool Random
        {
            get;
            set;
        }

        /// <summary>
        /// スピードスケール
        /// </summary>
        public float SpeedScale
        {
            get;
            set;
        }

        /// <summary>
        /// 効果音なし
        /// </summary>
        public bool MuteSE
        {
            get;
            set;
        }

        /// <summary>
        /// 同時押しを結ぶか
        /// </summary>
        public bool Connect
        {
            get;
            set;
        }

        /// <summary>
        /// デバッグかどうか
        /// </summary>
        public bool IsDebug
        {
            get;
            set;
        }

        /// <summary>
        /// ゴッドモード(死なない)かどうか
        /// </summary>
        public bool GodMode
        {
            get;
            set;
        }

        /// <summary>
        /// ライバルゴーストを使うかどうか
        /// </summary>
        public bool RivalGhost
        {
            get;
            set;
        }

        /// <summary>
        /// ライバルゴーストの数
        /// </summary>
        public int RivalGhostCount
        {
            get;
            set;
        }

        /// <summary>
        /// 標準プロパティかどうか
        /// !IsDebug  !Auto  !Random  Profile.Index == 0;
        /// </summary>
        public bool IsRegular
        {
            get
            {
                return !GodMode && !IsDebug && AutoMode == AutoMode.None && !Random && Profile.Index == 0 && Difficulty != Difficulty.Other && NotModifyMod;
            }
        }

        /// <summary>
        /// データを変更しないModかどうか
        /// </summary>
        public bool NotModifyMod
        {
            get
            {
                if (AppliedMods == null)
                {
                    return true;
                }

                foreach (ModInfo modInfo in AppliedMods)
                {
                    if (modInfo.ContainsModifyData)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// 適用されるMODを取得します
        /// </summary>
        public ModInfo[] AppliedMods
        {
            get
            {
                if (appliedMods == null)
                {
                    var ret = new List<ModInfo>();
                    ModManager.Instance.WaitForLoadFinish();
                    if (ModManager.Instance.Root != null)
                    {
                        appliedMods = ModManager.Instance.Root.Descendants().OfType<ModInfo>().Where(m => m.IsApplied).ToArray();
                    }
                }
                return appliedMods;
            }
            set
            {
                appliedMods = value;
            }
        }

        /// <summary>
        /// ランキングを更新するための関数を取得、設定します。
        /// </summary>
        public Func<Ranking> RankingUpdateFunc
        {
            get;
            set;
        }

        /// <summary>
        /// ライバルのランキングを更新するための関数を取得、設定します。
        /// </summary>
        public Func<Ranking> RivalRankingUpdateFunc
        {
            get;
            set;
        }

        /// <summary>
        /// レンテンシのタイプを取得、設定します。
        /// </summary>
        public LatencyType LatencyType
        {
            get;
            set;
        }

        /// <summary>
        /// アイテムリストを取得します。
        /// </summary>
        public ItemInfo[] Items
        {
            get { return items.ToArray(); }
        }

        /// <summary>
        /// 使用するアイテムを追加します。
        /// </summary>
        /// <param name="item">アイテム。</param>
        public void AddItem(ItemInfo item)
        {
            items.Add(item);
        }

        /// <summary>
        /// パーフェクトトライアルをするかどうか。
        /// </summary>
        public bool PerfectTrial
        {
            get;
            set;
        }

        /// <summary>
        /// リプレイするリザルトID。
        /// </summary>
        public int ReplayResultId
        {
            get;
            set;
        }

        /// <summary>
        /// 適用可能なModかどうかを返すコールバック。
        /// </summary>
        public Func<ModInfo, bool> CanApplyModCallback
        {
            get;
            set;
        }

        /// <summary>
        /// 適用可能なModかどうかを返します。
        /// </summary>
        /// <param name="modInfo"></param>
        /// <returns></returns>
        public bool CanApplyMod(ModInfo modInfo)
        {
            if (CanApplyModCallback != null)
            {
                return CanApplyModCallback(modInfo);
            }
            return true;
        }
    }
}
