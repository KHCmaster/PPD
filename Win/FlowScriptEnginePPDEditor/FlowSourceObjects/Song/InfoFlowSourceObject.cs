using FlowScriptEngine;
using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Song
{
    [ToolTipText("Song_Info_Summary")]
    public partial class InfoFlowSourceObject : FlowSourceObjectBase
    {
        ISongInfo songInfo;

        public override string Name
        {
            get { return "PPDEditor.Song.Info"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Manager.Items.ContainsKey("SongInfo"))
            {
                songInfo = Manager.Items["SongInfo"] as ISongInfo;
            }
        }

        [ToolTipText("Song_Info_BPM")]
        public float BPM
        {
            get
            {
                if (songInfo != null)
                {
                    return songInfo.BPM;
                }
                return 0;
            }
        }

        [ToolTipText("Song_Info_BPMOffset")]
        public float BPMOffset
        {
            get
            {
                if (songInfo != null)
                {
                    return songInfo.BPMOffset;
                }
                return 0;
            }
        }

        [ToolTipText("Song_Info_ProjectPath")]
        public string ProjectPath
        {
            get
            {
                if (songInfo != null)
                {
                    return songInfo.CurrentProjectFilePath;
                }
                return null;
            }
        }

        [ToolTipText("Song_Info_ProjectName")]
        public string ProjectName
        {
            get
            {
                if (songInfo != null)
                {
                    return songInfo.CurrentProjectName;
                }
                return null;
            }
        }
    }
}
