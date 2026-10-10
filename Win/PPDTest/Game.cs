using PPDFramework;

namespace PPDTest
{
    class Game(PPDExecuteArg args) : GameCore(args, new MyForm())
    {
        protected override bool ShowFPS
        {
            get { return true; }
        }
    }
}
