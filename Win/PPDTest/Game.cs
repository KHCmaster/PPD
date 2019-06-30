using PPDFramework;

namespace PPDTest
{
    class Game : GameCore
    {
        protected override bool ShowFPS
        {
            get { return true; }
        }

        public Game(PPDExecuteArg args) : base(args, new MyForm())
        {
        }
    }
}
