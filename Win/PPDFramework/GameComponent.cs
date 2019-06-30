using Effect2D;
using PPDFramework.ScreenFilters;
using PPDFramework.Shaders;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDFramework
{
    /// <summary>
    /// ゲームコンポーネントクラス
    /// </summary>
    public abstract class GameComponent : IGameComponent
    {
        /// <summary>
        /// マウスイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseEvent"></param>
        public delegate void MouseEventHandler(GameComponent sender, MouseEvent mouseEvent);

        /// <summary>
        /// マウスの左ボタンが押された
        /// </summary>
        public event MouseEventHandler MouseLeftDown;

        /// <summary>
        /// マウスの左ボタンが離された
        /// </summary>
        public event MouseEventHandler MouseLeftUp;

        /// <summary>
        /// マウスの中ボタンが押された
        /// </summary>
        public event MouseEventHandler MouseMiddleDown;

        /// <summary>
        /// マウスの中ボタンが離された
        /// </summary>
        public event MouseEventHandler MouseMiddleUp;

        /// <summary>
        /// マウスの右ボタンが押された
        /// </summary>
        public event MouseEventHandler MouseRightDown;

        /// <summary>
        /// マウスの右ボタンが離された
        /// </summary>
        public event MouseEventHandler MouseRightUp;

        /// <summary>
        /// マウスのホイールがまわされた
        /// </summary>
        public event MouseEventHandler MouseWheel;

        /// <summary>
        /// マウスが動いた
        /// </summary>
        public event MouseEventHandler MouseMove;

        /// <summary>
        /// マウスが入った
        /// </summary>
        public event MouseEventHandler MouseEnter;

        /// <summary>
        /// マウスが出た
        /// </summary>
        public event MouseEventHandler MouseLeave;

        /// <summary>
        /// マウスの左ボタンでクリックした
        /// </summary>
        public event MouseEventHandler MouseLeftClick;

        /// <summary>
        /// マウスの右ボタンでクリックした
        /// </summary>
        public event MouseEventHandler MouseRightClick;

        /// <summary>
        /// マウスの中ボタンでクリックした
        /// </summary>
        public event MouseEventHandler MouseMiddleClick;

        /// <summary>
        /// 描画するかどうか
        /// </summary>
        public event Func<GameComponent, AlphaBlendContext, int, int, bool> CanDraw;

        /// <summary>
        /// 子要素を描画するかどうか
        /// </summary>
        public event Func<GameComponent, AlphaBlendContext, int, int, bool> CanDrawChild;

        /// <summary>
        /// 更新するかどうか
        /// </summary>
        public event Func<GameComponent, bool> CanUpdate;

        /// <summary>
        /// 子要素を更新するかどうか
        /// </summary>
        public event Func<GameComponent, int, bool> CanUpdateChild;

        /// <summary>
        /// デバイス
        /// </summary>
        protected PPDDevice device;

        private GameComponent parent;
        private List<GameComponent> children;
        private bool childChanged;

        private float _width;
        private float _height;

        private bool lastOnMouse;
        private Vector2 lastMousePos;
        private Vector2 lastPosition;
        private bool mouseLeftDown;
        private bool mouseRightDown;
        private bool mouseMiddleDown;
        private bool acceptMouseOperation;

        /// <summary>
        /// 描画前のスクリーンフィルター
        /// </summary>
        protected List<ScreenFilterBase> preScreenFilters;

        /// <summary>
        /// 描画後のスクリーンフィルター
        /// </summary>
        protected List<ScreenFilterBase> postScreenFilters;

        /// <summary>
        /// カラーフィルター
        /// </summary>
        protected List<ColorFilterBase> colorFilters;

        /// <summary>
        /// レンダーマスク
        /// </summary>
        protected List<RenderMaskBase> renderMasks;

        /// <summary>
        /// 位置
        /// </summary>
        public virtual Vector2 Position { get; set; }

        /// <summary>
        /// アルファ
        /// </summary>
        public virtual float Alpha { get; set; }

        /// <summary>
        /// 描画するか
        /// </summary>
        public virtual bool Hidden { get; set; }

        /// <summary>
        /// 回転
        /// </summary>
        public virtual float Rotation { get; set; }

        /// <summary>
        /// 回転中心
        /// </summary>
        public virtual Vector2 RotationCenter { get; set; }

        /// <summary>
        /// 拡大
        /// </summary>
        public virtual Vector2 Scale { get; set; }

        /// <summary>
        /// 拡大中心
        /// </summary>
        public virtual Vector2 ScaleCenter { get; set; }

        /// <summary>
        /// オフセット
        /// </summary>
        public virtual Vector2 Offset { get; set; }

        /// <summary>
        /// 幅
        /// </summary>
        public virtual float Width
        {
            get
            {
                if (childChanged)
                {
                    childChanged = false;
                    Calculate();
                }
                return _width;
            }
            private set
            {
                _width = value;
            }
        }

        /// <summary>
        /// 高さ
        /// </summary>
        public virtual float Height
        {
            get
            {
                if (childChanged)
                {
                    childChanged = false;
                    Calculate();
                }
                return _height;
            }
            private set
            {
                _height = value;
            }
        }

        /// <summary>
        /// マウス操作を受け付けるかどうかを取得、設定します
        /// </summary>
        public bool AcceptMouseOperation
        {
            get
            {
                return acceptMouseOperation;
            }
            set
            {
                acceptMouseOperation = value;
            }
        }

        /// <summary>
        /// 処分されたか
        /// </summary>
        public bool Disposed
        {
            get
            {
                return disposed;
            }
        }

        /// <summary>
        /// クリップ情報。
        /// </summary>
        public ClipInfo Clip
        {
            get;
            set;
        }

        /// <summary>
        /// プレスクリーンフィルターの一覧を取得します。
        /// </summary>
        public List<ScreenFilterBase> PreScreenFilters
        {
            get { return preScreenFilters; }
        }

        /// <summary>
        /// ポストスクリーンフィルターの一覧を取得します。
        /// </summary>
        public List<ScreenFilterBase> PostScreenFilters
        {
            get { return postScreenFilters; }
        }

        /// <summary>
        /// カラースクリーンフィルターの一覧を取得します。
        /// </summary>
        public List<ColorFilterBase> ColorFilters
        {
            get { return colorFilters; }
        }

        /// <summary>
        /// レンダーマスクの一覧を取得します。
        /// </summary>
        public List<RenderMaskBase> RenderMasks
        {
            get { return renderMasks; }
        }

        private bool disposed;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        protected GameComponent(PPDDevice device)
        {
            this.device = device;
            Scale = Vector2.One;
            Alpha = 1;
            AcceptMouseOperation = true;
            children = new List<GameComponent>();
            colorFilters = new List<ColorFilterBase>();
            preScreenFilters = new List<ScreenFilterBase>();
            postScreenFilters = new List<ScreenFilterBase>();
            renderMasks = new List<RenderMaskBase>();
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~GameComponent()
        {
            Dispose(false);
        }

        /// <summary>
        /// 子要素を追加します
        /// </summary>
        /// <param name="component"></param>
        public void AddChild(GameComponent component)
        {
            if (disposed) return;
            if (component == null)
                throw new NullReferenceException("");
            if (component.parent == this) return;
            if (component.parent != null)
            {
                component.parent.RemoveChild(component);
            }
            children.Add(component);
            component.parent = this;
            childChanged = true;
        }

        /// <summary>
        /// 指定したインデックスに挿入します
        /// </summary>
        /// <param name="component"></param>
        /// <param name="index"></param>
        public void InsertChild(GameComponent component, int index)
        {
            if (disposed) return;
            if (component == null)
                throw new NullReferenceException("");
            if (component.parent != null)
            {
                component.parent.RemoveChild(component);
            }
            children.Insert(index, component);
            component.parent = this;
            childChanged = true;
        }

        /// <summary>
        /// 子要素を削除します
        /// </summary>
        /// <param name="component"></param>
        public void RemoveChild(GameComponent component)
        {
            if (disposed) return;
            if (component == null)
                throw new NullReferenceException("");
            if (children.Contains(component))
            {
                children.Remove(component);
                component.parent = null;
            }
            childChanged = true;
        }

        /// <summary>
        /// 子要素を含むか
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public bool ContainsChild(GameComponent component)
        {
            if (disposed) return false;
            if (component == null)
                throw new NullReferenceException("");
            return children.Contains(component);
        }

        /// <summary>
        /// 特定の子要素を取得します
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GameComponent this[int index]
        {
            get
            {
                return children[index];
            }
        }

        /// <summary>
        /// 特定の子要素を取得します
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GameComponent GetChildAt(int index)
        {
            return children[index];
        }

        /// <summary>
        /// 全ての子要素
        /// </summary>
        public IEnumerable<GameComponent> Children
        {
            get
            {
                return children;
            }
        }

        /// <summary>
        /// マスク用のゲームコンポーネントを取得、設定します。
        /// </summary>
        public GameComponent Mask
        {
            get;
            set;
        }

        /// <summary>
        /// マスクタイプを取得、設定します。
        /// </summary>
        public MaskType MaskType
        {
            get;
            set;
        }

        /// <summary>
        /// 親を取得します
        /// </summary>
        public GameComponent Parent
        {
            get
            {
                return parent;
            }
        }

        /// <summary>
        /// 子要素の数
        /// </summary>
        public int ChildrenCount
        {
            get
            {
                return children.Count;
            }
        }

        /// <summary>
        /// ブレンドモード
        /// </summary>
        public BlendMode BlendMode
        {
            get;
            set;
        }

        /// <summary>
        /// 後でまとめて描画するマネージャーインスタンス
        /// </summary>
        public PostDrawManager PostDrawManager
        {
            get;
            set;
        }

        /// <summary>
        /// 子要素のインデックスを取得します
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public int IndexOf(GameComponent component)
        {
            if (component == null)
                throw new NullReferenceException("");
            return children.IndexOf(component);
        }

        /// <summary>
        /// 全ての子要素をクリアします
        /// </summary>
        public void ClearChildren()
        {
            if (disposed) return;
            for (int i = 0; i < children.Count; i++)
            {
                GameComponent gc = children[i];
                gc.parent = null;
            }
            children.Clear();
            childChanged = true;
        }

        /// <summary>
        /// 子要素をクリアして破棄します。
        /// </summary>
        public void ClearDisposeChildren()
        {
            if (disposed) return;
            for (int i = 0; i < children.Count; i++)
            {
                GameComponent gc = children[i];
                gc.parent = null;
                gc.ClearDisposeChildren();
                gc.Dispose();
            }
            children.Clear();
            childChanged = true;
        }

        /// <summary>
        /// 画面上の表示位置を取得します
        /// </summary>
        public Vector2 ScreenPos
        {
            get
            {
                Vector2 ret = Position;
                GameComponent tempParent = parent;
                while (tempParent != null)
                {
                    ret += tempParent.Position;
                    tempParent = tempParent.parent;
                }
                return ret;
            }
        }

        /// <summary>
        /// 表示コンテントを持つかどうかを取得します。
        /// </summary>
        public virtual bool HasDrawContent
        {
            get { return false; }
        }

        private void Calculate()
        {
            if (ChildrenCount > 0)
            {
                float minX = float.PositiveInfinity, minY = float.PositiveInfinity, maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
                for (int i = 0; i < children.Count; i++)
                {
                    GameComponent gc = children[i];
                    minX = Math.Min(minX, gc.Position.X);
                    minY = Math.Min(minY, gc.Position.Y);
                    maxX = Math.Max(maxX, gc.Position.X + gc.Width);
                    maxY = Math.Max(maxY, gc.Position.Y + gc.Height);
                }
                Width = maxX - minX;
                Height = maxY - minY;
            }
            else
            {
                Width = 0;
                Height = 0;
            }
        }

        /// <summary>
        /// 処分する
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを処分する
        /// </summary>
        protected virtual void DisposeResource()
        {
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    DisposeResource();
                    if (this.parent != null)
                    {
                        this.parent.RemoveChild(this);
                        this.parent = null;
                    }
                    for (int i = children.Count - 1; i >= 0; i--)
                    {
                        children[i].Dispose();
                    }
                    children.Clear();
                }
            }
            disposed = true;
        }

        /// <summary>
        /// 各パラメーターをデフォルトにします
        /// </summary>
        public void SetDefault()
        {
            Hidden = childChanged = lastOnMouse = mouseLeftDown = mouseRightDown = mouseMiddleDown = false;
            Scale = Vector2.One;
            lastMousePos = Position = RotationCenter = ScaleCenter = Vector2.Zero;
            Alpha = 1;
            Rotation = 0;
            PreScreenFilters.Clear();
            PostScreenFilters.Clear();
            ColorFilters.Clear();
            RenderMasks.Clear();
        }

        /// <summary>
        /// 表示状態をデフォルトにします
        /// </summary>
        public void HideDefault()
        {
            Hidden = false;
            Alpha = 1;
        }

        #region IUpdatable メンバ

        /// <summary>
        /// 更新する
        /// </summary>
        public virtual void Update()
        {
            if (disposed || !OnCanUpdate() || !OnCanUpdateEvent())
            {
                return;
            }

            UpdateChildren();
            UpdateSelfAfterChildren();
        }

        /// <summary>
        /// 子の更新後に自身を更新します
        /// </summary>
        protected virtual void UpdateSelfAfterChildren()
        {
            if (childChanged)
            {
                childChanged = false;
                Calculate();
            }

            if (lastPosition != Position && Parent != null)
            {
                Parent.childChanged = true;
            }
            lastPosition = Position;
            if (Mask != null)
            {
                Mask.Update();
            }

            UpdateImpl();
        }

        /// <summary>
        /// 自身の更新処理
        /// </summary>
        protected virtual void UpdateImpl()
        {

        }

        /// <summary>
        /// 子の更新処理
        /// </summary>
        protected virtual void UpdateChildren()
        {
            for (int i = children.Count - 1; i >= 0; i--)
            {
                if (OnCanUpdateChild(i) && OnCanUpdateChildEvent(i))
                {
                    children[i].Update();
                }
                if (disposed)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 子要素を更新可能かどうかを返します。
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnCanUpdateChild(int childIndex)
        {
            return true;
        }

        /// <summary>
        /// 子要素を更新可能かどうかをイベントで判定します。
        /// </summary>
        /// <param name="childIndex"></param>
        /// <returns></returns>
        protected virtual bool OnCanUpdateChildEvent(int childIndex)
        {
            CanUpdateChild?.Invoke(this, childIndex);
            return true;
        }

        /// <summary>
        /// 更新可能かどうかを返します
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnCanUpdate()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnCanUpdateEvent()
        {
            if (CanUpdate != null)
            {
                return CanUpdate(this);
            }
            return true;
        }

        /// <summary>
        /// マウスの入力情報にあわせて更新します
        /// </summary>
        /// <param name="mouseInfo"></param>
        public void UpdateMouseInfo(MouseInfo mouseInfo)
        {
            UpdateMouseInfo(mouseInfo, Position + Offset);
        }

        private void UpdateMouseInfo(MouseInfo mouseInfo, Vector2 baseOffset)
        {
            if (!AcceptMouseOperation)
            {
                return;
            }

            var onMouse = HitTest(mouseInfo.Position - baseOffset);
            if (onMouse)
            {
                if (!lastOnMouse)
                {
                    OnMouseEnter(new MouseEvent(mouseInfo.Position, false, MouseEvent.MouseEventType.None));
                }
                if (lastMousePos != mouseInfo.Position)
                {
                    OnMouseMove(new MouseEvent(mouseInfo.Position, false, MouseEvent.MouseEventType.None));
                }
            }
            else
            {
                if (lastOnMouse)
                {
                    OnMouseLeave(new MouseEvent(mouseInfo.Position, false, MouseEvent.MouseEventType.None));
                }
            }
            lastMousePos = mouseInfo.Position;
            lastOnMouse = onMouse;

            foreach (MouseEvent mouseEvent in mouseInfo.Events)
            {
                if (HitTest(mouseEvent.Position - baseOffset))
                {
                    switch (mouseEvent.EventType)
                    {
                        case MouseEvent.MouseEventType.Left:
                            if (mouseEvent.Up)
                            {
                                OnMouseLeftUp(mouseEvent);
                                if (mouseLeftDown)
                                {
                                    OnMouseLeftClick(mouseEvent);
                                }
                                mouseLeftDown = false;
                            }
                            else
                            {
                                mouseLeftDown = true;
                                OnMouseLeftDown(mouseEvent);
                            }
                            break;
                        case MouseEvent.MouseEventType.Middle:
                            if (mouseEvent.Up)
                            {
                                OnMouseMiddleUp(mouseEvent);
                                if (mouseMiddleDown)
                                {
                                    OnMouseMiddleClick(mouseEvent);
                                }
                                mouseMiddleDown = false;
                            }
                            else
                            {
                                mouseMiddleDown = true;
                                OnMouseMiddleDown(mouseEvent);
                            }
                            break;
                        case MouseEvent.MouseEventType.Right:
                            if (mouseEvent.Up)
                            {
                                OnMouseRightUp(mouseEvent);
                                if (mouseRightDown)
                                {
                                    OnMouseRightClick(mouseEvent);
                                }
                                mouseRightDown = false;
                            }
                            else
                            {
                                mouseRightDown = true;
                                OnMouseRightDown(mouseEvent);
                            }
                            break;
                        case MouseEvent.MouseEventType.Wheel:
                            OnMouseWheel(mouseEvent);
                            break;
                    }
                }
                else
                {
                    switch (mouseEvent.EventType)
                    {
                        case MouseEvent.MouseEventType.Left:
                            mouseLeftDown &= !mouseEvent.Up;
                            break;
                        case MouseEvent.MouseEventType.Middle:
                            mouseMiddleDown &= !mouseEvent.Up;
                            break;
                        case MouseEvent.MouseEventType.Right:
                            mouseRightDown &= !mouseEvent.Up;
                            break;
                    }
                }
            }

            for (int i = 0; i < ChildrenCount; i++)
            {
                GameComponent child = children[i];
                child.UpdateMouseInfo(mouseInfo, baseOffset + child.Position + child.Offset);
            }
        }

        /// <summary>
        /// 衝突判定を行います
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public virtual bool HitTest(Vector2 vec)
        {
            return 0 <= vec.X && vec.X <= Width && 0 <= vec.Y && vec.Y <= Height;
        }

        #endregion

        #region IDrawable メンバ

        /// <summary>
        /// 描画する
        /// </summary>
        public virtual void Draw()
        {
            var alphaBlendContext = device.GetModule<AlphaBlendContextCache>().Get();
            alphaBlendContext.Alpha = Alpha;
            alphaBlendContext.BlendMode = BlendMode.Normal;
            Draw(alphaBlendContext, 0, 0);
        }

        /// <summary>
        /// 描画する
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        /// <param name="depth"></param>
        /// <param name="childIndex"></param>
        protected virtual void Draw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            if (disposed || Hidden || alphaBlendContext.Alpha <= 0 || !OnCanDraw(alphaBlendContext, depth, childIndex) || !OnCanDrawEvent(alphaBlendContext, depth, childIndex))
            {
                return;
            }
            if (PostDrawManager != null)
            {
                PostDrawManager.Add(this, alphaBlendContext, depth);
            }
            else
            {
                DrawInternal(alphaBlendContext, depth);
            }
        }

        internal void DrawInternal(AlphaBlendContext alphaBlendContext, int depth)
        {
            if (Clip != null)
            {
                Clip.GameHost.SetClipping(Clip.PositionX, Clip.PositionY, Clip.Width, Clip.Height);
            }
            if (preScreenFilters.Count > 0)
            {
                device.GetModule<AlphaBlend>().Flush();
                foreach (var filter in preScreenFilters)
                {
                    filter.Filter(device);
                }
            }
            if (BlendMode != BlendMode.None)
            {
                alphaBlendContext.BlendMode = BlendMode;
            }
            alphaBlendContext.SetSRT(Matrix.Transformation2D(ScaleCenter, 0, Scale, RotationCenter, Rotation, Position + Offset), depth);
            alphaBlendContext.SRTDepth = depth;
            for (var i = colorFilters.Count - 1; i >= 0; i--)
            {
                alphaBlendContext.SetFilter(colorFilters[i], alphaBlendContext.FilterCount++);
            }
            var currentAlpha = alphaBlendContext.Alpha;
            var currentBlendMode = alphaBlendContext.BlendMode;
            var currentFilterCount = alphaBlendContext.FilterCount;
            var enabledRenderMask = renderMasks.Where(m => m.Enabled).ToArray();

            WorkspaceTexture renderMaskTexture = null;
            if (enabledRenderMask.Length > 0 && !PPDSetting.Setting.ShaderDisabled)
            {
                device.GetModule<AlphaBlend>().Flush();
                var renderTarget = device.GetRenderTarget();
                renderMaskTexture = device.Workspace.Get();
                device.SetRenderTarget(renderMaskTexture);
                device.Clear();
                DrawImpl(alphaBlendContext);
                device.GetModule<AlphaBlend>().Flush();
                device.SetRenderTarget(renderTarget);
            }
            WorkspaceTexture maskTexture = null;
            if (Mask != null && !PPDSetting.Setting.ShaderDisabled)
            {
                device.GetModule<AlphaBlend>().Flush();
                maskTexture = device.Workspace.Get();
                var renderTarget = device.GetRenderTarget();
                device.SetRenderTarget(maskTexture);
                device.Clear(MaskType == MaskType.Include ? new Color4(0, 0, 0, 0) : new Color4(0, 0, 0, 1));
                using (var handler = device.GetModule<AlphaBlend>().StartMaskGeneration(MaskType))
                {
                    Mask.Draw(device.GetModule<AlphaBlendContextCache>().Clone(alphaBlendContext), depth + 1, 0);
                }
                device.SetRenderTarget(renderTarget);
                alphaBlendContext.MaskTexture = maskTexture.Texture;
            }
            WorkspaceTexture filterTargetTexture = null;
            WorkspaceTexture prevRenderTarget = null;
            if (currentFilterCount >= 2 && !PPDSetting.Setting.ShaderDisabled)
            {
                device.GetModule<AlphaBlend>().Flush();
                alphaBlendContext.Alpha = 1;
                alphaBlendContext.BlendMode = BlendMode.Normal;
                filterTargetTexture = device.Workspace.Get();
                prevRenderTarget = device.GetRenderTarget();
                device.SetRenderTarget(filterTargetTexture);
                device.Clear(new Color4(0, 0, 0, 0));
            }
            DrawImpl(alphaBlendContext);
            if (currentFilterCount >= 2 && !PPDSetting.Setting.ShaderDisabled)
            {
                device.GetModule<AlphaBlend>().Flush();
                var context = device.GetModule<AlphaBlendContextCache>().Get();
                context.Alpha = 1;
                context.BlendMode = BlendMode.Normal;
                context.Vertex = device.GetModule<ShaderCommon>().ScreenVertex;
                context.SetSRT(Matrix.Identity, 0);
                WorkspaceTexture renderedTexture = filterTargetTexture;
                for (var i = currentFilterCount - 2; i >= 0; i--)
                {
                    context.Texture = renderedTexture.Texture;
                    var newRenderTarget = device.Workspace.Get();
                    device.SetRenderTarget(newRenderTarget);
                    context.SetFilter(alphaBlendContext.Filters[i], 0);
                    context.FilterCount = 1;
                    device.GetModule<AlphaBlend>().Draw(device, context);
                    renderedTexture.Dispose();
                    renderedTexture = newRenderTarget;
                    if (i == 0)
                    {
                        context.Alpha = currentAlpha;
                        context.BlendMode = currentBlendMode;
                        context.Texture = renderedTexture.Texture;
                        context.FilterCount = 0;
                        device.SetRenderTarget(prevRenderTarget);
                        device.GetModule<AlphaBlend>().Draw(device, context);
                    }
                }
                if (renderedTexture != null)
                {
                    renderedTexture.Dispose();
                }
            }
            if (enabledRenderMask.Length > 0 && !PPDSetting.Setting.ShaderDisabled)
            {
                device.GetModule<AlphaBlend>().Flush();
                foreach (var renderMask in enabledRenderMask)
                {
                    renderMask.Draw(device, renderMaskTexture);
                }
                if (renderMaskTexture != null)
                {
                    renderMaskTexture.Dispose();
                }
            }
            DrawChildrenImpl(alphaBlendContext, currentAlpha, currentBlendMode, currentFilterCount, depth + 1);
            if (Mask != null && !PPDSetting.Setting.ShaderDisabled)
            {
                alphaBlendContext.MaskTexture = null;
                if (maskTexture != null)
                {
                    maskTexture.Dispose();
                }
            }
            alphaBlendContext.SRTDepth = depth;
            alphaBlendContext.Alpha = currentAlpha;
            alphaBlendContext.BlendMode = currentBlendMode;
            alphaBlendContext.FilterCount = currentFilterCount;
            AfterChildenDraw(alphaBlendContext);
            if (postScreenFilters.Count > 0)
            {
                device.GetModule<AlphaBlend>().Flush();
                foreach (var filter in postScreenFilters)
                {
                    filter.Filter(device);
                }
            }
            if (Clip != null)
            {
                Clip.GameHost.RestoreClipping();
            }
        }

        /// <summary>
        /// 子要素を描画します。
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        /// <param name="currentAlpha"></param>
        /// <param name="currentBlendMode"></param>
        /// <param name="currentFilterCount"></param>
        /// <param name="depth"></param>
        protected virtual void DrawChildrenImpl(AlphaBlendContext alphaBlendContext, float currentAlpha, BlendMode currentBlendMode, int currentFilterCount, int depth)
        {
            if (Clip != null)
            {
                Matrix m = Matrix.Identity;
                for (var i = 0; i < alphaBlendContext.SRTDepth + 1; i++)
                {
                    m *= alphaBlendContext.SRTS[i];
                }
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    var top = Vector2.TransformCoordinate(children[i].Position, m).Y;
                    var bottom = Vector2.TransformCoordinate(children[i].Position + new Vector2(0, children[i].Height), m).Y;
                    if ((bottom < Clip.PositionY) || top > (Clip.PositionY + Clip.Height))
                    {
                        continue;
                    }
                    alphaBlendContext.Alpha = currentAlpha * children[i].Alpha;
                    alphaBlendContext.BlendMode = currentBlendMode;
                    alphaBlendContext.FilterCount = currentFilterCount;
                    if (OnCanDrawChild(alphaBlendContext, depth, i) && OnCanDrawChildEvent(alphaBlendContext, depth, i))
                    {
                        children[i].Draw(alphaBlendContext, depth, i);
                    }
                }
            }
            else
            {
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    alphaBlendContext.Alpha = currentAlpha * children[i].Alpha;
                    alphaBlendContext.BlendMode = currentBlendMode;
                    alphaBlendContext.FilterCount = currentFilterCount;
                    if (OnCanDrawChild(alphaBlendContext, depth, i) && OnCanDrawChildEvent(alphaBlendContext, depth, i))
                    {
                        children[i].Draw(alphaBlendContext, depth, i);
                    }
                }
            }
        }

        /// <summary>
        /// 子要素を描画するかどうかを返します。
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        /// <param name="depth"></param>
        /// <param name="childIndex"></param>
        /// <returns></returns>
        protected virtual bool OnCanDrawChild(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return true;
        }

        /// <summary>
        /// 子要素を描画するかどうかをイベントで判定します。
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        /// <param name="depth"></param>
        /// <param name="childIndex"></param>
        /// <returns></returns>
        protected virtual bool OnCanDrawChildEvent(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            if (CanDrawChild != null)
            {
                return CanDrawChild(this, alphaBlendContext, depth, childIndex);
            }
            return true;
        }

        /// <summary>
        /// 描画するかどうかを返します
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        /// <param name="depth"></param>
        /// <param name="childIndex"></param>
        /// <returns></returns>
        protected virtual bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return true;
        }

        /// <summary>
        /// 描画できるかどうかイベントで判定します。
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnCanDrawEvent(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            if (CanDraw != null)
            {
                return CanDraw(this, alphaBlendContext, depth, childIndex);
            }
            return true;
        }

        /// <summary>
        /// 描画の処理
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        protected virtual void DrawImpl(AlphaBlendContext alphaBlendContext)
        {

        }

        /// <summary>
        /// 子を描画した後の描画の処理
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        protected virtual void AfterChildenDraw(AlphaBlendContext alphaBlendContext)
        {

        }

        #endregion

        /// <summary>
        /// マウスの左ボタンが押されたときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseLeftDown(MouseEvent mouseEvent)
        {
            if (MouseLeftDown != null)
            {
                MouseLeftDown.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスの左ボタンが離されたときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseLeftUp(MouseEvent mouseEvent)
        {
            if (MouseLeftUp != null)
            {
                MouseLeftUp.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスの中ボタンが押されたときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseMiddleDown(MouseEvent mouseEvent)
        {
            if (MouseMiddleDown != null)
            {
                MouseMiddleDown.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスの中ボタンが離されたときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseMiddleUp(MouseEvent mouseEvent)
        {
            if (MouseMiddleUp != null)
            {
                MouseMiddleUp.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスの右ボタンが押されたときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseRightDown(MouseEvent mouseEvent)
        {
            if (MouseRightDown != null)
            {
                MouseRightDown.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスの右ボタンが離されたときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseRightUp(MouseEvent mouseEvent)
        {
            if (MouseRightUp != null)
            {
                MouseRightUp.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスのホイールがまわされたときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseWheel(MouseEvent mouseEvent)
        {
            if (MouseWheel != null)
            {
                MouseWheel.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスが動いたときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseMove(MouseEvent mouseEvent)
        {
            if (MouseMove != null)
            {
                MouseMove.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスが入ったときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseEnter(MouseEvent mouseEvent)
        {
            if (MouseEnter != null)
            {
                MouseEnter.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスがでた時の処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseLeave(MouseEvent mouseEvent)
        {
            if (MouseLeave != null)
            {
                MouseLeave.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスの左ボタンでクリックしたときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseLeftClick(MouseEvent mouseEvent)
        {
            if (MouseLeftClick != null)
            {
                MouseLeftClick.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスの右ボタンでクリックしたときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseRightClick(MouseEvent mouseEvent)
        {
            if (MouseRightClick != null)
            {
                MouseRightClick.Invoke(this, mouseEvent);
            }
        }

        /// <summary>
        /// マウスの中ボタンでクリックしたときの処理です
        /// </summary>
        /// <param name="mouseEvent"></param>
        protected void OnMouseMiddleClick(MouseEvent mouseEvent)
        {
            if (MouseMiddleClick != null)
            {
                MouseMiddleClick.Invoke(this, mouseEvent);
            }
        }
    }
}
