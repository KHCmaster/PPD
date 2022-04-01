using PPDFramework.Chars;
using PPDFramework.ScreenFilters.Impl;
using PPDFramework.Shaders;
using PPDFrameworkCore;
using SharpDX;
using System;
using System.Threading;
using System.Windows.Forms;

namespace PPDFramework
{
    /// <summary>
    /// ゲームコアの基底クラスです。
    /// </summary>
    public abstract class GameCoreBase : DisposableComponent
    {
        /// <summary>
        /// ゲームのタイマーです。
        /// </summary>
        protected GameTimer gameTimer;

        /// <summary>
        /// デバイスです。
        /// </summary>
        protected PPDDevice device;

        double nextframe;
        float wait = 1000 / 60f;
        double lastPresentEndTime;
        double presentStartTime;
        int presentCount;
        RectangleComponent[] blackRectangles;
        PPDFramework.Resource.ResourceManager resourceManager;

        /// <summary>
        /// 引数を取得します。
        /// </summary>
        public PPDExecuteArg Args
        {
            get;
            private set;
        }

        /// <summary>
        /// FPSを取得します。
        /// </summary>
        public float FPS
        {
            get;
            private set;
        }

        /// <summary>
        /// 最後に更新した時間を取得します。
        /// </summary>
        public float LastUpdateTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 最後に描画した時間を取得します。
        /// </summary>
        public float LastDrawTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 閉じるべきかどうかを取得、設定します。
        /// </summary>
        public bool ShouldBeExit
        {
            get;
            set;
        }

        /// <summary>
        /// フルスクリーンかどうかを取得、設定します。
        /// </summary>
        public bool FullScreen
        {
            get;
            set;
        }

        /// <summary>
        /// コントロールを取得します。
        /// </summary>
        public Control Control
        {
            get;
            private set;
        }

        /// <summary>
        /// 現在のFPSを設定します。
        /// </summary>
        protected virtual string CurrentFPS
        {
            get;
            set;
        }

        /// <summary>
        /// ロード中かどうかを取得します。
        /// </summary>
        protected virtual bool IsLoading
        {
            get;
        }

        /// <summary>
        /// FPSを表示するかどうかを取得します。
        /// </summary>
        protected virtual bool ShowFPS
        {
            get;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="args"></param>
        /// <param name="control"></param>
        protected GameCoreBase(PPDExecuteArg args, Control control)
        {
            SharpDX.Configuration.EnableReleaseOnFinalizer = true;
            gameTimer = new GameTimer();
            Control = control;
            Args = args;
            ShouldBeExit = false;
        }

        /// <summary>
        /// DirectXの初期化に失敗したときの処理です。
        /// </summary>
        protected virtual void OnFailedInitializeDirectX()
        {
        }

        /// <summary>
        /// DirectXの初期化に成功したときの処理です。
        /// </summary>
        protected virtual void OnSuccessInitializeDirectX()
        {
        }

        private void PreventScreenSaver()
        {
            try
            {
                var prev = WinAPI.SetThreadExecutionState(WinAPI.EXECUTION_STATE.ES_DISPLAY_REQUIRED);
            }
            catch
            {
            }
        }

        /// <summary>
        /// ルーチンを実行します。
        /// </summary>
        public void Routine()
        {
            nextframe = gameTimer.ElapsedTickTime;
            PreventScreenSaver();
            long last = gameTimer.ElapsedTime;
            double beforeUpdateTime = gameTimer.ElapsedTickTime;
            Update();
            LastUpdateTime = (float)(gameTimer.ElapsedTickTime - beforeUpdateTime);
            long current = gameTimer.ElapsedTime;
            if ((double)current < nextframe + wait)
            {
                double beforeDrawTime = gameTimer.ElapsedTickTime;
                device.DrawStart();
                Draw();
                foreach (var rect in blackRectangles)
                {
                    rect.Draw();
                }
                device.GetModule<AlphaBlendContextCache>().Reset();
#if BENCHMARK
                Benchmark.Instance.EndLoop();
#endif
                DrawEnd();
                device.DrawEnd();
                LastDrawTime = (float)(gameTimer.ElapsedTickTime - beforeDrawTime);
                try
                {
                    //更新
                    var diff = (int)(lastPresentEndTime + wait - gameTimer.ElapsedTickTime - 1);
                    if (diff > 0 && !PPDSetting.Setting.FixedFPSDisabled)
                    {
                        Thread.Sleep(diff);
                    }
                    device.Present();
                    lastPresentEndTime = gameTimer.ElapsedTickTime;
                    presentCount++;
                    if (presentCount >= 60)
                    {
                        FPS = (float)(1000 / (lastPresentEndTime - presentStartTime) * 60);
                        if (ShowFPS)
                        {
                            CurrentFPS = String.Format("PPD FPS:{0}", FPS);
                        }
                        presentCount = 0;
                        presentStartTime = lastPresentEndTime;
                    }
                }
                catch (SharpDXException e)
                {
                    if (e.ResultCode == SharpDX.Direct3D9.ResultCode.DeviceLost)
                    {
                        device.ResetDevice();
                    }
                }
            }
        }

        /// <summary>
        /// DirectXを初期化します。
        /// </summary>
        /// <returns></returns>
        public bool InitializeDirectX(int? width = 800, int? height = 450, int? expectedWidth = 800, int? expectedHeight = 450, float? expectedAspectRatio = 16f / 9)
        {
            try
            {
                if (width <= 0)
                {
                    width = 800;
                }
                if (height <= 0)
                {
                    height = 450;
                }
                Control.Size = new System.Drawing.Size(width.Value, height.Value);
                InitializeUI();

                device = PPDDevice.Initialize(Control.Handle, width.Value, height.Value, expectedWidth.Value, expectedHeight.Value, expectedAspectRatio.Value);
                device.RegisterModule(new AlphaBlendContextCache(1024));
                device.RegisterModule(new ShaderCommon(device));
                device.RegisterModule(new ColorTextureAllcator(device));
                device.RegisterModule(new CharCacheManager(device));
                device.RegisterModule(new AlphaBlend(device));
                device.RegisterModule(new GaussianFilter(device));
                device.RegisterModule(new ColorScreenFilter());
                device.RegisterModule(new GlassFilter(device));
                device.RegisterModule(new BorderFilter(device));
                device.RegisterModule(new MosaicFilter(device));
                device.GetModule<ShaderCommon>().InitializeScreenVertex(device);
                resourceManager = new PPDFramework.Resource.ResourceManager();
                blackRectangles = new RectangleComponent[2];
                for (var i = 0; i < blackRectangles.Length; i++)
                {
                    blackRectangles[i] = new RectangleComponent(device, resourceManager, PPDColors.Black);
                }
                var invScale = 1 / device.Scale.X;
                if (device.Offset.X == 0)
                {
                    blackRectangles[0].Position = new Vector2(0, -device.Offset.Y * invScale);
                    blackRectangles[0].RectangleWidth = width.Value * invScale;
                    blackRectangles[0].RectangleHeight = device.Offset.Y * invScale;
                    blackRectangles[1].Position = new Vector2(0, device.ExpectedHeight);
                    blackRectangles[1].RectangleWidth = width.Value * invScale;
                    blackRectangles[1].RectangleHeight = device.Offset.Y * invScale;
                }
                else
                {
                    blackRectangles[0].Position = new Vector2(-device.Offset.X * invScale, 0);
                    blackRectangles[0].RectangleWidth = device.Offset.X * invScale;
                    blackRectangles[0].RectangleHeight = height.Value * invScale;
                    blackRectangles[1].Position = new Vector2(device.ExpectedWidth, 0);
                    blackRectangles[1].RectangleWidth = device.Offset.X * invScale;
                    blackRectangles[1].RectangleHeight = height.Value * invScale;
                }
                blackRectangles[0].Update();
                blackRectangles[1].Update();
                return true;
            }
#if DEBUG
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
                throw;
#else
            catch
            {
                //初期化失敗
                return false;
#endif
            }
        }

        /// <summary>
        /// UIを初期化します。
        /// </summary>
        protected virtual void InitializeUI()
        {

        }

        /// <summary>
        /// 初期化の処理です。
        /// </summary>
        protected virtual void Initialize()
        {

        }

        /// <summary>
        /// 更新する処理です。
        /// </summary>
        protected virtual void Update()
        {

        }

        /// <summary>
        /// 描画する処理です。
        /// </summary>
        protected virtual void Draw()
        {

        }

        /// <summary>
        /// 描画が完全に終わったときの処理です。
        /// </summary>
        protected virtual void DrawEnd()
        {

        }

        /// <summary>
        /// リソースを破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (device != null)
            {
                device.Dispose();
                device = null;
            }
        }
    }
}
