using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace KeyConfiger.ViewModels
{
    /// <summary>
    /// 画面の文字です。<c>{Binding Texts[キー]}</c> で今の言語の文字を表示します。言語を変えると表示も変わります。
    /// </summary>
    public class LanguageTexts : ObservableObject
    {
        public string this[string key]
        {
            get { return Utility.Language[key]; }
        }

        /// <summary>
        /// 言語を変えたので、表示し直させます。
        /// </summary>
        public void Refresh()
        {
            OnPropertyChanged("Item[]");
        }
    }

    /// <summary>
    /// 選ぶメニューの 1 項目です（言語、設定、コントローラー）。
    /// </summary>
    public partial class MenuOption<T>(string name, T value) : ObservableObject
    {
        public string Name
        {
            get;
        } = name;

        public T Value
        {
            get;
        } = value;

        [ObservableProperty]
        public partial bool IsChecked { get; set; }
    }

    /// <summary>
    /// リソースの中から DataContext を使うための入れ物です（メニューの CompositeCollection などは DataContext を受け継がない）。
    /// </summary>
    public class BindingProxy : Freezable
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }

    /// <summary>
    /// メニューの中で、ViewModel の一覧から作った項目にだけスタイルを当てます（XAML に書いた MenuItem や Separator には当てない）。
    /// </summary>
    public class OptionStyleSelector : StyleSelector
    {
        public Style OptionStyle
        {
            get;
            set;
        }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            return item is UIElement ? null : OptionStyle;
        }
    }
}
