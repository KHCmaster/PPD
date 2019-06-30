using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SlimDX.Direct3D9;

namespace testgame
{
    class SceneManager
    {
        public ISceneBase CurrentScene
        {
            get;
            set;
        }
        public ISceneBase LoadingScene
        {
            get;
            set;
        }
        Device Device { get; set; }
        Sprite Sprite { get; set; }
        ExSound Sound { get; set; }
        ISceneBase NextScene;
        Thread LoadingThread;
        Dictionary<Type, Dictionary<string, object>> GlobalPool;
        public SceneManager()
        {
            GlobalPool = new Dictionary<Type, Dictionary<string, object>>();
        }
        public void PrepareNextScene(ISceneBase Caller, ISceneBase nextscene, Dictionary<string, object> param, Dictionary<string, object> previousparam)
        {
            NextScene = nextscene;
            NextScene.Device = Device;
            NextScene.Sprite = Sprite;
            NextScene.Sound = Sound;
            NextScene.SceneManager = this;
            NextScene.Param = param == null ? new Dictionary<string, object>() : param;
            NextScene.PreviousParam = GlobalPool.ContainsKey(NextScene.GetType()) ? GlobalPool[NextScene.GetType()] : new Dictionary<string, object>();
            if (previousparam != null)
            {
                if (GlobalPool.ContainsKey(Caller.GetType()))
                {
                    GlobalPool[Caller.GetType()] = previousparam;
                }
                else
                {
                    GlobalPool.Add(Caller.GetType(), previousparam);
                }
            }
            LoadingThread = new Thread(new ThreadStart(nextscene.Load));
            LoadingThread.Start();
            CurrentScene.Dispose();
            CurrentScene = null;
        }
        public void Update(int[] presscount, bool[] released, Device device, Sprite sprite, ExSound sound)
        {
            Device = device;
            Sprite = sprite;
            Sound = sound;
            if (CurrentScene != null)
            {
                CurrentScene.Update(presscount, released);
            }
            if (NextScene != null)
            {
                if (LoadingThread.ThreadState == ThreadState.Stopped)
                {
                    LoadingThread.Join();
                    LoadingThread = null;
                    CurrentScene = NextScene;
                    NextScene = null;
                }
            }
            if (LoadingThread != null)
            {
                LoadingScene.Update(presscount, released);
            }
        }

        public void Draw()
        {
            if (CurrentScene != null)
            {
                CurrentScene.Draw();
            }
            if (LoadingThread != null)
            {
                LoadingScene.Draw();
            }
        }
    }
}
