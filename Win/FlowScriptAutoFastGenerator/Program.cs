using FlowScriptEngine;
using FlowScriptEngine.AutoFastGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FlowScriptAutoFastGenerator
{
    partial class Program
    {
        static readonly HashSet<ClassInfo> errorFiles = [];


#if DEBUG
        static void Main()
        {
            var winPath = Path.GetFullPath("..\\..\\..\\..\\");
            var list = new string[]{
                Path.Combine(winPath,$@"FlowScriptEngineBasic\bin\x64\Debug\FlowScriptEngineBasic.dll"),
                Path.Combine(winPath,@"FlowScriptEngineBasic"),
                Path.Combine(winPath,$@"FlowScriptEngineConsole\bin\x64\Debug\FlowScriptEngineConsole.dll"),
                Path.Combine(winPath,@"FlowScriptEngineConsole"),
                Path.Combine(winPath,$@"FlowScriptEnginePPD\bin\x64\Debug\FlowScriptEnginePPD.dll"),
                Path.Combine(winPath,@"FlowScriptEnginePPD"),
                Path.Combine(winPath,$@"FlowScriptEngineSlimDX\bin\x64\Debug\FlowScriptEngineSlimDX.dll"),
                Path.Combine(winPath,@"FlowScriptEngineSlimDX"),
                Path.Combine(winPath,$@"FlowScriptEnginePPDEditor\bin\x64\Debug\FlowScriptEnginePPDEditor.dll"),
                Path.Combine(winPath,@"FlowScriptEnginePPDEditor"),
                Path.Combine(winPath,$@"FlowScriptEngineBasicExtension\bin\x64\Debug\FlowScriptEngineBasicExtension.dll"),
                Path.Combine(winPath,@"FlowScriptEngineBasicExtension"),
                Path.Combine(winPath,$@"FlowScriptEngineData\bin\x64\Debug\FlowScriptEngineData.dll"),
                Path.Combine(winPath,@"FlowScriptEngineData")
            };

            for (int i = 0; i < list.Length / 2; i++)
            {
                Process(list[i * 2], list[i * 2 + 1]);
            }
#else
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: FlowScriptAutoFastGenerator.exe [dllPath] [projectPath]");
                return;
            }
            string dllPath = args[0], projectPath = args[1];
#endif
            if (errorFiles.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                foreach (var errorFile in errorFiles)
                {
                    Console.WriteLine("{0} has error:", errorFile.FilePath);
                    foreach (var error in errorFile.Errors)
                    {
                        Console.WriteLine(error);
                    }
                }
                Console.ReadLine();
            }
        }

        static void Process(string dllPath, string projectPath)

        {
            var fileList = GetFileList(projectPath);
            var dict = GetClassInfo(fileList);

            foreach (AssemblyAndType asm in FlowSourceEnumerator.EnumerateFromFile(dllPath, [typeof(FlowSourceObjectBase)]))
            {
                var temp = (FlowSourceObjectBase)asm.Assembly.CreateInstance(asm.Type.FullName);
                if (temp != null)
                {
                    if (dict.TryGetValue(asm.Type.FullName, out ClassInfo classInfo))
                    {
                        CheckSetProperties(dict, classInfo, temp);

                        var autoFastContent = Generator.GetFastCode(temp);
                        var path = Path.Combine(Path.GetDirectoryName(classInfo.FilePath), String.Format("{0}.AutoFast.cs", Path.GetFileNameWithoutExtension(classInfo.FilePath)));
                        UpdateContent(autoFastContent, path);

                        if (!classInfo.HasPartial)
                        {
                            string content;
                            using (StreamReader sr = new(classInfo.FilePath, Encoding.UTF8))
                            {
                                content = sr.ReadToEnd();
                            }

                            var classRegex = new Regex(String.Format("public +class +{0}", classInfo.ClassName));
                            var m = classRegex.Match(content);
                            if (m.Success)
                            {
                                content = content[..m.Groups[0].Index] + "public partial class " + classInfo.ClassName + content[(m.Groups[0].Index + m.Groups[0].Length)..];
                                UpdateContent(content, classInfo.FilePath);
                            }
                        }
                    }
                }
            }

            HideAutoFastCs(projectPath);
        }

        static void CheckSetProperties(Dictionary<string, ClassInfo> classes, ClassInfo classInfo, FlowSourceObjectBase flowSource)
        {
            foreach (var name in classInfo.SetValues)
            {
                if (!flowSource.InPropertyNames.Contains(name))
                {
                    classInfo.AddError(String.Format("{0} is not defined", name));
                    errorFiles.Add(classInfo);
                }
            }
            foreach (var setterPropertyName in flowSource.InPropertyNames)
            {
                bool usedFound = false;
                var type = flowSource.GetType();
                ClassInfo currentClassInfo = classInfo;
                while (type != null)
                {
                    if (currentClassInfo.ContainsSetValue(setterPropertyName))
                    {
                        usedFound = true;
                        break;
                    }
                    type = type.BaseType;
                    var className = type.FullName;
                    if (type.IsGenericType)
                    {
                        className = GetGenericClassName(className);
                    }
                    if (!classes.TryGetValue(className, out currentClassInfo))
                    {
                        break;
                    }
                }
                if (!usedFound)
                {
                    classInfo.AddError(String.Format("{0} is defined, but not used in SetValue()", setterPropertyName));
                    errorFiles.Add(classInfo);
                }
            }
        }

        private static string GetGenericClassName(string name)
        {
            var backQuoteIndex = name.IndexOf('`');
            if (backQuoteIndex < 0)
            {
                return name;
            }
            return name[..backQuoteIndex];
        }

        static void UpdateContent(string content, string filePath)
        {
            UpdateContent(content, filePath, true);
        }

        static void UpdateContent(string content, string filePath, bool withSignature)
        {
            UpdateContent(content, filePath, withSignature ? Encoding.UTF8 : new UTF8Encoding(false));
        }

        static void UpdateContent(string content, string filePath, Encoding encoding)
        {
            if (File.Exists(filePath))
            {
                var currentContent = File.ReadAllText(filePath, encoding);
                if (content == currentContent)
                {
                    return;
                }
            }
            using StreamWriter sw = new(filePath, false, encoding);
            sw.Write(content);
        }

        static string GetRelativePath(string dir, string filePath)
        {
            return filePath[(dir.Length + 1)..];
        }

        static void HideAutoFastCs(string projectDir)
        {
            var regex = AutoFastFileRegex();
            var autoFasts = new List<string>();
            var dirs = new Queue<string>();
            dirs.Enqueue(projectDir);
            while (dirs.Count > 0)
            {
                var dir = dirs.Dequeue();
                foreach (string childDir in Directory.GetDirectories(dir))
                {
                    dirs.Enqueue(childDir);
                }

                foreach (string fn in Directory.GetFiles(dir))
                {
                    var filename = Path.GetFileName(fn);
                    if (regex.IsMatch(filename))
                    {
                        autoFasts.Add(GetRelativePath(projectDir, fn));
                    }
                }
            }

            foreach (string filePath in Directory.GetFiles(projectDir, "*.csproj"))
            {
                var doc = new XmlDocument();
                doc.Load(filePath);
                var list = doc.DocumentElement.SelectNodes("//*");
                var compileNodes = new List<XmlElement>();
                foreach (XmlNode node in list)
                {
                    if (node is XmlElement elem && elem.Name == "Compile")
                    {
                        compileNodes.Add(elem);
                    }
                }

                foreach (XmlElement elem in compileNodes)
                {
                    string path = elem.Attributes["Include"].Value;
                    Console.WriteLine(path);
                    if (regex.IsMatch(Path.GetFileName(path)))
                    {
                        autoFasts.Remove(path);
                        while (elem.ChildNodes.Count > 0)
                        {
                            elem.RemoveChild(elem.ChildNodes[0]);
                        }
                        var newElem = doc.CreateElement("DependentUpon", "http://schemas.microsoft.com/developer/msbuild/2003");
                        newElem.InnerText = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path)) + ".cs";
                        elem.AppendChild(newElem);
                    }
                    else
                    {
                    }
                }

                var parent = compileNodes[0].ParentNode as XmlElement;
                foreach (string autoFast in autoFasts)
                {
                    var compileElem = doc.CreateElement("Compile", "http://schemas.microsoft.com/developer/msbuild/2003");
                    var attr = doc.CreateAttribute("Include");
                    attr.Value = autoFast;
                    compileElem.Attributes.Append(attr);
                    parent.AppendChild(compileElem);
                    var newElem = doc.CreateElement("DependentUpon", "http://schemas.microsoft.com/developer/msbuild/2003");
                    newElem.InnerText = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(autoFast)) + ".cs";
                    compileElem.AppendChild(newElem);
                }

                using MemoryStream stream = new();
                doc.Save(stream);
                var content = Encoding.UTF8.GetString(stream.ToArray());
                content = content[1..];
                UpdateContent(content, filePath, false);
                break;
            }
        }

        static string[] GetFileList(string projectDir)
        {
            var regex = CsFileRegex();
            var ret = new List<string>();
            var queue = new Queue<string>();
            queue.Enqueue(projectDir);
            while (queue.Count > 0)
            {
                var dir = queue.Dequeue();
                foreach (string childDir in Directory.GetDirectories(dir))
                {
                    queue.Enqueue(childDir);
                }

                foreach (string childFile in Directory.GetFiles(dir))
                {
                    if (regex.IsMatch(Path.GetFileName(childFile)))
                    {
                        ret.Add(childFile);
                    }
                }
            }

            return [.. ret];
        }

        static Dictionary<string, ClassInfo> GetClassInfo(string[] fileList)
        {
            var ret = new Dictionary<string, ClassInfo>();
            var classRegex = ClassDeclarationRegex();
            var namespaceRegex = NamespaceDeclarationRegex();
            var setValueRegex1 = SetValueStringRegex();
            var setValueRegex2 = SetValueNameofRegex();

            foreach (string filePath in fileList)
            {
                using StreamReader sr = new(filePath, Encoding.UTF8);
                var content = sr.ReadToEnd();
                Match classMatch = classRegex.Match(content),
                    namespaceMatch = namespaceRegex.Match(content);

                if (classMatch.Success && namespaceMatch.Success)
                {
                    var names = new HashSet<string>();
                    foreach (Match match in setValueRegex1.Matches(content))
                    {
                        names.Add(match.Groups[1].Value);
                    }
                    foreach (Match match in setValueRegex2.Matches(content))
                    {
                        names.Add(match.Groups[1].Value);
                    }
                    var classInfo = new ClassInfo
                    {
                        FilePath = filePath,
                        ClassName = classMatch.Groups["className"].Value,
                        HasPartial = !String.IsNullOrEmpty(classMatch.Groups["partial"].Value),
                        IsAbstract = !String.IsNullOrEmpty(classMatch.Groups["abstract"].Value)
                    };
                    foreach (var name in names)
                    {
                        classInfo.AddSetValue(name);
                    }
                    ret.Add(String.Format("{0}.{1}", namespaceMatch.Groups["namespace"], classMatch.Groups["className"].Value), classInfo);
                }
            }

            return ret;
        }

        class ClassInfo
        {
            readonly List<string> errors;
            readonly HashSet<string> setValues;

            public string[] Errors
            {
                get
                {
                    return [.. errors];
                }
            }

            public string[] SetValues
            {
                get { return [.. setValues]; }
            }

            public ClassInfo()
            {
                errors = [];
                setValues = [];
            }

            public string FilePath
            {
                get;
                set;
            }

            public string ClassName
            {
                get;
                set;
            }

            public bool HasPartial
            {
                get;
                set;
            }

            public bool IsAbstract
            {
                get;
                set;
            }

            public void AddSetValue(string name)
            {
                setValues.Add(name);
            }

            public void AddError(string error)
            {
                errors.Add(error);
            }

            public bool ContainsSetValue(string name)
            {
                return setValues.Contains(name);
            }
        }

        class Warning(string filePath, string warningText)
        {
            public string FilePath
            {
                get;
                private set;
            } = filePath;

            public string WarningText
            {
                get;
                private set;
            } = warningText;
        }

        [GeneratedRegex("^\\w+\\.AutoFast\\.cs$")]
        private static partial Regex AutoFastFileRegex();
        [GeneratedRegex("^\\w+\\.cs$")]
        private static partial Regex CsFileRegex();
        [GeneratedRegex("public +(?<partial>partial)? *(?<abstract>abstract)? *class +(?<className>[\\w]+)")]
        private static partial Regex ClassDeclarationRegex();
        [GeneratedRegex("namespace +(?<namespace>[\\w.]+)")]
        private static partial Regex NamespaceDeclarationRegex();
        [GeneratedRegex("SetValue\\(\"([a-zA-Z_][a-zA-Z0-9_]*)\"\\)")]
        private static partial Regex SetValueStringRegex();
        [GeneratedRegex("SetValue\\(nameof\\(([a-zA-Z_][a-zA-Z0-9_]*)\\)\\)")]
        private static partial Regex SetValueNameofRegex();
    }
}
