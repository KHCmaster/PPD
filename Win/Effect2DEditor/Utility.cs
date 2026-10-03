using AvalonDock.Layout;
using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Effect2DEditor
{
    class Utility
    {
        /// <summary>
        /// 画像として読み込めるかを確かめます。読み込めないときは例外を投げます（ファイルは開いたままにしない）。
        /// </summary>
        public static void ValidateImage(string fileName)
        {
            using var stream = File.OpenRead(fileName);
            BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }

        /// <summary>
        /// アプリケーションの画像（img フォルダー）を読み込みます。
        /// </summary>
        public static BitmapImage LoadImage(string fileName)
        {
            var image = new BitmapImage(new Uri("pack://application:,,,/img/" + fileName));
            image.Freeze();
            return image;
        }

        /// <summary>
        /// 「表示」メニューからウィンドウを表示・非表示にします。Shift を押していたら別のウィンドウにします。
        /// </summary>
        public static void ShowOrHideWindow(LayoutAnchorable anchorable)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                anchorable.Show();
                anchorable.Float();
                anchorable.IsActive = true;
                return;
            }
            if (!anchorable.IsVisible || anchorable.IsAutoHidden)
            {
                anchorable.Show();
                anchorable.IsActive = true;
            }
            else if (anchorable.IsActive || anchorable.IsFloating)
            {
                anchorable.Hide();
            }
            else
            {
                anchorable.IsActive = true;
            }
        }
    }
}
