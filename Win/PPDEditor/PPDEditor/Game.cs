using PPDEditor.Forms;
using PPDFramework;
using System;

namespace PPDEditor
{
    public class Game : GameCore
    {
        public EditorForm Window
        {
            get { return Form as EditorForm; }
        }

        public Game(PPDExecuteArg args) : base(args, new EditorForm())
        {
            SplashForm.ShowSplash(Window);
            SplashForm.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["MainWindow"]));
            Window.Initialize();
        }
    }
}