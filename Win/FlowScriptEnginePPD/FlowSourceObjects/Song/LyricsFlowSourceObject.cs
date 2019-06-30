using FlowScriptEngine;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Song
{
    [ToolTipText("Song_Lyrics_Summary")]
    public partial class LyricsFlowSourceObject : FlowSourceObjectBase
    {
        ILylics lylics;

        public override string Name
        {
            get { return "PPD.Song.Lyrics"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("Lylics"))
            {
                lylics = this.Manager.Items["Lylics"] as ILylics;
            }
        }

        [ToolTipText("Song_Lyrics_Value")]
        public string Value
        {
            get
            {
                return lylics != null ? lylics.Lylics : "";
            }
        }
    }
}
