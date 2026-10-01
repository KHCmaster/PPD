using System;
using System.Collections.Generic;
namespace PPDFramework.Web
{
    /// <summary>
    /// アイテムの情報を扱うクラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクターです。
    /// </remarks>
    /// <param name="itemId">アイテムのID。</param>
    /// <param name="itemType">アイテムのタイプ。</param>
    public class ItemInfo(int itemId, ItemType itemType)
    {
        private readonly ItemType[] availableItems = [ItemType.Auto1, ItemType.Auto2, ItemType.Auto3, ItemType.Auto4, ItemType.AutoFreePass];
        private readonly Dictionary<string, object> parameters = [];

        /// <summary>
        /// アイテムのIDを取得します。
        /// </summary>
        public int ItemId
        {
            get;
            private set;
        } = itemId;

        /// <summary>
        /// アイテムのタイプを取得します。
        /// </summary>
        public ItemType ItemType
        {
            get;
            private set;
        } = itemType;

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
