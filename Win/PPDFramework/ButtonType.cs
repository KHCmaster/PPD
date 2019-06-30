using System;

namespace PPDFramework
{
    /// <summary>
    /// ボタンタイプ
    /// </summary>
    public enum ButtonType
    {
        /// <summary>
        /// スクエア
        /// </summary>
        Square = 0,
        /// <summary>
        /// クロス
        /// </summary>
        Cross = 1,
        /// <summary>
        /// サークル
        /// </summary>
        Circle = 2,
        /// <summary>
        /// トライアングル
        /// </summary>
        Triangle = 3,
        /// <summary>
        /// 左
        /// </summary>
        Left = 4,
        /// <summary>
        /// 下
        /// </summary>
        Down = 5,
        /// <summary>
        /// 右
        /// </summary>
        Right = 6,
        /// <summary>
        /// 上
        /// </summary>
        Up = 7,
        /// <summary>
        /// R
        /// </summary>
        R = 8,
        /// <summary>
        /// L
        /// </summary>
        L = 9,
        /// <summary>
        /// スタート
        /// </summary>
        Start = 10,
        /// <summary>
        /// ホームボタン
        /// </summary>
        Home = 11,
    }

    /// <summary>
    /// ボタンユーティリティ
    /// </summary>
    public static class ButtonUtility
    {
        /// <summary>
        /// ボタンの配列
        /// </summary>
        public static ButtonType[] Array = (ButtonType[])Enum.GetValues(typeof(ButtonType));
    }
}
