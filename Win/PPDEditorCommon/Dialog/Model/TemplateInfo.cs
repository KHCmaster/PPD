using PPDFramework;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace PPDEditorCommon.Dialog.Model
{
    public class TemplateInfo
    {
        public string Type
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            private set;
        }

        public string IconPath
        {
            get;
            private set;
        }

        public BitmapImage Image
        {
            get;
            private set;
        }

        public SoundManagerInfo SoundManagerInfo
        {
            get;
            private set;
        }

        public EventManagerInfo EventManagerInfo
        {
            get;
            private set;
        }

        public TimeLineInfo TimeLineInfo
        {
            get;
            private set;
        }

        public ScriptManagerInfo ScriptManagerInfo
        {
            get;
            private set;
        }

        public TemplateInfo()
        {
            SoundManagerInfo = new SoundManagerInfo();
            EventManagerInfo = new EventManagerInfo();
            TimeLineInfo = new TimeLineInfo();
            ScriptManagerInfo = new ScriptManagerInfo();
        }

        public void Load(string filePath)
        {
            var document = XDocument.Load(filePath);
            Type = ReplaceLanguage(ReadAttribute(document.Root, "Type"));
            Name = ReplaceLanguage(ReadAttribute(document.Root, "Name"));
            Description = ReplaceLanguage(ReadAttribute(document.Root, "Description")).Replace("\\n", "\n");
            IconPath = ReplaceLanguage(ReadAttribute(document.Root, "IconPath"));

            if (File.Exists(IconPath))
            {
                Image = new BitmapImage();
                Image.BeginInit();
                Image.UriSource = new Uri(IconPath, UriKind.RelativeOrAbsolute);
                Image.CacheOption = BitmapCacheOption.OnLoad;
                Image.EndInit();
            }

            foreach (var element in document.Descendants("SoundManager"))
            {
                foreach (var soundsElement in element.Elements("Sounds"))
                {
                    foreach (var soundElement in soundsElement.Elements("Sound"))
                    {
                        SoundManagerInfo.Add(soundElement.Value);
                    }
                    break;
                }
                foreach (var changesElement in element.Elements("Changes"))
                {
                    foreach (var changeElement in changesElement.Elements("Change"))
                    {
                        var time = GetElementValue(changeElement, "Time");
                        var data = GetElementValue(changeElement, "Data");
                        if (time == null || data == null)
                        {
                            continue;
                        }
                        SoundManagerInfo.AddChange(ParseFloat(time), ParseUshortList(data, 10));
                    }
                    break;
                }
                break;
            }

            foreach (var element in document.Descendants("EventManager"))
            {
                foreach (var eventsElement in element.Elements("Events"))
                {
                    foreach (var eventElement in eventsElement.Elements("Event"))
                    {
                        var time = GetElementValue(eventElement, "Time");
                        var noteType = GetElementValue(eventElement, "NoteType");
                        var bpm = GetElementValue(eventElement, "BPM");
                        var initializeOrders = GetElementValue(eventElement, "InitializeOrders");
                        if (time == null)
                        {
                            continue;
                        }
                        var newEvent = new Event();
                        if (noteType != null)
                        {
                            newEvent.NoteType = ParseEnum<NoteType>(noteType);
                        }
                        if (bpm != null)
                        {
                            newEvent.BPM = ParseFloat(bpm);
                        }
                        if (initializeOrders != null)
                        {
                            var io = ParseIntList(initializeOrders, 10).Distinct().ToArray();
                            for (int i = 0; i < Math.Min(io.Length, newEvent.InitializeOrders.Length); i++)
                            {
                                newEvent.InitializeOrders[i] = io[i];
                            }
                        }
                        EventManagerInfo.Add(ParseFloat(time), newEvent);
                    }
                    break;
                }
                break;
            }
            foreach (var element in document.Descendants("TimeLine"))
            {
                var rowOrders = GetElementValue(element, "RowOrders");
                var rowVisibilities = GetElementValue(element, "RowVisibilities");
                var rowLimited = GetElementValue(element, "RowLimited");
                if (rowOrders != null)
                {
                    var ro = ParseIntList(rowOrders, 10).Distinct().ToArray();
                    for (int i = 0; i < Math.Min(ro.Length, TimeLineInfo.RowOrders.Length); i++)
                    {
                        TimeLineInfo.RowOrders[i] = ro[i];
                    }
                }
                if (rowVisibilities != null)
                {
                    var rv = ParseIntList(rowVisibilities, 10);
                    for (int i = 0; i < Math.Min(rv.Length, TimeLineInfo.RowVisibilities.Length); i++)
                    {
                        TimeLineInfo.RowVisibilities[i] = rv[i] == 1;
                    }
                }
                if (rowLimited != null)
                {
                    TimeLineInfo.RowLimited = int.Parse(rowLimited, NumberStyles.Integer, CultureInfo.InvariantCulture) == 1;
                }
                break;
            }

            foreach (var element in document.Descendants("ScriptManager"))
            {
                foreach (var scriptsElement in element.Elements("Scripts"))
                {
                    foreach (var scriptElement in scriptsElement.Elements("Script"))
                    {
                        var src = GetElementValue(scriptElement, "Src");
                        var dest = GetElementValue(scriptElement, "Dest");
                        if (src == null || dest == null)
                        {
                            continue;
                        }
                        ScriptManagerInfo.Add(new ScriptInfo(src, dest));
                    }
                    break;
                }
                break;
            }
        }

        public T ParseEnum<T>(string str)
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        public ushort[] ParseUshortList(string str, int count)
        {
            return ParseList<ushort>(str, count, s =>
            {
                return ushort.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
            });
        }

        public int[] ParseIntList(string str, int count)
        {
            return ParseList<int>(str, count, s =>
            {
                return int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
            });
        }

        public T[] ParseList<T>(string str, int count, Func<string, T> parseCallback)
        {
            T[] ret = new T[count];
            var iter = 0;
            foreach (var split in str.Split(','))
            {
                ret[iter++] = parseCallback(split);
                if (iter >= count)
                {
                    break;
                }
            }
            return ret;
        }

        public float ParseFloat(string str)
        {
            if (str == "$(Epsilon)")
            {
                return float.Epsilon;
            }
            return float.Parse(str, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public string GetElementValue(XElement element, string name)
        {
            var foundElement = element.Elements(name).FirstOrDefault();
            if (foundElement == null)
            {
                return null;
            }
            return foundElement.Value;
        }

        public string ReadAttribute(XElement elem, string name)
        {
            var attr = elem.Attribute(name);
            if (name == null)
            {
                return null;
            }
            return attr.Value;
        }

        private string ReplaceLanguage(string str)
        {
            var regex = new Regex("{lang:(\\w+)}");
            return regex.Replace(str, m =>
            {
                return TranslateExtension.Language[m.Groups[1].Value];
            });
        }
    }
}
