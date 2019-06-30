using PPDFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class EditorForm
    {
        #region プロパティ

        /// <summary>
        /// 閉じられようとしているかどうかを取得、設定します。
        /// </summary>
        public bool IsCloseRequired
        {
            get;
            set;
        }

        /// <summary>
        /// 最初に閉じされようとしているかどうかを取得、設定します。
        /// </summary>
        public bool IsFirstCloseRequierd
        {
            get;
            set;
        }

        /// <summary>
        /// 閉じることが許可されたかどうかを取得、設定します。
        /// </summary>
        public bool CloseAdmitted
        {
            get;
            set;
        }

        /// <summary>
        /// フォームを取得します。
        /// </summary>
        public Form MainForm
        {
            get { return this; }
        }

        /// <summary>
        /// フォームを取得します。
        /// </summary>
        public Form RenderForm
        {
            get { return dxf; }
        }

        public string CurrentKasi
        {
            get
            {
                return ke.askcurrentkasi();
            }
        }

        public IntPtr WindowSpace
        {
            get
            {
                return dxf.Handle;
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
                if (MyGame.Movie != null)
                {
                    MyGame.Movie.TrimmingData = value;
                }
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
                return (dockPanel1.ActiveContent == paals);// (paals.DockState > DockState.Unknown && paals.DockState < DockState.Hidden ? true : false);
            }
        }

        public bool DrawAngle
        {
            get
            {
                return paals.DrawAngle;
            }
        }

        private bool IsNotSaved
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
                    MessageBox.Show(noproject);
                    return false;
                }
                if (!File.Exists(Program.PPDExePath))
                {
                    MessageBox.Show(noppdexe);
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
                return this.toolStripButton1.Checked;
            }
            set
            {
                toolStripButton1.Checked = value;
            }
        }

        public bool DrawToggle
        {
            get
            {
                return this.toolStripButton2.Checked;
            }
            set
            {
                this.toolStripButton2.Checked = value;
            }
        }

        public bool MarkFocus
        {
            get
            {
                return this.toolStripButton4.Checked;
            }
            set
            {
                this.toolStripButton4.Checked = value;
            }
        }

        public bool ShowHoldExtent
        {
            get
            {
                return this.toolStripButton3.Checked;
            }
            set
            {
                this.toolStripButton3.Checked = value;
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
                switch (toolStripComboBox2.SelectedIndex)
                {
                    case 0:
                        return 0.5f;
                    case 1:
                        return 0.75f;
                    case 2:
                        return 1;
                    case 3:
                        return 1.25f;
                    case 4:
                        return 1.5f;
                    case 5:
                        return 1.75f;
                    case 6:
                        return 2;
                    default:
                        return 1;
                }
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
                return this.exToolStripSplitButton1.Checked;
            }
            set
            {
                this.exToolStripSplitButton1.Checked = value;
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
                        easyToolStripMenuItem.Checked = true;
                        TurnOffExceptDifficulty(easyToolStripMenuItem);
                        break;
                    case AvailableDifficulty.Normal:
                        normalToolStripMenuItem.Checked = true;
                        TurnOffExceptDifficulty(normalToolStripMenuItem);
                        break;
                    case AvailableDifficulty.Hard:
                        hardToolStripMenuItem.Checked = true;
                        TurnOffExceptDifficulty(hardToolStripMenuItem);
                        break;
                    case AvailableDifficulty.Extreme:
                        extremeToolStripMenuItem.Checked = true;
                        TurnOffExceptDifficulty(extremeToolStripMenuItem);
                        break;
                    case AvailableDifficulty.Base:
                        baseToolStripMenuItem.Checked = true;
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
                baseToolStripMenuItem.Enabled = (availabledifficulty & AvailableDifficulty.Base) == AvailableDifficulty.Base;
                easyToolStripMenuItem.Enabled = (availabledifficulty & AvailableDifficulty.Easy) == AvailableDifficulty.Easy;
                normalToolStripMenuItem.Enabled = (availabledifficulty & AvailableDifficulty.Normal) == AvailableDifficulty.Normal;
                hardToolStripMenuItem.Enabled = (availabledifficulty & AvailableDifficulty.Hard) == AvailableDifficulty.Hard;
                extremeToolStripMenuItem.Enabled = (availabledifficulty & AvailableDifficulty.Extreme) == AvailableDifficulty.Extreme;
            }
        }

        public int AvailableDifficultyCount
        {
            get
            {
                return ((AvailableDifficulty[])Enum.GetValues(typeof(AvailableDifficulty))).Where(d => d != PPDEditor.AvailableDifficulty.None)
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
                if (dockPanel1 != null)
                {
                    dockPanel1.AllowChangeDock = dockPanel1.AllowEndUserDocking =
                        dockPanel1.AllowEndUserNestedDocking = !value;
                }
                ドックを固定ToolStripMenuItem.Checked = value;
            }
        }

        public bool Connect
        {
            get
            {
                return connectToolStripMenuItem.Checked;
            }
            set
            {
                connectToolStripMenuItem.Checked = value;
            }
        }


        private string GetModName(ToolStripMenuItem menu)
        {
            var names = new List<string>();
            while (menu != mODToolStripMenuItem)
            {
                names.Add(menu.Text);
                menu = (ToolStripMenuItem)menu.OwnerItem;
            }
            names.Reverse();
            return String.Join(Path.DirectorySeparatorChar.ToString(), names.ToArray());
        }

        public string ModList
        {
            get
            {
                var list = new List<string>();
                var menus = new Queue<ToolStripMenuItem>();
                menus.Enqueue(mODToolStripMenuItem);
                while (menus.Count > 0)
                {
                    var menu = menus.Dequeue();
                    foreach (ToolStripMenuItem childMenu in menu.DropDownItems)
                    {
                        menus.Enqueue(childMenu);
                    }
                    if (menu.Tag is bool && (bool)menu.Tag && menu.Checked)
                    {
                        list.Add(GetModName(menu));
                    }
                }
                return String.Join(",", list.ToArray());
            }
            set
            {
                var menus = new Queue<ToolStripMenuItem>();
                menus.Enqueue(mODToolStripMenuItem);
                while (menus.Count > 0)
                {
                    var menu = menus.Dequeue();
                    foreach (ToolStripMenuItem childMenu in menu.DropDownItems)
                    {
                        menus.Enqueue(childMenu);
                    }
                    menu.Checked = false;
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
                    foreach (ToolStripMenuItem childMenu in menu.DropDownItems)
                    {
                        menus.Enqueue(childMenu);
                    }
                    if (Array.IndexOf(mods, GetModName(menu)) >= 0)
                    {
                        menu.Checked = true;
                    }
                }
            }
        }

        public bool DisableExpansion
        {
            get
            {
                return disableExpansionToolStripMenuItem.Checked;
            }
            set
            {
                disableExpansionToolStripMenuItem.Checked = value;
            }
        }

        public bool DisableShader
        {
            get
            {
                return disableShaderToolStripMenuItem.Checked;
            }
            set
            {
                disableShaderToolStripMenuItem.Checked = value;
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
                toolStripSplitButton1.Checked = tlf.RowManager.IsLimited;
            }
        }

        #endregion
    }
}
