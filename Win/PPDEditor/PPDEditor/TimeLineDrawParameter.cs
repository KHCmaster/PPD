using PPDEditor.Command.PPDSheet;
using System.Drawing;

namespace PPDEditor
{
    class TimeLineDrawParameter(Graphics graphics, int width, int height, Image backGroundImage, PPDSheet sheet, float bpm,
        float bpmStart, int interval, DisplayLineMode displayMode, int leftOffset, float currentTime, DisplayBeatType beatType)
    {
        public Graphics Graphics
        {
            get;
            private set;
        } = graphics;

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

        public Image BackGroundImage
        {
            get;
            private set;
        } = backGroundImage;

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
