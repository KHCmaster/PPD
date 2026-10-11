using PPDEditor.Controls;
using PPDEditor.ViewModels;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.Windows.Input;
using Keys = System.Windows.Forms.Keys;
using Point = Vortice.Mathematics.Int2;

namespace PPDEditor.Forms
{
    /// <summary>
    /// ゲームの画面（以前の DXForm）の操作と、メニューのショートカットキーです。
    /// </summary>
    public partial class EditorForm
    {
        /// <summary>
        /// メニューのショートカットキー（ウィンドウの InputBindings）を実行します。ゲームの画面（WPF の外のウィンドウ）で押したときにも使う。
        /// </summary>
        private bool ProcessMenuShortcut(Key key, ModifierKeys modifiers)
        {
            foreach (var binding in InputBindings.OfType<KeyBinding>())
            {
                if (binding.Key == key && binding.Modifiers == modifiers && binding.Command != null)
                {
                    if (!binding.Command.CanExecute(binding.CommandParameter))
                    {
                        return false;
                    }
                    binding.Command.Execute(binding.CommandParameter);
                    return true;
                }
            }
            return false;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (ProcessMenuShortcut(key, Keyboard.Modifiers))
            {
                e.Handled = true;
            }
        }

        private Point GetGameViewMousePosition()
        {
            NativeMethods.GetCursorPos(out var point);
            NativeMethods.ScreenToClient(GameView.RenderHandle, ref point);
            return new Point(point.X, point.Y);
        }

        private void Movemark(object sender, EventArgs e)
        {
            WindowUtility.Seekmain.MoveMark(GetGameViewMousePosition(), GameView.ClientWidth, GameView.ClientHeight);
        }

        private void GameView_GameMouseMove(object sender, Point e)
        {
            WindowUtility.Seekmain.OnMouseMove(e, GameView.ClientWidth, GameView.ClientHeight);
        }

        private void GameView_GameMouseLeave(object sender, EventArgs e)
        {
            ViewModel.ChangeMousePos(new Point(-1, -1));
        }

        private void GameView_GameMouseDown(object sender, Point e)
        {
            moveMarkTimer.Start();
        }

        private void GameView_GameMouseUp(object sender, Point e)
        {
            moveMarkTimer.Stop();
            WindowUtility.Seekmain.FinishMarkEdit();
        }

        private void GameView_GameKeyDown(object sender, GameKeyEventArgs e)
        {
            if (ProcessMenuShortcut(e.Key, e.Modifiers))
            {
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Space && e.Modifiers == ModifierKeys.None)
            {
                ViewModel.PlayOrPause();
                e.Handled = true;
                return;
            }
            var keyCode = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            WindowUtility.TimeLineForm.DxFormPreviewKeyDown(keyCode, e.Modifiers.HasFlag(ModifierKeys.Control),
                e.Modifiers.HasFlag(ModifierKeys.Shift), e.Modifiers.HasFlag(ModifierKeys.Alt));
            e.Handled = true;
        }

        /// <summary>
        /// ゲームウィンドウのタブ（浮かせたときはタイトルバー）の右クリックのメニューに、画面の大きさを変える項目を加える（以前と同じ）。
        /// </summary>
        /// <remarks>
        /// AvalonDock のメニューはすべてのウィンドウで共通なので、開いたときに、ゲームウィンドウのときだけ項目を出す。
        /// ゲームウィンドウはドキュメントの領域にも置けるので、ドキュメントのメニューにも加える。
        /// テーマのメニューは、このアプリでは DockingManager に設定されない（タブを右クリックしても何も出ない）ので、
        /// AvalonDock のテーマのファイルから読み込んで、自分で設定する。
        /// </remarks>
        private void AddGameWindowMenu()
        {
            var theme = new ResourceDictionary
            {
                Source = new Uri("/AvalonDock;component/themes/generic.xaml", UriKind.Relative)
            };
            var anchorableMenu = (System.Windows.Controls.ContextMenu)theme["AnchorableContextMenu"];
            var documentMenu = (System.Windows.Controls.ContextMenu)theme["DocumentContextMenu"];
            AddGameWindowMenu(anchorableMenu);
            AddGameWindowMenu(documentMenu);
            dockPanel1.AnchorableContextMenu = anchorableMenu;
            dockPanel1.DocumentContextMenu = documentMenu;
        }

        private void AddGameWindowMenu(System.Windows.Controls.ContextMenu menu)
        {
            if (menu == null)
            {
                return;
            }
            // 同じ項目を 2 つのメニューに入れることはできないので、メニューごとに作る
            var aspect = CreateGameWindowMenuItem("ChangeAspect", 画面比を169にMenuItem_Click);
            var standard = CreateGameWindowMenuItem("GWDefaultResolution", 標準解像度にするMenuItem_Click);
            var items = new System.Windows.Controls.Control[] { aspect, standard, new System.Windows.Controls.Separator() };
            for (int i = 0; i < items.Length; i++)
            {
                menu.Items.Insert(i, items[i]);
            }
            menu.Opened += (sender, e) =>
            {
                // LayoutItem.Model は、ウィンドウの中身（ゲームの画面）
                var isGameWindow = (menu.DataContext as AvalonDock.Controls.LayoutItem)?.Model == GameView;
                foreach (var item in items)
                {
                    item.Visibility = isGameWindow ? Visibility.Visible : Visibility.Collapsed;
                }
                // 画面の大きさを変えるのは、浮かせているときだけ
                aspect.IsEnabled = standard.IsEnabled = docks[DockKind.GameWindow].IsFloating;
            };
        }

        /// <summary>
        /// ゲームウィンドウのメニューの項目を作ります。表示する文字は今の言語の文字にする。
        /// </summary>
        private static System.Windows.Controls.MenuItem CreateGameWindowMenuItem(string textKey, RoutedEventHandler click)
        {
            var item = new System.Windows.Controls.MenuItem();
            item.SetBinding(HeaderedItemsControl.HeaderProperty, new Binding("[" + textKey + "]") { Source = LanguageTexts.Instance });
            item.Click += click;
            return item;
        }

        private void 画面比を169にMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var width = GameView.ClientWidth;
            var height = GameView.ClientHeight;
            if ((float)width / height > 16f / 9)
            {
                ChangeGameViewSize((int)(height * 16f / 9), height);
            }
            else
            {
                ChangeGameViewSize(width, (int)(width * 9f / 16));
            }
        }

        private void 標準解像度にするMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeGameViewSize(800, 450);
        }

        /// <summary>
        /// 浮かせているゲームの画面のウィンドウの大きさを、画面が指定の大きさになるように変える。
        /// </summary>
        private void ChangeGameViewSize(int width, int height)
        {
            // AvalonDock は中身を論理ツリーではメインのウィンドウの下に置き、浮かせたウィンドウでは中身を別の HwndSource に入れるので、
            // Window.GetWindow でも PresentationSource でも浮かせたウィンドウを取れない。ゲームの画面の一番上の親のウィンドウから探す
            const uint GA_ROOT = 2;
            var root = NativeMethods.GetAncestor(GameView.RenderHandle, GA_ROOT);
            if (System.Windows.Interop.HwndSource.FromHwnd(root)?.RootVisual is not Window window || window == this)
            {
                return;
            }
            window.Width += width - GameView.ClientWidth;
            window.Height += height - GameView.ClientHeight;
        }
    }
}
