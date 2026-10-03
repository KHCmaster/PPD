using Effect2D;
using PPDEditor.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PPDEditor.Controls.Resource
{
    public partial class EffectPanel : UserControl
    {
        private EffectManager effectManager;
        private Dictionary<string, BitmapSource> dict;
        private readonly DispatcherTimer timer;

        private readonly Dictionary<int, int> sizeDict = new()
        {
            {0,256},
            {1,384},
            {2,512},
            {3,768},
            {4,1024}
        };

        bool ignore;

        public EffectPanel()
        {
            InitializeComponent();
            canvasPanel.Panel = this;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000 / 60)
            };
            timer.Tick += Timer_Tick;

            ignore = true;
            SetBackColorItems(["デフォルト", "黒", "白", "カスタム"]);
            toolStripComboBox1.SelectedIndex = PPDStaticSetting.CanvasSizeIndex;
            toolStripComboBox2.SelectedIndex = PPDStaticSetting.CanvasColorIndex;
            ignore = false;
        }

        public void SetLang()
        {
            playToolStripMenuItem.Header = Utility.Language["PlayOnce"];
            reversePlayToolStripMenuItem.Header = Utility.Language["PlayReverce"];
            loopPlayToolStripMenuItem.Header = Utility.Language["PlayLoop"];
            reverseLoopPlayToolStripMenuItem.Header = Utility.Language["PlayLoopReverse"];
            toolStripButton1.ToolTip = Utility.Language["Stop"];
            toolStripComboBox1.ToolTip = Utility.Language["CanvasSize"];
            toolStripComboBox2.ToolTip = Utility.Language["CanvasBackColor"];
            int prevSelect = toolStripComboBox2.SelectedIndex;
            ignore = true;
            SetBackColorItems([Utility.Language["Default"], Utility.Language["Black"], Utility.Language["White"], Utility.Language["Custom"]]);
            toolStripComboBox2.SelectedIndex = prevSelect;
            ignore = false;
        }

        /// <summary>
        /// 背景色の選択肢を、色の見本を付けて作る。
        /// </summary>
        private void SetBackColorItems(string[] names)
        {
            toolStripComboBox2.Items.Clear();
            for (int i = 0; i < names.Length; i++)
            {
                var swatch = new Border
                {
                    Width = 16,
                    Height = 16,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(0, 0, 4, 0),
                    Background = i switch
                    {
                        0 => CreateCheckBrush(4),
                        1 => Brushes.Black,
                        2 => Brushes.White,
                        _ => WpfSkin.ToBrush(PPDStaticSetting.CustomCanvasBackColor),
                    }
                };
                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                panel.Children.Add(swatch);
                panel.Children.Add(new TextBlock { Text = names[i], VerticalAlignment = VerticalAlignment.Center });
                toolStripComboBox2.Items.Add(new ComboBoxItem { Content = panel });
            }
        }

        /// <summary>
        /// 白と灰色の市松模様のブラシを作る。
        /// </summary>
        internal static Brush CreateCheckBrush(double size)
        {
            var group = new DrawingGroup();
            group.Children.Add(new GeometryDrawing(Brushes.White, null, new RectangleGeometry(new Rect(0, 0, size * 2, size * 2))));
            var gray = new GeometryGroup();
            gray.Children.Add(new RectangleGeometry(new Rect(size, 0, size, size)));
            gray.Children.Add(new RectangleGeometry(new Rect(0, size, size, size)));
            group.Children.Add(new GeometryDrawing(Brushes.LightGray, null, gray));
            var brush = new DrawingBrush(group)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, size * 2, size * 2),
                ViewportUnits = BrushMappingMode.Absolute
            };
            brush.Freeze();
            return brush;
        }

        private void EffectPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void ToolStripSplitButton1_Click(object sender, RoutedEventArgs e)
        {
            if (effectManager == null)
            {
                toolStripSplitButton1.IsChecked = false;
                return;
            }
            if (toolStripSplitButton1.IsChecked == true)
            {
                Play();
            }
            else
            {
                Pause();
                ChangeImage();
            }
        }

        private void PlayModeButton_Click(object sender, RoutedEventArgs e)
        {
            playModeMenu.PlacementTarget = toolStripSplitButton1;
            playModeMenu.Placement = PlacementMode.Bottom;
            playModeMenu.IsOpen = true;
        }

        private void ChangeImage()
        {
            var menuItem = GetCheckedMenu();
            if (menuItem?.Icon is Image icon)
            {
                toolStripSplitButton1Image.Source = icon.Source;
            }
        }

        private MenuItem GetCheckedMenu()
        {
            if (playToolStripMenuItem.IsChecked) return playToolStripMenuItem;
            else if (reversePlayToolStripMenuItem.IsChecked) return reversePlayToolStripMenuItem;
            else if (loopPlayToolStripMenuItem.IsChecked) return loopPlayToolStripMenuItem;
            else if (reverseLoopPlayToolStripMenuItem.IsChecked) return reverseLoopPlayToolStripMenuItem;
            return null;
        }

        /// <summary>
        /// キャンバスに書く。座標の原点はキャンバスの真ん中。
        /// </summary>
        internal void DrawCanvas(DrawingContext drawingContext, double width, double height)
        {
            DrawBack(drawingContext, width, height);
            drawingContext.PushTransform(new TranslateTransform(width / 2, height / 2));
            effectManager?.Draw((fn, state) =>
            {
                Draw(drawingContext, fn, state);
            });
            DrawCenter(drawingContext, width, height);
            drawingContext.Pop();
        }

        private void DrawBack(DrawingContext drawingContext, double width, double height)
        {
            Brush brush = toolStripComboBox2.SelectedIndex switch
            {
                0 => CreateCheckBrush(10),
                1 => Brushes.Black,
                2 => Brushes.White,
                3 => WpfSkin.ToBrush(PPDStaticSetting.CustomCanvasBackColor),
                _ => null,
            };
            if (brush != null)
            {
                drawingContext.DrawRectangle(brush, null, new Rect(0, 0, width, height));
            }
        }

        private static void DrawCenter(DrawingContext drawingContext, double width, double height)
        {
            var pen = new Pen(Brushes.Black, 1);
            pen.Freeze();
            drawingContext.DrawLine(pen, new Point(0.5, -height / 2), new Point(0.5, height / 2));
            drawingContext.DrawLine(pen, new Point(-width / 2, 0.5), new Point(width / 2, 0.5));
        }

        private void Draw(DrawingContext drawingContext, string filename, EffectStateStructure state)
        {
            if (state == null || state.Alpha <= 0 || state.ScaleX == 0 || state.ScaleY == 0) return;
            var image = dict[filename];
            var imageWidth = image.PixelWidth;
            var imageHeight = image.PixelHeight;
            if (state.ScaleX * imageWidth >= 1 && state.ScaleY * imageHeight >= 1)
            {
                // 以前の GDI と同じく、画像の中心を原点にして、拡大、回転（度）、移動の順に当てる
                drawingContext.PushTransform(new TranslateTransform(state.X, state.Y));
                drawingContext.PushTransform(new RotateTransform(state.Rotation));
                drawingContext.PushTransform(new ScaleTransform(state.ScaleX, state.ScaleY));
                drawingContext.PushOpacity(state.Alpha);
                drawingContext.DrawImage(image, new Rect(-imageWidth / 2, -imageHeight / 2, imageWidth, imageHeight));
                drawingContext.Pop();
                drawingContext.Pop();
                drawingContext.Pop();
                drawingContext.Pop();
            }
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            if (effectManager.State == EffectManager.PlayState.Stop)
            {
                timer.Stop();
            }
            effectManager.Update();
            canvasPanel.InvalidateVisual();
        }

        public void OpenFile(string path)
        {
            dict = new Dictionary<string, BitmapSource>(StringComparer.OrdinalIgnoreCase);
            var dir = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            effectManager = EffectLoader.Load(path, false, (fn) =>
            {
                var filename = Path.Combine(dir, fn);
                if (!dict.ContainsKey(filename))
                {
                    dict.Add(filename, ImagePanel.LoadImage(filename));
                }
            });
            effectManager.Finish += EffectManager_Finish;
            canvasPanel.InvalidateVisual();
        }

        void EffectManager_Finish(object sender, EventArgs e)
        {
            this.toolStripSplitButton1.IsChecked = false;
            ChangeImage();
        }

        private void Stop()
        {
            timer.Stop();
            if (effectManager == null)
            {
                return;
            }
            effectManager.Update(0, null);
            effectManager.Stop();
            canvasPanel.InvalidateVisual();
        }

        private void Pause()
        {
            timer.Stop();
            effectManager?.Pause();
        }

        private void Play()
        {
            if (effectManager == null)
            {
                return;
            }
            EffectManager.PlayType playType = Effect2D.EffectManager.PlayType.Once;
            if (playToolStripMenuItem.IsChecked)
            {
                playType = EffectManager.PlayType.Once;
            }
            else if (reversePlayToolStripMenuItem.IsChecked)
            {
                playType = EffectManager.PlayType.ReverseOnce;
            }
            else if (loopPlayToolStripMenuItem.IsChecked)
            {
                playType = EffectManager.PlayType.Loop;
            }
            else if (reverseLoopPlayToolStripMenuItem.IsChecked)
            {
                playType = EffectManager.PlayType.ReverseLoop;
            }
            toolStripSplitButton1.IsChecked = true;
            toolStripSplitButton1Image.Source = new BitmapImage(new Uri("pack://application:,,,/PPDEditor;component/img/pause.png"));

            timer.Stop();
            effectManager.CheckFrameLength();
            effectManager.Play(playType);
            timer.Start();
        }

        private void PlayModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            foreach (MenuItem item in playModeMenu.Items)
            {
                item.IsChecked = item == menuItem;
            }
            Play();
        }

        private void ToolStripButton1_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            toolStripSplitButton1.IsChecked = false;
            ChangeImage();
        }

        private void ToolStripComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (toolStripComboBox1.SelectedIndex < 0)
            {
                return;
            }
            int length = sizeDict[toolStripComboBox1.SelectedIndex];
            canvasPanel.Width = canvasPanel.Height = length;
            PPDStaticSetting.CanvasSizeIndex = toolStripComboBox1.SelectedIndex;
        }

        private void ToolStripComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (toolStripComboBox2.SelectedIndex < 0)
            {
                return;
            }
            if (toolStripComboBox2.SelectedIndex == 3 && !ignore)
            {
                var color = PPDStaticSetting.CustomCanvasBackColor;
                var dialog = new FlowScriptDrawControl.Dialog.ColorDialog
                {
                    CurrentColor = Color.FromRgb(color.R, color.G, color.B)
                };
                if (WpfDialog.ShowDialog(dialog))
                {
                    PPDStaticSetting.CustomCanvasBackColor = System.Drawing.Color.FromArgb(dialog.CurrentColor.R, dialog.CurrentColor.G, dialog.CurrentColor.B);
                    // 見本の色も変える
                    if (toolStripComboBox2.Items[3] is ComboBoxItem { Content: StackPanel panel } && panel.Children[0] is Border swatch)
                    {
                        swatch.Background = WpfSkin.ToBrush(PPDStaticSetting.CustomCanvasBackColor);
                    }
                }
            }
            canvasPanel.InvalidateVisual();
            PPDStaticSetting.CanvasColorIndex = toolStripComboBox2.SelectedIndex;
        }
    }

    /// <summary>
    /// エフェクトを書くキャンバスです。書く内容は <see cref="EffectPanel"/> に任せる。
    /// </summary>
    public class EffectCanvas : FrameworkElement
    {
        internal EffectPanel Panel
        {
            get;
            set;
        }

        public EffectCanvas()
        {
            ClipToBounds = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Panel?.DrawCanvas(drawingContext, ActualWidth, ActualHeight);
        }
    }
}
