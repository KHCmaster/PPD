namespace PPDFramework.PPDStructure.PPDData
{
    /// <summary>
    /// マークデータクラス
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="angle">回転角</param>
    /// <param name="time">時間</param>
    /// <param name="buttontype">ボタンタイプ</param>
    /// <param name="id">ID</param>
    public class MarkData(float x, float y, float angle, float time, ButtonType buttontype, uint id) : MarkDataBase(x, y, angle, time, buttontype, id)
    {
    }
}
