using PPDFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PPDEditor.Forms
{
    public partial class EditorForm
    {
        #region プロパティ

        public string CurrentKasi
        {
            get
            {
                return ke.Askcurrentkasi();
            }
        }

        public SoundManager SoundManager
        {
            get
            {
                return sm;
            }
        }

        public ResourceManager ResourceManager
        {
            get
            {
                return rm;
            }
        }

        public MyGame MyGame
        {
            get;
            set;
        }

        public double CurrentTime
        {
            get
            {
                double d = 0;
                if (MyGame.Movie != null && MyGame.Movie.Initialized)
                {
                    d = MyGame.Movie.MoviePosition;
                }
                return d;
            }
        }

        public double MovieLength
        {
            get
            {
                if (MyGame.Movie != null)
                {
                    return MyGame.Movie.Length;
                }
                return 0;
            }
        }

        public int MovieWidth
        {
            get
            {
                if (MyGame.Movie != null)
                {
                    return MyGame.Movie.MovieWidth;
                }
                return 0;
            }
        }

        public int MovieHeight
        {
            get
            {
                if (MyGame.Movie != null)
                {
                    return MyGame.Movie.MovieHeight;
                }
                return 0;
            }
        }

        public bool MovieInitialized
        {
            get
            {
                if (MyGame.Movie != null)
                {
                    return MyGame.Movie.Initialized;
                }
                return false;
            }
        }

        public double MoviePosition
        {
            get
            {
                if (MyGame.Movie != null)
                {
                    return MyGame.Movie.MoviePosition;
                }
                return 0;
            }
        }

        public string MovieFileName
        {
            get
            {
                if (MyGame.Movie != null)
                {
                    return MyGame.Movie.FileName;
                }
                return "";
            }
        }

        public MovieTrimmingData MovieTrimmingData
        {
            get
            {
                if (MyGame.Movie != null)
                {
                    return MyGame.Movie.TrimmingData;

                }
                return null;
            }
            set
            {
                MyGame.Movie?.TrimmingData = value;
            }
        }

        public MarkSelectMode MarkSelectMode
        {
            get
            {
                return tlf.Seekmain.MarkSelectMode;
            }
        }

        public MarkSelectMode OnMouseMarkSelectMode
        {
            get
            {
                return tlf.Seekmain.OnMouseMarkSelectMode;
            }
        }

        public bool DrawPoint
        {
            get
            {
                return paalsDock != null && paalsDock.IsActive;
            }
        }

        public bool DrawAngle
        {
            get
            {
                return paals.DrawAngle;
            }
        }

        private static bool IsNotSaved
        {
            get
            {
                return sm.IsContentChanged || tlf.IsContentChanged || ifw.IsContentChanged || ke.IsContentChanged || rm.IsContentChanged || scm.IsContentChanged || em.IsContentChanged;
            }
        }

        private bool CanExecutePpd
        {
            get
            {
                if (!Directory.Exists(projectfiledir + "\\" + Path.GetFileNameWithoutExtension(projectfilename)))
                {
                    MessageBox.Show(this, noproject);
                    return false;
                }
                if (!File.Exists(Program.PPDExePath))
                {
                    MessageBox.Show(this, noppdexe);
                    return false;
                }
                return true;
            }
        }

        public float BPM
        {
            get
            {
                if (float.TryParse(this.toolStripTextBox1.Text, out float ret))
                {
                    return ret;
                }
                return 100;
            }
            set
            {
                if (value > 0 && value < 500)
                {
                    this.toolStripTextBox1.Text = value.ToString();
                }
            }
        }

        public float BPMOffset
        {
            get
            {
                if (float.TryParse(this.toolStripTextBox2.Text, out float ret))
                {
                    return ret;
                }
                return 0;
            }
            set
            {
                this.toolStripTextBox2.Text = value.ToString();
            }
        }

        public int DisplayWidth
        {
            get
            {
                if (int.TryParse(this.toolStripTextBox3.Text, out int ret))
                {
                    return ret;
                }
                return 240;
            }
            set
            {
                this.toolStripTextBox3.Text = value.ToString();
            }
        }

        public bool Bpmfixed
        {
            get
            {
                return this.toolStripButton1.IsChecked == true;
            }
            set
            {
                toolStripButton1.IsChecked = value;
            }
        }

        public bool DrawToggle
        {
            get
            {
                return this.toolStripButton2.IsChecked == true;
            }
            set
            {
                this.toolStripButton2.IsChecked = value;
            }
        }

        public bool MarkFocus
        {
            get
            {
                return this.toolStripButton4.IsChecked == true;
            }
            set
            {
                this.toolStripButton4.IsChecked = value;
            }
        }

        public bool ShowHoldExtent
        {
            get
            {
                return this.toolStripButton3.IsChecked == true;
            }
            set
            {
                this.toolStripButton3.IsChecked = value;
            }
        }

        public DisplayLineMode DisplayMode
        {
            get
            {
                return (DisplayLineMode)this.toolStripComboBox1.SelectedIndex;
            }
            set
            {
                if (value >= 0 && (int)value < toolStripComboBox1.Items.Count)
                {
                    toolStripComboBox1.SelectedIndex = (int)value;
                }
            }
        }

        public DisplayBeatType BeatType
        {
            get
            {
                return (DisplayBeatType)this.toolStripComboBox3.SelectedIndex;
            }
            set
            {
                if (value >= 0 && (int)value <= toolStripComboBox3.Items.Count)
                {
                    toolStripComboBox3.SelectedIndex = (int)value;
                }
            }
        }

        public int SpeedScale
        {
            get
            {
                return toolStripComboBox2.SelectedIndex;
            }
            set
            {
                if (value >= 0 && value < toolStripComboBox2.Items.Count)
                {
                    toolStripComboBox2.SelectedIndex = value;
                }
            }
        }

        public float SpeedScaleAsFloat
        {
            get
            {
                return toolStripComboBox2.SelectedIndex switch
                {
                    0 => 0.5f,
                    1 => 0.75f,
                    2 => 1,
                    3 => 1.25f,
                    4 => 1.5f,
                    5 => 1.75f,
                    6 => 2,
                    _ => 1,
                };
            }
        }

        public float Farness
        {
            get;
            set;
        }

        public SquareGrid Grid
        {
            get;
            set;
        }

        public bool DisplayGrid
        {
            get
            {
                return this.exToolStripSplitButton1.IsChecked == true;
            }
            set
            {
                this.exToolStripSplitButton1.IsChecked = value;
            }
        }

        private int GridPixel
        {
            set
            {
                Grid.Width = value;
                Grid.Height = value;
            }
        }

        public AvailableDifficulty CurrentDifficulty
        {
            get
            {
                return currentdifficulty;
            }
            set
            {
                currentdifficulty = value;
                switch (value)
                {
                    case AvailableDifficulty.Easy:
                        easyToolStripMenuItem.IsChecked = true;
                        TurnOffExceptDifficulty(easyToolStripMenuItem);
                        break;
                    case AvailableDifficulty.Normal:
                        normalToolStripMenuItem.IsChecked = true;
                        TurnOffExceptDifficulty(normalToolStripMenuItem);
                        break;
                    case AvailableDifficulty.Hard:
                        hardToolStripMenuItem.IsChecked = true;
                        TurnOffExceptDifficulty(hardToolStripMenuItem);
                        break;
                    case AvailableDifficulty.Extreme:
                        extremeToolStripMenuItem.IsChecked = true;
                        TurnOffExceptDifficulty(extremeToolStripMenuItem);
                        break;
                    case AvailableDifficulty.Base:
                        baseToolStripMenuItem.IsChecked = true;
                        TurnOffExceptDifficulty(baseToolStripMenuItem);
                        break;
                }
            }
        }

        public string CurrentProjectDir
        {
            get
            {
                return Path.Combine(projectfiledir, CurrentProjectName);
            }
        }

        public string CurrentProjectFilePath
        {
            get
            {
                return Path.Combine(projectfiledir, projectfilename);
            }
        }

        public string CurrentProjectName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(CurrentProjectFilePath);
            }
        }

        public AvailableDifficulty AvailableDifficulty
        {
            get
            {
                return availabledifficulty;
            }
            private set
            {
                availabledifficulty = value;
                baseToolStripMenuItem.IsEnabled = (availabledifficulty & AvailableDifficulty.Base) == AvailableDifficulty.Base;
                easyToolStripMenuItem.IsEnabled = (availabledifficulty & AvailableDifficulty.Easy) == AvailableDifficulty.Easy;
                normalToolStripMenuItem.IsEnabled = (availabledifficulty & AvailableDifficulty.Normal) == AvailableDifficulty.Normal;
                hardToolStripMenuItem.IsEnabled = (availabledifficulty & AvailableDifficulty.Hard) == AvailableDifficulty.Hard;
                extremeToolStripMenuItem.IsEnabled = (availabledifficulty & AvailableDifficulty.Extreme) == AvailableDifficulty.Extreme;
            }
        }

        public int AvailableDifficultyCount
        {
            get
            {
                return Enum.GetValues<AvailableDifficulty>().Where(d => d != PPDEditor.AvailableDifficulty.None)
#pragma warning disable RECS0002 // Convert anonymous method to method group
                    .Count(d => availabledifficulty.HasFlag(d));
#pragma warning restore RECS0002 // Convert anonymous method to method group
            }
        }

        public bool IsProjectLoaded
        {
            get
            {
                return Directory.Exists(projectfiledir);
            }
        }

        public bool FixDockPanel
        {
            get
            {
                return PPDStaticSetting.FixDockPanel;
            }
            set
            {
                PPDStaticSetting.FixDockPanel = value;
                // 固定するときは、ウィンドウを動かしたり浮かせたりできなくする
                foreach (var anchorable in AllAnchorables.Where(a => a != null))
                {
                    anchorable.CanMove = anchorable.CanFloat = anchorable.CanDockAsTabbedDocument = !value;
                }
                ドックを固定ToolStripMenuItem.IsChecked = value;
            }
        }

        public bool Connect
        {
            get
            {
                return connectToolStripMenuItem.IsChecked;
            }
            set
            {
                connectToolStripMenuItem.IsChecked = value;
            }
        }


        private string GetModName(MenuItem menu)
        {
            var names = new List<string>();
            while (menu != mODToolStripMenuItem)
            {
                names.Add(menu.Tag is ModMenuInfo info ? info.Name : (string)menu.Tag);
                menu = (MenuItem)menu.Parent;
            }
            names.Reverse();
            return String.Join(Path.DirectorySeparatorChar.ToString(), names.ToArray());
        }

        public string ModList
        {
            get
            {
                var list = new List<string>();
                var menus = new Queue<MenuItem>();
                menus.Enqueue(mODToolStripMenuItem);
                while (menus.Count > 0)
                {
                    var menu = menus.Dequeue();
                    foreach (MenuItem childMenu in menu.Items)
                    {
                        menus.Enqueue(childMenu);
                    }
                    if (menu.Tag is ModMenuInfo && menu.IsChecked)
                    {
                        list.Add(GetModName(menu));
                    }
                }
                return String.Join(",", list.ToArray());
            }
            set
            {
                var menus = new Queue<MenuItem>();
                menus.Enqueue(mODToolStripMenuItem);
                while (menus.Count > 0)
                {
                    var menu = menus.Dequeue();
                    foreach (MenuItem childMenu in menu.Items)
                    {
                        menus.Enqueue(childMenu);
                    }
                    menu.IsChecked = false;
                }

                if (String.IsNullOrEmpty(value))
                {
                    return;
                }
                var mods = value.Split(',');
                menus.Enqueue(mODToolStripMenuItem);
                while (menus.Count > 0)
                {
                    var menu = menus.Dequeue();
                    foreach (MenuItem childMenu in menu.Items)
                    {
                        menus.Enqueue(childMenu);
                    }
                    if (menu.Tag is ModMenuInfo && Array.IndexOf(mods, GetModName(menu)) >= 0)
                    {
                        menu.IsChecked = true;
                    }
                }
            }
        }

        public bool DisableExpansion
        {
            get
            {
                return disableExpansionToolStripMenuItem.IsChecked;
            }
            set
            {
                disableExpansionToolStripMenuItem.IsChecked = value;
            }
        }


        public bool IsTimeLineRowLimited
        {
            get { return tlf.RowManager.IsLimited; }
            set
            {
                if (tlf.RowManager.IsLimited != value)
                {
                    tlf.RowManager.IsLimited = value;
                    tlf.InvalidateAll();
                }
                toolStripSplitButton1.IsChecked = tlf.RowManager.IsLimited;
            }
        }

        #endregion
    }
}
