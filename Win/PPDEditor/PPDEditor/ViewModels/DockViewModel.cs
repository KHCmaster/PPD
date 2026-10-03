using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PPDEditor.ViewModels
{
    /// <summary>
    /// ドックの ViewModel の基底クラスです。
    /// </summary>
    /// <remarks>
    /// タブの表示名と、保存していない変更の印（「*」）は、メインのウィンドウが扱うのでドックの画面（DockView）が持つ。
    /// 内容を変えたときは <see cref="Changed"/> で画面に知らせる。
    /// </remarks>
    public abstract class DockViewModel : ObservableObject
    {
        /// <summary>
        /// 内容を変えたとき（保存していない変更ができたとき）に起こります。
        /// </summary>
        public event EventHandler Changed;

        public LanguageTexts Texts
        {
            get { return LanguageTexts.Instance; }
        }

        protected void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
