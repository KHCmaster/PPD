using PPDEditor.Controls;
using PPDEditor.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor
{
    public static class WindowUtility
    {
        public static EditorForm MainForm
        {
            get;
            set;
        }
        public static BPMMeasure BPMMeasure
        {
            get;
            set;
        }
        public static DXForm DXForm
        {
            get;
            set;
        }
        public static EventManager EventManager
        {
            get;
            set;
        }
        public static InfoForm InfoForm
        {
            get;
            set;
        }
        public static IniFileWriter IniFileWriter
        {
            get;
            set;
        }
        public static KasiEditor KasiEditor
        {
            get;
            set;
        }
        public static LayerManager LayerManager
        {
            get;
            set;
        }
        public static PosAndAngleLoaderSaver PosAndAngleLoaderSaver
        {
            get;
            set;
        }
        public static SoundManager SoundManager
        {
            get;
            set;
        }
        public static TimeLineForm TimeLineForm
        {
            get;
            set;
        }
        public static Seekmain Seekmain
        {
            get;
            set;
        }
        public static GeometryCreator GeometryCreator
        {
            get;
            set;
        }
        public static DockPanel DockPanel
        {
            get;
            set;
        }
        public static MemoWindow MemoWindow
        {
            get;
            set;
        }
        public static HelpForm HelpForm
        {
            get;
            set;
        }
        public static ResourceManager ResourceManager
        {
            get;
            set;
        }
        public static ScriptManager ScriptManager
        {
            get;
            set;
        }
        public static StatsManager StatsManager
        {
            get;
            set;
        }
    }
}
