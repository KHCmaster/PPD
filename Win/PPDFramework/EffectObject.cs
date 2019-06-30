using Effect2D;
using PPDFramework.Resource;
using PPDFramework.Shaders;
using SharpDX;
using System;
using System.Windows.Forms;

namespace PPDFramework
{
    /// <summary>
    /// エフェクト描画クラス
    /// </summary>
    public class EffectObject : GameComponent
    {
        /// <summary>
        /// エフェクトの配置
        /// </summary>
        public enum EffectAlignment
        {
            /// <summary>
            /// センタリング
            /// </summary>
            Center = 0,
            /// <summary>
            /// 上左端
            /// </summary>
            TopLeft = 1
        }

        /// <summary>
        /// シークする場所
        /// </summary>
        public enum SeekPosition
        {
            /// <summary>
            /// 開始
            /// </summary>
            Start = 0,
            /// <summary>
            /// 終了
            /// </summary>
            End = 1
        }

        /// <summary>
        /// 再生終了イベント
        /// </summary>
        public event EventHandler Finish;

        Resource.ResourceManager resourceManager;
        EffectManager manager;

        /// <summary>
        /// 再生タイプ
        /// </summary>
        public EffectManager.PlayType PlayType
        {
            get;
            set;
        }

        /// <summary>
        /// 再生状態
        /// </summary>
        public EffectManager.PlayState PlayState
        {
            get
            {
                return manager.State;
            }
        }

        /// <summary>
        /// 配置
        /// </summary>
        public EffectAlignment Alignment
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filename">エフェクトパス</param>
        /// <param name="resourceManager">リソースマネージャー</param>
        public EffectObject(PPDDevice device, Resource.ResourceManager resourceManager, PathObject filename) : base(device)
        {
            this.resourceManager = resourceManager;
            manager = EffectLoader.Load(filename, LoadFunc);
            if (manager == null)
            {
                MessageBox.Show("Failed to load effect:" + filename);
                return;
            }
            manager.Finish += manager_Finish;
            Alignment = EffectAlignment.Center;
            PlayType = EffectManager.PlayType.Once;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="manager"></param>
        /// <param name="resourceManager">リソースマネージャー</param>
        public EffectObject(PPDDevice device, Resource.ResourceManager resourceManager, EffectManager manager) : base(device)
        {
            this.resourceManager = resourceManager;
            this.manager = manager;
            manager.Finish += manager_Finish;
            Alignment = EffectAlignment.Center;
            PlayType = EffectManager.PlayType.Once;
        }

        private void LoadFunc(string fn)
        {
            var ir = resourceManager.GetResource<ImageResourceBase>(fn);
            if (ir == null)
            {
                resourceManager.Add(fn, ImageResourceFactoryManager.Factory.Create(device, fn, false));
            }
        }

        void manager_Finish(object sender, EventArgs e)
        {
            if (Finish != null) Finish.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 再生する
        /// </summary>
        public void Play()
        {
            manager.Play(PlayType);
        }

        /// <summary>
        /// シークする
        /// </summary>
        public void Seek(SeekPosition seekPosition)
        {
            manager.Seek(seekPosition == SeekPosition.Start ? manager.StartFrame : manager.StartFrame + manager.FrameLength);
        }

        /// <summary>
        /// 一時停止する
        /// </summary>
        public void Pause()
        {
            manager.Pause();
        }

        /// <summary>
        /// 停止する
        /// </summary>
        public void Stop()
        {
            manager.Stop();
        }

        /// <summary>
        /// 更新します。
        /// </summary>
        protected override void UpdateImpl()
        {
            manager.Update();
        }

        /// <summary>
        /// 描画処理を行います
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            var initialAlpha = alphaBlendContext.Alpha;
            var initialDepth = alphaBlendContext.SRTDepth;
            manager.Draw((filename, structure) =>
            {
                var ir = resourceManager.GetResource<ImageResourceBase>(filename);
                if (ir != null)
                {
                    alphaBlendContext.Texture = ir.Texture;
                    alphaBlendContext.Vertex = ir.Vertex;
                    alphaBlendContext.Alpha = initialAlpha * structure.ComposedAlpha;
                    if (structure.ComposedBlendMode != BlendMode.None)
                    {
                        alphaBlendContext.BlendMode = structure.ComposedBlendMode;
                    }
                    alphaBlendContext.SRTDepth = initialDepth + 1;
                    foreach (var matrix in structure.ComposedMatrices)
                    {
                        alphaBlendContext.SetSRT(matrix, alphaBlendContext.SRTDepth++);
                    }
                    var pos = new Vector2(-ir.Width / 2, -ir.Height / 2);
                    if (Alignment == EffectAlignment.TopLeft)
                    {
                        pos = Vector2.Zero;
                    }
                    alphaBlendContext.SetSRT(Matrix.Translation(new Vector3(pos, 0)), alphaBlendContext.SRTDepth);
                    device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext);
                }
            });
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (manager != null)
            {
                manager.Finish -= manager_Finish;
                manager = null;
            }
        }
    }
}
