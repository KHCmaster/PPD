using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using System;
using System.Text;

namespace PPDEditorCommon
{
    public class EventData
    {
        int[] volpercents = new int[11];
        bool[] keepplayings = new bool[11];
        bool[] releasesounds = new bool[11];
        public EventData()
        {
            InitializeOrder = new ButtonType[10];
            SetDefault();
        }
        public void SetVolume(int channel, int volpercent)
        {
            if (volpercent < 0) volpercent = 0;
            if (volpercent > 100) volpercent = 100;
            volpercents[channel] = volpercent;
        }
        public void SetKeepPlaying(int channel, bool keepPlaying)
        {
            keepplayings[channel] = keepPlaying;
        }
        public void SetReleaseSound(int channel, bool releaseSound)
        {
            releasesounds[channel] = releaseSound;
        }
        public EventData Clone()
        {
            var ret = new EventData();
            Array.Copy(volpercents, ret.volpercents, volpercents.Length);
            Array.Copy(keepplayings, ret.keepplayings, keepplayings.Length);
            Array.Copy(releasesounds, ret.releasesounds, releasesounds.Length);
            ret.BPM = BPM;
            ret.BPMRapidChange = BPMRapidChange;
            ret.DisplayState = DisplayState;
            ret.MoveState = MoveState;
            ret.NoteType = NoteType;
            ret.SlideScale = SlideScale;
            Array.Copy(InitializeOrder, ret.InitializeOrder, InitializeOrder.Length);
            return ret;
        }
        public void SetDefault()
        {
            for (int i = 0; i < volpercents.Length; i++)
            {
                volpercents[i] = 90;
            }
            for (int i = 0; i < keepplayings.Length; i++)
            {
                keepplayings[i] = false;
            }
            for (int i = 0; i < releasesounds.Length; i++)
            {
                releasesounds[i] = false;
            }
            BPM = 100;
            BPMRapidChange = false;
            DisplayState = DisplayState.Normal;
            MoveState = MoveState.Normal;
            NoteType = NoteType.Normal;
            SlideScale = 1;
            for (int i = 0; i < 10; i++)
            {
                InitializeOrder[i] = (ButtonType)i;
            }
        }
        public int MovieVolumePercent
        {
            get { return volpercents[0]; }
            set { volpercents[0] = value; }
        }
        public int SquareVolumePercent
        {
            get { return volpercents[1]; }
            set { volpercents[1] = value; }
        }
        public int CrossVolumePercent
        {
            get { return volpercents[2]; }
            set { volpercents[2] = value; }
        }
        public int CircleVolumePercent
        {
            get { return volpercents[3]; }
            set { volpercents[3] = value; }
        }
        public int TriangleVolumePercent
        {
            get { return volpercents[4]; }
            set { volpercents[4] = value; }
        }
        public int LeftVolumePercent
        {
            get { return volpercents[5]; }
            set { volpercents[5] = value; }
        }
        public int DownVolumePercent
        {
            get { return volpercents[6]; }
            set { volpercents[6] = value; }
        }
        public int RightVolumePercent
        {
            get { return volpercents[7]; }
            set { volpercents[7] = value; }
        }
        public int UpVolumePercent
        {
            get { return volpercents[8]; }
            set { volpercents[8] = value; }
        }
        public int RVolumePercent
        {
            get { return volpercents[9]; }
            set { volpercents[9] = value; }
        }
        public int LVolumePercent
        {
            get { return volpercents[10]; }
            set { volpercents[10] = value; }
        }
        public float BPM
        {
            get;
            set;
        }
        public bool BPMRapidChange
        {
            get;
            set;
        }
        public bool SquareKeepPlaying
        {
            get { return keepplayings[1]; }
            set { keepplayings[1] = value; }
        }
        public bool CrossKeepPlaying
        {
            get { return keepplayings[2]; }
            set { keepplayings[2] = value; }
        }
        public bool CircleKeepPlaying
        {
            get { return keepplayings[3]; }
            set { keepplayings[3] = value; }
        }
        public bool TriangleKeepPlaying
        {
            get { return keepplayings[4]; }
            set { keepplayings[4] = value; }
        }
        public bool LeftKeepPlaying
        {
            get { return keepplayings[5]; }
            set { keepplayings[5] = value; }
        }
        public bool DownKeepPlaying
        {
            get { return keepplayings[6]; }
            set { keepplayings[6] = value; }
        }
        public bool RightKeepPlaying
        {
            get { return keepplayings[7]; }
            set { keepplayings[7] = value; }
        }
        public bool UpKeepPlaying
        {
            get { return keepplayings[8]; }
            set { keepplayings[8] = value; }
        }
        public bool RKeepPlaying
        {
            get { return keepplayings[9]; }
            set { keepplayings[9] = value; }
        }
        public bool LKeepPlaying
        {
            get { return keepplayings[10]; }
            set { keepplayings[10] = value; }
        }
        public bool SquareReleaseSound
        {
            get { return releasesounds[1]; }
            set { releasesounds[1] = value; }
        }
        public bool CrossReleaseSound
        {
            get { return releasesounds[2]; }
            set { releasesounds[2] = value; }
        }
        public bool CircleReleaseSound
        {
            get { return releasesounds[3]; }
            set { releasesounds[3] = value; }
        }
        public bool TriangleReleaseSound
        {
            get { return releasesounds[4]; }
            set { releasesounds[4] = value; }
        }
        public bool LeftReleaseSound
        {
            get { return releasesounds[5]; }
            set { releasesounds[5] = value; }
        }
        public bool DownReleaseSound
        {
            get { return releasesounds[6]; }
            set { releasesounds[6] = value; }
        }
        public bool RightReleaseSound
        {
            get { return releasesounds[7]; }
            set { releasesounds[7] = value; }
        }
        public bool UpReleaseSound
        {
            get { return releasesounds[8]; }
            set { releasesounds[8] = value; }
        }
        public bool RReleaseSound
        {
            get { return releasesounds[9]; }
            set { releasesounds[9] = value; }
        }
        public bool LReleaseSound
        {
            get { return releasesounds[10]; }
            set { releasesounds[10] = value; }
        }
        public DisplayState DisplayState
        {
            get;
            set;
        }
        public MoveState MoveState
        {
            get;
            set;
        }
        public NoteType NoteType
        {
            get;
            set;
        }
        public float SlideScale
        {
            get;
            set;
        }
        public ButtonType[] InitializeOrder
        {
            get;
            set;
        }
        public int[] DrawingOrder
        {
            get
            {
                var ret = new int[10];
                for (int i = 0; i < 10; i++)
                {
                    ret[(int)InitializeOrder[i]] = i;
                }
                return ret;
            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(MovieVolumePercent);
            sb.Append(",");
            sb.Append(SquareVolumePercent);
            sb.Append(",");
            sb.Append(CrossVolumePercent);
            sb.Append(",");
            sb.Append(CircleVolumePercent);
            sb.Append(",");
            sb.Append(TriangleVolumePercent);
            sb.Append(",");
            sb.Append(LeftVolumePercent);
            sb.Append(",");
            sb.Append(DownVolumePercent);
            sb.Append(",");
            sb.Append(RightVolumePercent);
            sb.Append(",");
            sb.Append(UpVolumePercent);
            sb.Append(",");
            sb.Append(RVolumePercent);
            sb.Append(",");
            sb.Append(LVolumePercent);
            sb.Append(",");
            sb.Append(BPM);
            sb.Append(",");
            sb.Append(BPMRapidChange ? "1" : "0");
            sb.Append(",");
            sb.Append(SquareKeepPlaying ? "1" : "0");
            sb.Append(",");
            sb.Append(CrossKeepPlaying ? "1" : "0");
            sb.Append(",");
            sb.Append(CircleKeepPlaying ? "1" : "0");
            sb.Append(",");
            sb.Append(TriangleKeepPlaying ? "1" : "0");
            sb.Append(",");
            sb.Append(LeftKeepPlaying ? "1" : "0");
            sb.Append(",");
            sb.Append(DownKeepPlaying ? "1" : "0");
            sb.Append(",");
            sb.Append(RightKeepPlaying ? "1" : "0");
            sb.Append(",");
            sb.Append(UpKeepPlaying ? "1" : "0");
            sb.Append(",");
            sb.Append(RKeepPlaying ? "1" : "0");
            sb.Append(",");
            sb.Append(LKeepPlaying ? "1" : "0");
            sb.Append(",");
            sb.Append(SquareReleaseSound ? "1" : "0");
            sb.Append(",");
            sb.Append(CrossReleaseSound ? "1" : "0");
            sb.Append(",");
            sb.Append(CircleReleaseSound ? "1" : "0");
            sb.Append(",");
            sb.Append(TriangleReleaseSound ? "1" : "0");
            sb.Append(",");
            sb.Append(LeftReleaseSound ? "1" : "0");
            sb.Append(",");
            sb.Append(DownReleaseSound ? "1" : "0");
            sb.Append(",");
            sb.Append(RightReleaseSound ? "1" : "0");
            sb.Append(",");
            sb.Append(UpReleaseSound ? "1" : "0");
            sb.Append(",");
            sb.Append(RReleaseSound ? "1" : "0");
            sb.Append(",");
            sb.Append(LReleaseSound ? "1" : "0");
            sb.Append(",");
            sb.Append(DisplayState);
            sb.Append(",");
            sb.Append(MoveState);
            sb.Append(",");
            sb.Append(NoteType);
            sb.Append(",");
            sb.Append(SlideScale);
            sb.Append("\n");

            int iter = 0;
            foreach (ButtonType type in InitializeOrder)
            {
                if (iter != 0)
                {
                    sb.Append(",");
                }
                sb.Append(type.ToString());
            }
            return sb.ToString();
        }
        public string GetFormattedContent()
        {
            var sb = new StringBuilder();
            Append(sb, MovieVolumePercent.ToString());
            Append(sb, SquareVolumePercent.ToString(), SquareKeepPlaying.ToString(), SquareReleaseSound.ToString());
            Append(sb, CrossVolumePercent.ToString(), CrossKeepPlaying.ToString(), CrossReleaseSound.ToString());
            Append(sb, CircleVolumePercent.ToString(), CircleKeepPlaying.ToString(), CircleReleaseSound.ToString());
            Append(sb, TriangleVolumePercent.ToString(), TriangleKeepPlaying.ToString(), TriangleReleaseSound.ToString());
            Append(sb, LeftVolumePercent.ToString(), LeftKeepPlaying.ToString(), LeftReleaseSound.ToString());
            Append(sb, DownVolumePercent.ToString(), DownKeepPlaying.ToString(), DownReleaseSound.ToString());
            Append(sb, RightVolumePercent.ToString(), RightKeepPlaying.ToString(), RightReleaseSound.ToString());
            Append(sb, UpVolumePercent.ToString(), UpKeepPlaying.ToString(), UpReleaseSound.ToString());
            Append(sb, RVolumePercent.ToString(), RKeepPlaying.ToString(), RReleaseSound.ToString());
            Append(sb, LVolumePercent.ToString(), LKeepPlaying.ToString(), LReleaseSound.ToString());
            Append(sb, BPM.ToString(), BPMRapidChange.ToString());
            Append(sb, DisplayState.ToString());
            Append(sb, MoveState.ToString());
            Append(sb, NoteType.ToString());
            Append(sb, SlideScale.ToString());
            Append(sb,
                InitializeOrder[0].ToString(),
                InitializeOrder[1].ToString(),
                InitializeOrder[2].ToString(),
                InitializeOrder[3].ToString(),
                InitializeOrder[4].ToString(),
                InitializeOrder[5].ToString(),
                InitializeOrder[6].ToString(),
                InitializeOrder[7].ToString(),
                InitializeOrder[8].ToString(),
                InitializeOrder[9].ToString()
            );
            return sb.ToString();
        }
        public void Append(StringBuilder sb, params string[] datas)
        {
            sb.Append(string.Join(",", datas));
            sb.Append("\n");
        }
    }
}
