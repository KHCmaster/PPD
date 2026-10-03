using PPDEditor.Controls;
using System;
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
        /// メニューのショートカットキーです。ゲームの画面（WPF の外のウィンドウ）で押したときにも使う。
        /// </summary>
        private bool ProcessMenuShortcut(Key key, ModifierKeys modifiers)
        {
            System.Windows.Controls.MenuItem target = (modifiers, key) switch
            {
                (ModifierKeys.Control, Key.S) => プロジェクトを上書き保存ToolStripMenuItem,
                (ModifierKeys.Control, Key.G) => 現在の時間から開始ToolStripMenuItem,
                (ModifierKeys.Control | ModifierKeys.Shift, Key.G) => 最初から開始ToolStripMenuItem,
                (ModifierKeys.Control, Key.D1) => 現在の時間から開始オートToolStripMenuItem,
                (ModifierKeys.Control | ModifierKeys.Shift, Key.D1) => 最初から開始オートToolStripMenuItem,
                _ => null,
            };
            if (target == null || !target.IsEnabled || !ゲームToolStripMenuItem.IsEnabled && target != プロジェクトを上書き保存ToolStripMenuItem)
            {
                return false;
            }
            target.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent, target));
            return true;
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
            ChangeMousePos(new Point(-1, -1));
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
                PlayOrPause();
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
        /// </remarks>
        private void AddGameWindowMenu()
        {
            AddGameWindowMenu(dockPanel1.AnchorableContextMenu);
            AddGameWindowMenu(dockPanel1.DocumentContextMenu);
        }

        private void AddGameWindowMenu(System.Windows.Controls.ContextMenu menu)
        {
            if (menu == null)
            {
                return;
            }
            var separator = new System.Windows.Controls.Separator();
            var items = new System.Windows.Controls.Control[] { 画面比を169にMenuItem, 標準解像度にするMenuItem, separator };
            // 同じ項目を 2 つのメニューに入れることはできないので、メニューごとに作る
            if (画面比を169にMenuItem.Parent != null)
            {
                var aspect = new System.Windows.Controls.MenuItem { Header = 画面比を169にMenuItem.Header };
                aspect.Click += 画面比を169にMenuItem_Click;
                var standard = new System.Windows.Controls.MenuItem { Header = 標準解像度にするMenuItem.Header };
                standard.Click += 標準解像度にするMenuItem_Click;
                items = [aspect, standard, separator];
                extraGameWindowMenuItems.Add(aspect);
                extraGameWindowMenuItems.Add(standard);
            }
            for (int i = 0; i < items.Length; i++)
            {
                menu.Items.Insert(i, items[i]);
            }
            menu.Opened += (sender, e) =>
            {
                var isGameWindow = (menu.DataContext as AvalonDock.Controls.LayoutItem)?.Model == dxfDock;
                foreach (var item in items)
                {
                    item.Visibility = isGameWindow ? Visibility.Visible : Visibility.Collapsed;
                }
                // 画面の大きさを変えるのは、浮かせているときだけ
                items[0].IsEnabled = items[1].IsEnabled = dxfDock.IsFloating;
            };
        }

        readonly System.Collections.Generic.List<System.Windows.Controls.MenuItem> extraGameWindowMenuItems = [];

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
            var window = Window.GetWindow(GameView);
            if (window == null || window == this)
            {
                return;
            }
            window.Width += width - GameView.ClientWidth;
            window.Height += height - GameView.ClientHeight;
        }
    }
}
