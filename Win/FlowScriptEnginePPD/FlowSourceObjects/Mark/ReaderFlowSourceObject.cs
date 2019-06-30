using PPDFramework.PPDStructure.PPDData;
using SharpDX;
using System.IO;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_Reader_Summary")]
    public partial class ReaderFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Mark.Reader"; }
        }

        private MarkDataBase[] markData;
        private int currentIndex;

        [ToolTipText("Mark_Reader_Stream")]
        public Stream Stream
        {
            private get;
            set;
        }

        [ToolTipText("Mark_Reader_EndOfStream")]
        public bool EndOfStream
        {
            get
            {
                return markData != null && currentIndex >= markData.Length;
            }
        }

        [ToolTipText("Mark_Reader_Position")]
        public Vector2 Position
        {
            get;
            private set;
        }

        [ToolTipText("Mark_Reader_Angle")]
        public float Angle
        {
            get;
            private set;
        }

        [ToolTipText("Mark_Reader_MarkType")]
        public PPDCoreModel.Data.MarkType MarkType
        {
            get;
            private set;
        }

        [ToolTipText("Mark_Reader_Time")]
        public float Time
        {
            get;
            private set;
        }

        [ToolTipText("Mark_Reader_EndTime")]
        public float EndTime
        {
            get;
            private set;
        }

        [ToolTipText("Mark_Reader_IsLong")]
        public bool IsLong
        {
            get;
            private set;
        }

        [ToolTipText("Mark_Reader_MarkID")]
        public int MarkID
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Stream));
            try
            {
                if (Stream != null)
                {
                    markData = PPDFramework.PPDStructure.PPDReader.Read(Stream);
                    OnSuccess();
                }
                else
                {
                    OnFailed();
                }
            }
            catch
            {
                OnFailed();
            }
        }

        protected override void OnReset()
        {
            markData = null;
            currentIndex = 0;
        }

        [ToolTipText("Mark_Reader_Next")]
        public void Next(FlowScriptEngine.FlowEventArgs e)
        {
            if (markData != null && !EndOfStream)
            {
                Position = new Vector2(markData[currentIndex].X, markData[currentIndex].Y);
                Angle = markData[currentIndex].Angle;
                Time = markData[currentIndex].Time;
                MarkType = (PPDCoreModel.Data.MarkType)markData[currentIndex].ButtonType;
                MarkID = (int)markData[currentIndex].ID;
                if (markData[currentIndex] is ExMarkData)
                {
                    IsLong = true;
                    EndTime = (markData[currentIndex] as ExMarkData).EndTime;
                }
                else
                {
                    IsLong = false;
                    EndTime = 0;
                }

                currentIndex++;
            }
        }
    }
}
