namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// ノーツタイプ切り替えイベントです
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="time"></param>
    /// <param name="noteType"></param>
    public class ChangeNoteTypeEvent(float time, NoteType noteType) : IEVDData(time, EventType.ChangeNoteType)
    {

        /// <summary>
        /// ノーツの種類
        /// </summary>
        public NoteType NoteType
        {
            get;
            private set;
        } = noteType;
    }
}
