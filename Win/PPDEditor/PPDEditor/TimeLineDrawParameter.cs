using PPDEditor.Command.PPDSheet;
using System.Windows.Media;

namespace PPDEditor
{
    class TimeLineDrawParameter(DrawingContext drawingContext, int width, int height, Brush background, PPDSheet sheet, float bpm,
        float bpmStart, int interval, DisplayLineMode displayMode, int leftOffset, float currentTime, DisplayBeatType beatType)
    {
        public DrawingContext DrawingContext
        {
            get;
            private set;
        } = drawingContext;

        public int Width
        {
            get;
            private set;
        } = width;

        public int Height
        {
            get;
            private set;
        } = height;

        /// <summary>
        /// 全体を塗る背景です。
        /// </summary>
        public Brush Background
        {
            get;
            private set;
        } = background;

        public PPDSheet Sheet
        {
            get;
            private set;
        } = sheet;

        public float BPM
        {
            get;
            private set;
        } = bpm;

        public float BPMStart
        {
            get;
            private set;
        } = bpmStart;

        public int Interval
        {
            get;
            private set;
        } = interval;

        public DisplayLineMode DisplayMode
        {
            get;
            private set;
        } = displayMode;

        public int LeftOffset
        {
            get;
            private set;
        } = leftOffset;

        public float CurrentTime
        {
            get;
            private set;
        } = currentTime;

        public DisplayBeatType BeatType
        {
            get;
            private set;
        } = beatType;

        public int BeatSplitCount
        {
            get;
            private set;
        } = (int)beatType + 2;
    }
}
