namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// ノーツタイプ切り替えイベントです
    /// </summary>
    public class ChangeNoteTypeEvent : IEVDData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time"></param>
        /// <param name="noteType"></param>
        public ChangeNoteTypeEvent(float time, NoteType noteType)
            : base(time, EventType.ChangeNoteType)
        {
            this.NoteType = noteType;
        }

        /// <summary>
        /// ノーツの種類
        /// </summary>
        public NoteType NoteType
        {
            get;
            private set;
        }
    }
}
