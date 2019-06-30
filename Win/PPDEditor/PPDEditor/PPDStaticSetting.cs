using PPDConfiguration;
using PPDEditor.Forms;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PPDEditor
{
    class PPDStaticSetting
    {
        static Regex rgb = new Regex("(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)");
        static Regex argb = new Regex("(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)");

        public static FormWindowState LastWindowState = FormWindowState.Normal;
        public static Size LastWindowSize = new Size(800, 600);
        public static Point LastWindowLocation = new Point(0, 0);
        public static string AuthorName = "";
        public static float MovieLatency;
        public static string langFileISO = "";
        public static bool HideToggleRectangle;
        public static bool HideToggleArrow;
        public static bool FixDockPanel;
        public static bool EnableToChangeMarkTypeAndTime;
        public static float MemoFontSize;
        public static bool ApplyToIniMovieTrimming;
        public static Color CustomCanvasBackColor;
        public static int CanvasSizeIndex;
        public static int CanvasColorIndex;
        public static int[] Moves = new int[8];
        public static int[] Angles = new int[8];

        static PPDStaticSetting()
        {
            var path = Path.Combine(Utility.AppDir, EditorForm.iniFileName);
            if (!File.Exists(path))
            {
                return;
            }

            var setting = new SettingReader(File.ReadAllText(path));
            if (setting.ReadString("Maximised") == "1")
            {
                LastWindowState = FormWindowState.Maximized;
            }
            var size = (setting.ReadString("Size")).Split(',');
            if (size.Length == 2)
            {
                LastWindowSize = new Size(int.Parse(size[0], CultureInfo.InvariantCulture), int.Parse(size[1], CultureInfo.InvariantCulture));
            }
            else
            {
                LastWindowSize = new Size(800, 600);
            }
            var location = (setting.ReadString("Location")).Split(',');
            if (location.Length == 2)
            {
                LastWindowLocation = new Point(int.Parse(location[0], CultureInfo.InvariantCulture), int.Parse(location[1], CultureInfo.InvariantCulture));
            }
            else
            {
                LastWindowLocation = new Point(0, 0);
            }
            AuthorName = setting.ReadString("AuthorName");
            var movielatency = setting.ReadString("MovieLatency");
            float.TryParse(movielatency, out MovieLatency);
            langFileISO = setting.ReadString("Language");
            if (langFileISO == "")
            {
                langFileISO = "jp";
            }
            HideToggleRectangle = ReadBoolean(setting, "HideToggleRectangle");
            HideToggleArrow = ReadBoolean(setting, "HideToggleArrow");
            FixDockPanel = ReadBoolean(setting, "FixDockPanel");
            EnableToChangeMarkTypeAndTime = ReadBoolean(setting, "EnableToChangeMarkTypeAndTime");
            ApplyToIniMovieTrimming = ReadBoolean(setting, "ApplyToIniMovieTrimming");
            CustomCanvasBackColor = GetColorFromString(setting.ReadString("CustomCanvasBackColor"), Color.White);
            CanvasSizeIndex = ReadInt(setting, "CanvasSizeIndex", 0);
            CanvasColorIndex = ReadInt(setting, "CanvasColorIndex", 0);
            MemoFontSize = ReadFloat(setting, "MemoFontSize", 10);
            Moves[0] = ReadInt(setting, String.Format("Move{0}", 0), 1);
            Moves[1] = ReadInt(setting, String.Format("Move{0}", 1), 10);
            Moves[2] = ReadInt(setting, String.Format("Move{0}", 2), 5);
            Angles[0] = ReadInt(setting, String.Format("Angle{0}", 0), 0);
            Angles[1] = ReadInt(setting, String.Format("Angle{0}", 1), 10);
            Angles[2] = ReadInt(setting, String.Format("Angle{0}", 2), 15);
            for (int i = 3; i < Moves.Length; i++)
            {
                Moves[i] = ReadInt(setting, String.Format("Move{0}", i), 1);
                Angles[i] = ReadInt(setting, String.Format("Angle{0}", i), 10);
            }
        }

        private static int ReadInt(SettingReader setting, string key, int error)
        {
            var val = setting.ReadString(key);
            if (!int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out int ret))
            {
                return error;
            }

            return ret;
        }

        private static float ReadFloat(SettingReader setting, string key, float error)
        {
            var val = setting.ReadString(key);
            if (!float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float ret))
            {
                return error;
            }

            return ret;

        }

        private static bool ReadBoolean(SettingReader setting, string key)
        {
            return setting.ReadString(key) == "1";
        }

        private static Color GetColorFromString(string data, Color ErrorColor)
        {
            Color ret = ErrorColor;
            var m = argb.Match(data);
            if (!m.Success)
            {
                m = rgb.Match(data);
            }
            if (m.Success)
            {
                if (m.Groups.Count == 4)
                {
                    ret = Color.FromArgb(int.Parse(m.Groups[1].Captures[0].Value), int.Parse(m.Groups[2].Captures[0].Value), int.Parse(m.Groups[3].Captures[0].Value));
                }
                else if (m.Groups.Count == 5)
                {
                    ret = Color.FromArgb(int.Parse(m.Groups[1].Captures[0].Value), int.Parse(m.Groups[2].Captures[0].Value), int.Parse(m.Groups[3].Captures[0].Value), int.Parse(m.Groups[4].Captures[0].Value));
                }
            }
            return ret;
        }
    }
}
