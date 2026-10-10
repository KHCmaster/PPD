using System.Linq;

namespace PPDFramework
{
    /// <summary>
    /// 入力デリゲートです
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void InputEventHandler(IFocusable sender, InputEventArgs args);
    /// <summary>
    /// フォーカスデリゲートです
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void FocusEventHandler(IFocusable sender, FocusEventArgs args);

    /// <summary>
    /// 入力イベント引数です
    /// </summary>
    /// <remarks>
    /// コンストラクタです
    /// </remarks>
    /// <param name="inputInfo"></param>
    public class InputEventArgs(InputInfoBase inputInfo)
    {

        /// <summary>
        /// 入力情報を取得します。
        /// </summary>
        public InputInfoBase InputInfo
        {
            get;
            private set;
        } = inputInfo;

        /// <summary>
        /// どれかのボタンが押されたかどうか
        /// </summary>
        public bool AnyPressed
        {
            get
            {
                return ButtonUtility.Array.Any(InputInfo.IsPressed);
            }
        }

        /// <summary>
        /// どれかのボタンが離されたかどうか
        /// </summary>
        public bool AnyReleased
        {
            get
            {
                return ButtonUtility.Array.Any(InputInfo.IsReleased);
            }
        }

        /// <summary>
        /// 入力を親フォーカスに伝播させないようにします
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }
    }

    /// <summary>
    /// フォーカスイベント引数です
    /// </summary>
    /// <remarks>
    /// コンストラクタです
    /// </remarks>
    /// <param name="ifObj"></param>
    public class FocusEventArgs(IFocusable ifObj)
    {

        /// <summary>
        /// フォーカス対象のオブジェクトです
        /// </summary>
        public IFocusable FocusObject
        {
            get;
            private set;
        } = ifObj;
    }

    /// <summary>
    /// フォーカス可能なGameComponentです
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="device"></param>
    public class FocusableGameComponent(PPDDevice device) : GameComponent(device), IFocusable
    {
        /// <summary>
        /// フォーカスが与えられた時のイベントです
        /// </summary>
        public event FocusEventHandler GotFocused;
        /// <summary>
        /// フォーカスが除かれた時のイベントです
        /// </summary>
        public event FocusEventHandler LostFocused;
        /// <summary>
        /// 入力があったときのイベントです
        /// </summary>
        public event InputEventHandler Inputed;

        /// <summary>
        /// フォーカスされたときに呼ばれます
        /// </summary>
        /// <param name="focusObject"></param>
        protected virtual void OnFocus(IFocusable focusObject)
        {
            GotFocused?.Invoke(this, new FocusEventArgs(focusObject));
        }

        /// <summary>
        /// フォーカスを除かれたときに呼ばれます
        /// </summary>
        /// <param name="focusObject"></param>
        protected virtual void OnLostFocus(IFocusable focusObject)
        {
            LostFocused?.Invoke(this, new FocusEventArgs(focusObject));
        }

        /// <summary>
        /// 入力を処理する時によばれます
        /// </summary>
        /// <param name="inputInfo"></param>
        protected virtual bool OnProcessInput(InputInfoBase inputInfo)
        {
            if (Inputed != null)
            {
                var e = new InputEventArgs(inputInfo);
                Inputed.Invoke(this, e);
                return e.Handled;
            }

            return false;
        }

        #region IFocusable メンバ

        FocusManager temp;
        /// <summary>
        /// フォーカスマネージャを取得、設定します
        /// </summary>
        public FocusManager FocusManager
        {
            get
            {
                return temp;
            }
            set
            {
                temp = value;
            }
        }

        bool _focused;
        /// <summary>
        /// フォーカスされたかどうかを取得、設定します
        /// </summary>
        public bool Focused
        {
            get
            {
                return _focused;
            }
            set
            {
                if (_focused != value)
                {
                    _focused = value;
                    if (_focused)
                    {
                        OnFocus(FocusManager.LastFocusObject);
                    }
                    else
                    {
                        OnLostFocus(FocusManager.LastFocusObject);
                    }
                }
            }
        }

        /// <summary>
        /// オーバーフォーカス時に入力を受け取るかどうかを取得、設定します
        /// </summary>
        public bool HandleOverFocusInput
        {
            get;
            set;
        }

        /// <summary>
        /// フォーカススタックに含まれているかを取得、設定します
        /// </summary>
        public bool OverFocused
        {
            get;
            set;
        }

        /// <summary>
        /// 入力を処理します
        /// </summary>
        /// <param name="inputInfo"></param>
        public bool ProcessInput(InputInfoBase inputInfo)
        {
            return OnProcessInput(inputInfo);
        }

        #endregion
    }
}
