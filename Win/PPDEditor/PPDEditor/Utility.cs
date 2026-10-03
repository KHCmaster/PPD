using PPDConfiguration;
using PPDEditorCommon;
using PPDFramework;
using System.Numerics;
using PPDMath;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor
{
    public static class Utility
    {
        public static PPDDevice Device { get; set; }
        public static PPDFramework.Resource.ResourceManager ResourceManager { get; set; }
        public static float[] Eval { get; set; }
        public static Vector2[] CirclePoints { get; set; }

        private static readonly PathManager pathManager = new(@"img\PPDEditor");

        public static PathManager Path
        {
            get { return pathManager; }
        }

        public static float ParseFloat(string str)
        {
            if (!float.TryParse(str, out float result))
            {
                if (!float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                {
                    throw new Exception("invalid number");
                }
            }
            return result;
        }

        public static bool IsSameArray(ButtonType[] array1, ButtonType[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CheckValidFileName(string fileName)
        {
            foreach (Char c in System.IO.Path.GetInvalidFileNameChars())
            {
                if (fileName.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }

        public static T[] GetSortedData<T>(SortedList<float, T>[] data)
        {
            int num = 0;
            for (int i = 0; i < 10; i++)
            {
                num += data[i].Count;
            }
            T[] ret = new T[num];
            int retiter = 0;
            int[] iters = new int[10];
            while (true)
            {
                int minimum = -1;
                float minimumtime = float.MaxValue;
                for (int i = 0; i < 10; i++)
                {
                    if (iters[i] < data[i].Count)
                    {
                        if (minimumtime >= data[i].Keys[iters[i]])
                        {
                            minimum = i;
                            minimumtime = data[i].Keys[iters[i]];
                        }
                    }
                }
                if (minimum == -1)
                {
                    break;
                }
                else
                {
                    T t = data[minimum].Values[iters[minimum]];
                    ret[retiter] = t;
                    retiter++;
                    iters[minimum]++;
                }
            }
            return ret;
        }

        private static string appDir;
        public static string AppDir
        {
            get
            {
                appDir ??= System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                return appDir;
            }
        }

        private static LanguageReader langReader = new("PPDEditor", PPDStaticSetting.langFileISO);

        public static LanguageReader Language
        {
            get
            {
                return langReader;
            }
        }

        public static void ChangeLanguage(string langIso)
        {
            langReader = new LanguageReader("PPDEditor", langIso);
        }

        public static void ShowOrHideWindow(DockPanel dockPanel, DockContent dockcontent, Keys modifierKeys)
        {
            if (!dockcontent.Visible || (DockState.DockTopAutoHide <= dockcontent.DockState && dockcontent.DockState <= DockState.DockRightAutoHide))
            {
                if (dockcontent.Pane != null)
                {
                    dockcontent.Show(dockPanel);
                    dockcontent.Pane.ActiveContent = dockcontent;
                }
                else
                {
                    dockcontent.Show(dockPanel, WeifenLuo.WinFormsUI.Docking.DockState.Float);
                }
            }
            else
            {
                if (dockPanel.ActivePane == dockcontent.Pane || dockcontent.DockState == DockState.Float)
                {
                    dockcontent.Hide();
                }
                else
                {
                    dockcontent.Focus();
                }
            }
            if (dockcontent.DockState == DockState.Unknown)
            {
                dockcontent.Show(dockPanel, DockState.Document);
                dockcontent.Pane.ActiveContent = dockcontent;
            }
            if ((modifierKeys & Keys.Shift) == Keys.Shift)
            {
                dockcontent.DockPanel = null;
                dockcontent.Show(dockPanel, DockState.Float);
                dockcontent.Pane.ActiveContent = dockcontent;
            }
        }

        public static bool CheckVisible(DockContent dc)
        {
            if (dc.Visible || (dc.DockState > DockState.Unknown && dc.DockState < DockState.Hidden))
            {
                return true;
            }
            return false;
        }

        public static void RecursiveCreateFolder(string filePath)
        {
            var dirStack = new Stack<string>();
            string dir = String.IsNullOrEmpty(System.IO.Path.GetExtension(filePath)) ? filePath : System.IO.Path.GetDirectoryName(filePath);
            while (true)
            {
                if (Directory.Exists(dir) || String.IsNullOrEmpty(dir))
                {
                    break;
                }
                else
                {
                    dirStack.Push(dir);
                    dir = System.IO.Path.GetDirectoryName(dir);
                }
            }

            while (dirStack.Count > 0)
            {
                SafeCreateDirectory(dirStack.Pop());
            }
        }

        public static void SafeCreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public static void SafeCopyFile(string src, string dest)
        {
            var destDir = System.IO.Path.GetDirectoryName(dest);
            SafeCreateDirectory(destDir);
            File.Copy(src, dest, true);
        }

        public static void CopyDirectory(string sourceDirName, string destDirName, Regex[] exceptFiles)
        {
            CopyDirectory(sourceDirName, destDirName, exceptFiles, null);
        }

        public static void CopyDirectory(string sourceDirName, string destDirName, Regex[] exceptFiles, Func<string, bool> overwriteConfirm)
        {
            exceptFiles ??= [];
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            if (destDirName[^1] != System.IO.Path.DirectorySeparatorChar)
            {
                destDirName += System.IO.Path.DirectorySeparatorChar;
            }

            var files = Directory.GetFiles(sourceDirName);
            foreach (string file in files)
            {
                var filename = System.IO.Path.GetFileName(file);
                bool ok = true;
                foreach (Regex regex in exceptFiles)
                {
                    if (regex.IsMatch(filename))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    bool copy = true;
                    string destFilePath = destDirName + filename;
                    if (File.Exists(destFilePath))
                    {
                        copy &= (overwriteConfirm == null || overwriteConfirm(destFilePath));
                    }
                    if (copy)
                    {
                        File.Copy(file, destFilePath, true);
                    }
                }
            }
            var dirs = Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
            {
                CopyDirectory(dir, destDirName + System.IO.Path.GetFileName(dir), exceptFiles);
            }
        }

        public static int Clamp(int val, int min, int max)
        {
            if (val < min)
            {
                return min;
            }
            if (val > max)
            {
                return max;
            }
            return val;
        }

        public static float Clamp(float val, float min, float max)
        {
            if (val < min)
            {
                return min;
            }
            if (val > max)
            {
                return max;
            }
            return val;
        }

        public static Dictionary<string, string> GetCommonParameters(Mark[] marks)
        {
            return GetCommonParameters(marks, out _);
        }

        public static Dictionary<string, string> GetCommonParameters(Mark[] marks, out string[] existKeys)
        {
            var commanParameters = new Dictionary<string, string>();
            var keys = new HashSet<string>();
            foreach (var pair in marks[0].Parameters)
            {
                commanParameters[pair.Key] = pair.Value;
                keys.Add(pair.Key);
            }
            foreach (var m in marks.Skip(1))
            {
                var temp = new Dictionary<string, string>();
                var parameters = m.Parameters;
                foreach (var pair in parameters)
                {
                    temp.Add(pair.Key, pair.Value);
                    keys.Add(pair.Key);
                }
                var removeKeys = new List<string>();
                foreach (var key in commanParameters.Keys)
                {
                    if (!temp.ContainsKey(key))
                    {
                        removeKeys.Add(key);
                    }
                }
                foreach (var removeKey in removeKeys)
                {
                    commanParameters.Remove(removeKey);
                }
            }
            existKeys = [.. keys];
            return commanParameters;
        }

        public static Mark[] GetSelectedMarks()
        {
            var sheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (sheet == null)
            {
                return [];
            }
            var mark = sheet.SelectedMark;
            Mark[] marks;
            if (mark != null)
            {
                marks = [mark];
            }
            else
            {
                marks = sheet.GetAreaData();
            }
            return marks;
        }

        /// <summary>
        /// プリセットのパラメーターがすべて当てはまるときは true、一部だけなら null、一つもなければ false を返す。
        /// </summary>
        public static bool? GetPresetCheckState(ParameterPreset preset, Dictionary<string, string> parameters)
        {
            int count = 0;
            foreach (var parameter in parameters)
            {
                var param = preset.Parameters.FirstOrDefault(p => p.Key == parameter.Key && p.Value == parameter.Value);
                if (param.Key != null)
                {
                    count++;
                }
            }
            return count == preset.Parameters.Length ? true : (count > 0 ? null : false);
        }
    }
}
