using PPDEditor.Command.PPDSheet;
using System.Drawing;

namespace PPDEditor
{
    class TimeLineDrawParameter
    {
        public Graphics Graphics
        {
            get;
            private set;
        }

        public int Width
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }

        public Image BackGroundImage
        {
            get;
            private set;
        }

        public PPDSheet Sheet
        {
            get;
            private set;
        }

        public float BPM
        {
            get;
            private set;
        }

        public float BPMStart
        {
            get;
            private set;
        }

        public int Interval
        {
            get;
            private set;
        }

        public DisplayLineMode DisplayMode
        {
            get;
            private set;
        }

        public int LeftOffset
        {
            get;
            private set;
        }

        public float CurrentTime
        {
            get;
            private set;
        }

        public DisplayBeatType BeatType
        {
            get;
            private set;
        }

        public int BeatSplitCount
        {
            get;
            private set;
        }

        public TimeLineDrawParameter(Graphics graphics, int width, int height, Image backGroundImage, PPDSheet sheet, float bpm,
            float bpmStart, int interval, DisplayLineMode displayMode, int leftOffset, float currentTime, DisplayBeatType beatType)
        {
            Graphics = graphics;
            Width = width;
            Height = height;
            BackGroundImage = backGroundImage;
            Sheet = sheet;
            BPM = bpm;
            BPMStart = bpmStart;
            Interval = interval;
            DisplayMode = displayMode;
            LeftOffset = leftOffset;
            CurrentTime = currentTime;
            BeatType = beatType;
            BeatSplitCount = (int)beatType + 2;
        }
    }
}
