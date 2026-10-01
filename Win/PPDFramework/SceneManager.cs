using PPDFramework.Chars;
using PPDFramework.Resource;
using PPDFramework.Scene;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PPDFramework
{
    /// <summary>
    /// シーンマネージャー
    /// </summary>
    public class SceneManager : DisposableComponent
    {
        /// <summary>
        /// ローディングに入った
        /// </summary>
        public event EventHandler EnterLoading;

        /// <summary>
        /// ローディングから出た
        /// </summary>
        public event EventHandler LeaveLoading;

        /// <summary>
        /// 現在のシーン
        /// </summary>
        public ISceneBase CurrentScene
        {
            get;
            set;
        }

        /// <summary>
        /// ローディングシーン
        /// </summary>
        public LoadingBase LoadingScene
        {
            get;
            set;
        }

        /// <summary>
        /// ゲームホスト
        /// </summary>
        public IGameHost GameHost
        {
            get;
            set;
        }

        ISound Sound
        {
            get;
            set;
        }

        readonly PPDDevice device;
        ISceneBase nextScene;
        ISceneBase previousScene;
        Thread loadingThread;
        readonly Dictionary<Type, Dictionary<string, object>> globalPool;
        readonly Stack<ISceneBase> sceneStack;
        bool previousSceneResourceUsing;

        float lastTime;

        private bool loadFinished;
        private bool loadResult;
        private bool LoadFinished
        {
            get
            {
                if (loadingThread != null)
                {
                    return loadingThread.ThreadState == ThreadState.Stopped;
                }
                else
                {
                    return loadFinished;
                }
            }
            set
            {
                loadFinished = value;
            }
        }

        /// <summary>
        /// ロード中かどうか
        /// </summary>
        public bool Loading
        {
            get
            {
                return loadingThread != null;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SceneManager(PPDDevice device)
        {
            this.device = device;
            globalPool = [];
            sceneStack = new Stack<ISceneBase>();
        }

        /// <summary>
        /// ゲームを変更します。
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="param"></param>
        /// <param name="spriteResourceManager"></param>
        public void ChangeGame(ISceneBase scene, Dictionary<string, object> param, SpriteResourceManager spriteResourceManager)
        {
            InnerPrepareNextScene(null, scene, param, null, true, spriteResourceManager);
        }

        /// <summary>
        /// 次のシーンを非同期で用意する
        /// </summary>
        /// <param name="caller">呼び出しシーン</param>
        /// <param name="nextScene">次のシーン</param>
        /// <param name="param">次のシーンに渡すパラメーター</param>
        /// <param name="previousParam">呼び出し側のシーンの次の生成時に渡すパラメーター</param>
        public void PrepareNextScene(ISceneBase caller, ISceneBase nextScene, Dictionary<string, object> param, Dictionary<string, object> previousParam)
        {
            PrepareNextScene(caller, nextScene, param, previousParam, false);
        }

        /// <summary>
        /// 次のシーンを非同期で用意する
        /// </summary>
        /// <param name="caller">呼び出しシーン</param>
        /// <param name="nextScene">次のシーン</param>
        /// <param name="param">次のシーンに渡すパラメーター</param>
        /// <param name="previousParam">呼び出し側のシーンの次の生成時に渡すパラメーター</param>
        /// <param name="useStack">スタックを使うかどうか</param>
        public void PrepareNextScene(ISceneBase caller, ISceneBase nextScene, Dictionary<string, object> param, Dictionary<string, object> previousParam, bool useStack)
        {
            InnerPrepareNextScene(caller, nextScene, param, previousParam, useStack, null);
        }

        /// <summary>
        /// 現在のシーンをポップします
        /// </summary>
        public void PopCurrentScene(Dictionary<string, object> param)
        {
            if (CurrentScene != null && CurrentScene.IsInSceneStack && sceneStack.Count > 0)
            {
                previousScene = CurrentScene;
                CurrentScene = sceneStack.Pop();
                CurrentScene.SceneStackPoped(param);
            }
        }

        /// <summary>
        /// ホームまでポップします
        /// </summary>
        public void PopToHome()
        {
            while (sceneStack.Count > 0)
            {
                if (CurrentScene != null)
                {
                    previousScene = CurrentScene;
                    CurrentScene = sceneStack.Pop();
                }
                if (previousScene != null)
                {
                    previousScene.Dispose();
                    if (!PPDSetting.Setting.AllowedToUseMuchMemory && previousScene.ResourceManager != null)
                    {
                        previousScene.ResourceManager.Dispose();
                        previousScene.Dispose();
                        previousScene = null;
                    }
                }
            }
            if (previousScene != null && previousScene.ResourceManager != null)
            {
                previousScene.ResourceManager.Dispose();
                previousScene.Dispose();
                previousScene = null;
            }
        }

        private void InnerPrepareNextScene(ISceneBase caller, ISceneBase nextScene, Dictionary<string, object> param, Dictionary<string, object> previousParam, bool useStack, ResourceManager resourceManager)
        {
            this.nextScene = nextScene;
            nextScene.Sound = Sound;
            nextScene.SceneManager = this;
            nextScene.GameHost = GameHost;
            previousSceneResourceUsing = false;
            if (resourceManager != null)
            {
                nextScene.ResourceManager = resourceManager;
            }
            else
            {
                if (String.IsNullOrEmpty(nextScene.SpriteDir))
                {
                    if (caller != null)
                    {
                        nextScene.ResourceManager = caller.ResourceManager;
                        previousSceneResourceUsing = true;
                    }
                    else
                    {
                        nextScene.ResourceManager = new ResourceManager();
                    }
                }
                else
                {
                    nextScene.ResourceManager = new ResourceManager(new Tuple<ResourceManager, bool>[] {
                        new( new SpriteResourceManager(device, nextScene.SpriteDir), true),
                        new( caller.ResourceManager,false)
                    });
                    previousSceneResourceUsing = true;
                }
            }
            nextScene.Param = param ?? [];
            nextScene.PreviousParam = globalPool.ContainsKey(nextScene.GetType()) ? globalPool[nextScene.GetType()] : [];
            if (previousParam != null && caller != null)
            {
                if (globalPool.ContainsKey(caller.GetType()))
                {
                    globalPool[caller.GetType()] = previousParam;
                }
                else
                {
                    globalPool.Add(caller.GetType(), previousParam);
                }
            }

            if (useStack)
            {
                nextScene.IsInSceneStack = true;
                sceneStack.Push(CurrentScene);
            }
            else
            {
                previousScene = CurrentScene;
            }

            CurrentScene = null;
            lastTime = Environment.TickCount;
            loadingThread = ThreadManager.Instance.GetThread(() =>
            {
                loadResult = nextScene.Load();
            });
            loadingThread.Start();
            LoadingScene?.EnterLoading();
            EnterLoading?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 更新する
        /// </summary>
        /// <param name="inputInfo">入力の情報</param>
        /// <param name="mouseInfo">マウスの情報</param>
        /// <param name="sound">サウンド</param>
        public void Update(InputInfoBase inputInfo, MouseInfo mouseInfo, ISound sound)
        {
            Sound = sound;
            if (previousScene != null)
            {
                previousScene.Dispose();
                if (!previousSceneResourceUsing && !PPDSetting.Setting.AllowedToUseMuchMemory && previousScene.ResourceManager != null)
                {
                    previousScene.ResourceManager.Dispose();
                }
                previousScene = null;
                device.GetModule<CharCacheManager>().ClearUnUsed();
            }
            if (nextScene != null)
            {
                if (LoadFinished)
                {
#if DEBUG
                    Console.WriteLine("LoadTime is:" + (Environment.TickCount - lastTime));
#if BENCHMARK
                    Benchmark.Instance.EndCategory();
#endif
#endif
                    loadingThread.Join();
                    loadingThread = null;
                    device.GetModule<CharCacheManager>().ClearUnUsed();
                    if (loadResult)
                    {
                        CurrentScene = nextScene;
                    }
                    else
                    {
                        previousScene = nextScene;
                        CurrentScene = sceneStack.Pop();
                        CurrentScene.SceneStackPoped(new Dictionary<string, object>
                        {
                            {"Failed to initialize", ""}
                        });
                    }
                    nextScene = null;
                    LeaveLoading?.Invoke(this, EventArgs.Empty);
                }
            }
            CurrentScene?.Update(inputInfo, mouseInfo);
            if (loadingThread != null && LoadingScene != null)
            {
                LoadingScene.Update(inputInfo, mouseInfo);
            }
        }

        /// <summary>
        /// 描画する
        /// </summary>
        public void Draw()
        {
            CurrentScene?.Draw();
            if (loadingThread != null && LoadingScene != null)
            {
                LoadingScene.Draw();
            }
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            try
            {
                if (loadingThread != null && loadingThread.IsAlive)
                {
                    nextScene?.ShouldFinishLoad = true;
                    // 読み込みの中断を要求して、終わるまで少し待つ（.NET では Thread.Abort が使えない）。
                    // 終わらなかった場合もバックグラウンドスレッドなので、プロセスの終了は妨げない
                    loadingThread.Join(3000);
                }
                while (sceneStack.Count > 0)
                {
                    DisposeScene(sceneStack.Pop());
                }
                DisposeScene(LoadingScene);
                DisposeScene(CurrentScene);
                DisposeScene(previousScene);
                device.GetModule<CharCacheManager>().ClearUnUsed();
            }
            catch
            {
            }
        }

        private void DisposeScene(ISceneBase scene)
        {
            if (scene != null)
            {
                scene.Dispose();
                scene.ResourceManager?.Dispose();
            }
        }
    }
}
