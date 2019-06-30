using FlowScriptEngine;

namespace PPDCoreModel
{
    public class PauseManager
    {
        public const string PausedCallbackName = "PPD_Pause_Paused";
        public const string ResumedCallbackName = "PPD_Pause_Resumed";

        private Engine engine;

        public bool IsPaused
        {
            get;
            private set;
        }

        public PauseManager(Engine engine)
        {
            this.engine = engine;
            Initialize();
        }

        public void Initialize()
        {
            IsPaused = false;
        }

        public void Pause()
        {
            if (!IsPaused)
            {
                engine.Call(PausedCallbackName, null);
                IsPaused = true;
            }
        }

        public void Resume()
        {
            if (IsPaused)
            {
                engine.Call(ResumedCallbackName, null);
                IsPaused = false;
            }
        }

        public void Toggle()
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
}
