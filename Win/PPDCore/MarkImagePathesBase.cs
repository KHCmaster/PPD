using PPDFramework;

namespace PPDCore
{
    /// <summary>
    /// マーク郡のイメージパスを取得するクラス
    /// 必ずpublic実装する必要があります
    /// </summary>
    public abstract class MarkImagePathsBase
    {
        /// <summary>
        /// カラーのマーク画像のパスを取得する
        /// </summary>
        /// <param name="buttontype">ボタンタイプ</param>
        /// <returns></returns>
        public abstract PathObject GetMarkColorImagePath(ButtonType buttontype);

        /// <summary>
        /// マーク画像のパスを取得する
        /// </summary>
        /// <param name="buttontype">ボタンタイプ</param>
        /// <returns></returns>
        public abstract PathObject GetMarkImagePath(ButtonType buttontype);

        /// <summary>
        /// マークのトレース（軌跡）の画像のパスを取得する
        /// </summary>
        /// <param name="buttontype">ボタンタイプ</param>
        /// <returns></returns>
        public abstract PathObject GetTraceImagePath(ButtonType buttontype);

        /// <summary>
        /// 長押しの円情報を取得する
        /// </summary>
        /// <param name="imagepath">画像パス</param>
        /// <param name="innerradius">内径</param>
        /// <param name="outerradius">外径</param>
        public abstract void GetLongNoteCircleInfo(out PathObject imagepath, out float innerradius, out float outerradius);

        /// <summary>
        /// 長押しの円の軸の画像のパスを取得する
        /// </summary>
        /// <returns></returns>
        public abstract PathObject GetCircleAxisImagePath();

        /// <summary>
        /// 普通の軸の画像のパスを取得する
        /// </summary>
        /// <returns></returns>
        public abstract PathObject GetClockAxisImagePath();

        /// <summary>
        /// マークの評価時のエフェクトパスを取得する
        /// </summary>
        /// <param name="evatype">評価</param>
        /// <returns></returns>
        public abstract PathObject GetEvaluateEffectPath(MarkEvaluateType evatype);

        /// <summary>
        /// マークが現れた時のエフェクトパスを取得する
        /// </summary>
        /// <returns></returns>
        public abstract PathObject GetAppearEffectPath();

        /// <summary>
        /// スライドのエフェクトパスを取得する
        /// </summary>
        /// <returns></returns>
        public abstract PathObject GetSlideEffectPath();

        /// <summary>
        /// HOLDの画像情報を取得する
        /// </summary>
        /// <returns></returns>
        public abstract void GetHoldInfo(out PathObject imagepath, out float x, out float y);
    }
}
