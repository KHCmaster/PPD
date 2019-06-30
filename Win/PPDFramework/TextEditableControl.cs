using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace PPDFramework
{
    /// <summary>
    /// テキストボックスの選択範囲
    /// </summary>
    public struct TextBoxSelection
    {
        /// <summary>
        /// 選択開始
        /// </summary>
        public int SelectionStart;

        /// <summary>
        /// 選択終了
        /// </summary>
        public int SelectionEnd;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="selStart"></param>
        /// <param name="selEnd"></param>
        public TextBoxSelection(int selStart, int selEnd)
        {
            SelectionStart = selStart;
            SelectionEnd = selEnd;
        }

        /// <summary>
        /// 長さ
        /// </summary>
        public int Length
        {
            get
            {
                return SelectionEnd - SelectionStart;
            }
        }

        /// <summary>
        /// 比較演算子
        /// </summary>
        /// <param name="sel1"></param>
        /// <param name="sel2"></param>
        /// <returns></returns>
        public static bool operator ==(TextBoxSelection sel1, TextBoxSelection sel2)
        {
            return sel1.SelectionStart == sel2.SelectionStart && sel1.SelectionEnd == sel2.SelectionEnd;
        }

        /// <summary>
        /// 反比較演算子
        /// </summary>
        /// <param name="sel1"></param>
        /// <param name="sel2"></param>
        /// <returns></returns>
        public static bool operator !=(TextBoxSelection sel1, TextBoxSelection sel2)
        {
            return !(sel1 == sel2);
        }

        /// <summary>
        /// 比較する
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// ハッシュコードを取得する
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// IMEテキスト編集可能なフォームです
    /// </summary>
    public class TextEditableControl : Control
    {
        /// <summary>
        /// IMEが開始した
        /// </summary>
        public event EventHandler IMEStarted;

        /// <summary>
        /// 有効状態が変更された
        /// </summary>
        public event EventHandler _EnabledChanged;

        TextBuffer buffer;
        TextBoxSelection selection;
        int caretIndex;
        bool isTextEditMode;
        Dictionary<Keys, Action> actionList;
        ImeMode lastIMEMode;
        ImeMode offModeIME;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TextEditableControl()
        {
            buffer = new TextBuffer();
            Size = new Size(800, 450);
            ImeMode = ImeMode.Disable;
            actionList = new Dictionary<Keys, Action>
            {
                { Keys.Left, MoveLeft },
                { Keys.Right, MoveRight },
                { Keys.Left | Keys.Shift, MoveSelectionLeft },
                { Keys.Right | Keys.Shift, MoveSelectionRight },
                { Keys.Back, BackSpace },
                { Keys.Delete, Delete }
            };

            ChangeImeMode(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            ParentChanged += TextEditableControl_ParentChanged;
        }

        void TextEditableControl_ParentChanged(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form != null)
            {
                form.InputLanguageChanged += form_InputLanguageChanged;
            }
        }

        void form_InputLanguageChanged(object sender, InputLanguageChangedEventArgs e)
        {
            ChangeImeMode(e.Culture.TwoLetterISOLanguageName);
        }

        private void ChangeImeMode(string iso)
        {
            switch (iso)
            {
                case "ja":
                    offModeIME = ImeMode.Off;
                    break;
                case "ko":
                    offModeIME = ImeMode.NoControl;
                    break;
                case "zh":
                    offModeIME = ImeMode.NoControl;
                    break;
            }
            lastIMEMode = offModeIME;
        }

        private void MoveLeft()
        {
            if (selection.Length == 0)
            {
                selection.SelectionStart--;
                if (selection.SelectionStart < 0) selection.SelectionStart = 0;
                selection.SelectionEnd = selection.SelectionStart;
            }
            else
            {
                selection.SelectionEnd = selection.SelectionStart;
            }
            caretIndex = selection.SelectionStart;
        }

        private void MoveRight()
        {
            if (selection.Length == 0)
            {
                selection.SelectionEnd++;
                if (selection.SelectionEnd >= buffer.Length) selection.SelectionEnd = buffer.Length;
                selection.SelectionStart = selection.SelectionEnd;
            }
            else
            {
                selection.SelectionStart = selection.SelectionEnd;
            }
            caretIndex = selection.SelectionEnd;
        }

        private void MoveSelectionLeft()
        {
            if (selection.Length == 0)
            {
                selection.SelectionStart--;
                if (selection.SelectionStart < 0) selection.SelectionStart = 0;
                caretIndex = selection.SelectionStart;
            }
            else
            {
                if (caretIndex == selection.SelectionStart)
                {
                    selection.SelectionStart--;
                    if (selection.SelectionStart < 0) selection.SelectionStart = 0;
                    caretIndex = selection.SelectionStart;
                }
                else
                {
                    selection.SelectionEnd--;
                    caretIndex = selection.SelectionEnd;
                }
            }
        }

        private void MoveSelectionRight()
        {
            if (selection.Length == 0)
            {
                selection.SelectionEnd++;
                if (selection.SelectionEnd >= buffer.Length) selection.SelectionEnd = buffer.Length;
                caretIndex = selection.SelectionEnd;
            }
            else
            {
                if (caretIndex == selection.SelectionStart)
                {
                    selection.SelectionStart++;
                    caretIndex = selection.SelectionStart;
                }
                else
                {
                    selection.SelectionEnd++;
                    if (selection.SelectionEnd >= buffer.Length) selection.SelectionEnd = buffer.Length;
                    caretIndex = selection.SelectionEnd;
                }
            }
        }

        private void Delete()
        {
            if (selection.Length > 0)
            {
                buffer.RemoveChars(selection.SelectionStart, selection.Length);
                selection.SelectionEnd = selection.SelectionStart;
            }
            else if (selection.SelectionStart < buffer.Length)
            {
                buffer.RemoveChars(selection.SelectionStart, 1);
            }
            caretIndex = selection.SelectionStart;
        }

        private void BackSpace()
        {
            if (selection.Length > 0)
            {
                buffer.RemoveChars(selection.SelectionStart, selection.Length);
                selection.SelectionEnd = selection.SelectionStart;
            }
            else if (selection.SelectionStart > 0)
            {
                buffer.RemoveChars(selection.SelectionStart - 1, 1);
                selection.SelectionStart--;
                selection.SelectionEnd = selection.SelectionStart;
            }
            caretIndex = selection.SelectionStart;
        }


        /// <summary>
        /// インプットキーを取得
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool IsInputKey(Keys keyData)
        {
            if (!isTextEditMode) return false;
            if (actionList.ContainsKey(keyData))
            {
                actionList[keyData].Invoke();
                return true;
            }
            return false;
        }

        private void ReplaceString(string str)
        {
            if (selection.Length > 0)
            {
                buffer.RemoveChars(selection.SelectionStart, selection.Length);
                selection.SelectionEnd = selection.SelectionStart;
            }
            buffer.InsertString(str, selection.SelectionStart);
            selection.SelectionStart += str.Length;
            selection.SelectionEnd = selection.SelectionStart;
            caretIndex = selection.SelectionStart;
        }

        /// <summary>
        /// 押されたキーを取得
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (isTextEditMode)
            {
                if (' ' <= e.KeyChar && e.KeyChar <= '~')
                {
                    if (ImeMode == offModeIME || ImeMode == ImeMode.Alpha)
                    {
                        ReplaceString(e.KeyChar.ToString());
                    }
                }
                else if (e.KeyChar == '\r' || e.KeyChar == '\n' || e.KeyChar == 0x1b)
                {
                    EnterClosed = e.KeyChar == '\r' || e.KeyChar == '\n';
                    IsTextEditMode = false;
                }
                else if (e.KeyChar == 22)
                {
                    try
                    {
                        if (Clipboard.ContainsText())
                        {
                            ReplaceString(Clipboard.GetText());
                        }
                    }
                    catch
                    {
                    }
                }
                else if (e.KeyChar == 3)
                {
                    try
                    {
                        var text = buffer.GetText(selection.SelectionStart, selection.Length);
                        Clipboard.SetText(text);
                    }
                    catch
                    {
                    }
                }
                else if (e.KeyChar == 1)
                {
                    selection.SelectionStart = 0;
                    selection.SelectionEnd = buffer.Length;
                }
            }
        }

        /// <summary>
        /// WndProcのオーバーライド
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            try
            {
                switch (m.Msg)
                {
                    case WinAPI.WM_IME_COMPOSITION:
                        if (IsTextEditMode)
                        {
                            if ((m.LParam.ToInt32() & WinAPI.GCS_RESULTSTR) != 0)
                            {
                                var str = GetIMEString(WinAPI.GCS_RESULTSTR);
                                ReplaceString(str);
                            }
                            else if ((m.LParam.ToInt32() & WinAPI.GCS_COMPSTR) != 0)
                            {
                                //Console.WriteLine(GetIMEString(WinAPI.GCS_COMPSTR));
                            }
                        }

                        break;
                    case WinAPI.WM_IME_STARTCOMPOSITION:
                        OnIMEStart();
                        UpdateCaret();
                        WinAPI.SetImeWindowFont(Handle, Font);
                        break;
                    case WinAPI.WM_IME_ENDCOMPOSITION:
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            base.WndProc(ref m);
        }


        private string GetIMEString(int gcs)
        {
            string text;
            IntPtr ime;
            int len;
            ime = WinAPI.ImmGetContext(Handle);
            len = WinAPI.ImmGetCompositionStringW(ime, gcs, null, 0);
            byte[] buf = new byte[len];
            WinAPI.ImmGetCompositionStringW(ime, gcs, buf, len);
            text = System.Text.Encoding.Unicode.GetString(buf, 0, buf.Length);
            WinAPI.ImmReleaseContext(Handle, ime);
            return text;
        }

        /// <summary>
        /// テキスト編集モードか
        /// </summary>
        public bool IsTextEditMode
        {
            get
            {
                return isTextEditMode;
            }
            set
            {
                if (isTextEditMode != value)
                {
                    isTextEditMode = value;
                    if (isTextEditMode)
                    {
                        EnterClosed = false;
                        ImeMode = lastIMEMode;
                        this.Focus();
                        UpdateCaret();
                    }
                    else
                    {
                        lastIMEMode = ImeMode;
                        ImeMode = offModeIME;
                    }
                    this._OnEnabledChanged();
                }
            }
        }

        /// <summary>
        /// エンターキーで閉じられたか
        /// </summary>
        public bool EnterClosed
        {
            get;
            private set;
        }

        /// <summary>
        /// フォーカスされた時の処理です
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (IsTextEditMode)
            {
                UpdateCaret();
            }
        }

        /// <summary>
        /// 有効状態が変更されたときの処理です
        /// </summary>
        protected void _OnEnabledChanged()
        {
            if (_EnabledChanged != null)
            {
                _EnabledChanged.Invoke(this, EventArgs.Empty);
            }
        }

        private void UpdateCaret()
        {
            // move IMM window to caret position
            WinAPI.SetImeWindowPos(Handle, TextBoxLocation);
        }

        /// <summary>
        /// テキストボックスの位置
        /// </summary>
        public Point TextBoxLocation
        {
            get;
            set;
        }

        /// <summary>
        /// テキストボックスのテキスト
        /// </summary>
        public String TextBoxText
        {
            get
            {
                return buffer.ToString();
            }
            set
            {
                buffer.Text = value;
                selection.SelectionStart = selection.SelectionEnd = buffer.Length;
                caretIndex = buffer.Length;
            }
        }

        /// <summary>
        /// 選択範囲
        /// </summary>
        public TextBoxSelection Selection
        {
            get
            {
                return selection;
            }
            set
            {
                selection = value;
            }
        }

        /// <summary>
        /// キャレットの位置
        /// </summary>
        public int CaretIndex
        {
            get
            {
                return caretIndex;
            }
            set
            {
                caretIndex = value;
            }
        }

        /// <summary>
        /// IME開始した
        /// </summary>
        protected void OnIMEStart()
        {
            if (IMEStarted != null)
            {
                IMEStarted.Invoke(this, EventArgs.Empty);
            }
        }

        internal class TextBuffer
        {
            private List<char> str;
            public TextBuffer()
            {
                str = new List<char>();
            }

            public void AppendString(string st)
            {
                foreach (Char c in st)
                {
                    str.Add(c);
                }
            }

            public void AppendChar(Char c)
            {
                str.Add(c);
            }

            public void InsertString(string st, int index)
            {
                int iter = 0;
                foreach (Char c in st)
                {
                    InsertChar(c, index + iter);
                    iter++;
                }
            }

            public void InsertChar(Char c, int index)
            {
                str.Insert(index, c);
            }

            public void RemoveChars(int index, int length)
            {
                str.RemoveRange(index, length);
            }

            public override string ToString()
            {
                return new string(str.ToArray());
            }

            public string GetText(int begin, int length)
            {
                var sb = new StringBuilder();

                for (int i = begin; i < begin + length; i++)
                {
                    sb.Append(str[i]);
                }

                return sb.ToString();
            }

            public int Length
            {
                get
                {
                    return str.Count;
                }
            }

            public string Text
            {
                get
                {
                    return ToString();
                }
                set
                {
                    str.Clear();
                    InsertString(value, 0);
                }
            }
        }
    }
}
