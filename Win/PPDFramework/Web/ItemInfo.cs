using System;
using System.Collections.Generic;
namespace PPDFramework.Web
{
    /// <summary>
    /// アイテムの情報を扱うクラスです。
    /// </summary>
    public class ItemInfo
    {
        private ItemType[] availableItems = { ItemType.Auto1, ItemType.Auto2, ItemType.Auto3, ItemType.Auto4, ItemType.AutoFreePass };
        private Dictionary<string, object> parameters;

        /// <summary>
        /// アイテムのIDを取得します。
        /// </summary>
        public int ItemId
        {
            get;
            private set;
        }

        /// <summary>
        /// アイテムのタイプを取得します。
        /// </summary>
        public ItemType ItemType
        {
            get;
            private set;
        }

        /// <summary>
        /// 使用したかどうかを取得します。
        /// </summary>
        public bool IsUsed
        {
            get;
            internal set;
        }

        /// <summary>
        /// 利用可能かどうかを取得します。
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return Array.IndexOf(availableItems, ItemType) >= 0;
            }
        }

        /// <summary>
        /// パラメータを取得します。
        /// </summary>
        /// <param name="paramName">パラメータ名。</param>
        /// <returns>パラメータ。</returns>
        public object this[string paramName]
        {
            get { return parameters[paramName]; }
            set { parameters[paramName] = value; }
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        /// <param name="itemId">アイテムのID。</param>
        /// <param name="itemType">アイテムのタイプ。</param>
        public ItemInfo(int itemId, ItemType itemType)
        {
            ItemId = itemId;
            ItemType = itemType;
            parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// パラメータを含むかどうかを返します。
        /// </summary>
        /// <param name="paramName">パラメータ名。</param>
        /// <returns>パラメータ。</returns>
        public bool ContainsParameter(string paramName)
        {
            return parameters.ContainsKey(paramName);
        }
    }
}
