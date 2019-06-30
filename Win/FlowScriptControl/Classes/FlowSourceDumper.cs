using FlowScriptControl.Controls;
using FlowScriptDrawControl.Model;
using FlowScriptEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;

namespace FlowScriptControl.Classes
{
    public class FlowSourceDumper : IToolTipText
    {
        private static Hashtable typeTextTypeDict = new Hashtable();
        private static List<Type> propertyTypes = new List<Type>();

        FlowSourceObjectBase flowSourceObject;
        AssemblyAndType asmAndType;
        bool initialized;

        private string sourceName;
        private string fullName;
        private string flowToolTipText;
        private string summary;
        private string warning;
        private Item[] inEvents;
        private Item[] outEvents;
        private Item[] inProperties;
        private Item[] outProperties;

        public string SourceName
        {
            get
            {
                Initialize();
                return sourceName;
            }
        }

        public string FullName
        {
            get
            {
                Initialize();
                return fullName;
            }
        }

        public string FlowToolTipText
        {
            get
            {
                Initialize();
                return flowToolTipText;
            }
        }

        public string Summary
        {
            get
            {
                Initialize();
                return summary;
            }
        }

        public string Warning
        {
            get
            {
                Initialize();
                return warning;
            }
        }

        public Item[] InEvents
        {
            get
            {
                Initialize();
                return inEvents;
            }
        }

        public Item[] OutEvents
        {
            get
            {
                Initialize();
                return outEvents;
            }
        }

        public Item[] InProperties
        {
            get
            {
                Initialize();
                return inProperties;
            }
        }

        public Item[] OutProperties
        {
            get
            {
                Initialize();
                return outProperties;
            }
        }

        public string ToolTipText
        {
            get;
            private set;
        }

        public AssemblyAndType AssemblyAndType
        {
            get
            {
                return asmAndType;
            }
        }

        public FlowSourceDumper(FlowSourceObjectBase flowSourceObject, AssemblyAndType asmAndType)
        {
            this.flowSourceObject = flowSourceObject;
            this.asmAndType = asmAndType;
        }

        private void Initialize()
        {
            Initialize(null);
        }

        private void Initialize(ILanguageProvider language)
        {
            if (initialized)
            {
                return;
            }

            sourceName = flowSourceObject.Name;
            fullName = asmAndType.Type.FullName;
            flowToolTipText = language == null ? flowSourceObject.ToolTipText : language.GetFlowSourceToolTipText(flowSourceObject.ToolTipTextKey);
            warning = language == null ? flowSourceObject.Warning : (String.IsNullOrEmpty(flowSourceObject.WarningKey) ? "" : language.GetFlowSourceToolTipText(flowSourceObject.WarningKey));
            ToolTipText = flowToolTipText;
#if DEBUG
            Write(flowSourceObject.ToolTipTextKey, flowSourceObject.ToolTipText);
#endif
            summary = flowSourceObject.Summary;
            inEvents = CreateInMethods(language);
            outEvents = CreateOutMethods(language);
            inProperties = CreateInProperties(language);
            outProperties = CreateOutProperties(language);

            initialized = true;
        }

        private Item[] CreateInMethods(ILanguageProvider language)
        {
            var ret = new List<Item>();
            int iter = 0;
            foreach (string name in flowSourceObject.InMethodNames)
            {
                ret.Add(new Item(flowSourceObject.InMethods[iter].ReplacedNames)
                {
                    Name = name,
                    Type = "event",
                    TypeText = "event",
                    ToolTip = GetToolTip(language, flowSourceObject.InMethods[iter])
                });
#if DEBUG
                Write(flowSourceObject.InMethods[iter]);
#endif
                iter++;
            }
            return ret.ToArray();
        }

        private Item[] CreateOutMethods(ILanguageProvider language)
        {
            var ret = new List<Item>();
            int iter = 0;
            foreach (string name in flowSourceObject.OutMethodNames)
            {
                ret.Add(new Item(flowSourceObject.OutMethods[iter].ReplacedNames)
                {
                    Name = name,
                    Type = "event",
                    TypeText = "event",
                    ToolTip = GetToolTip(language, flowSourceObject.OutMethods[iter]),
                    IsOut = true
                });
#if DEBUG
                Write(flowSourceObject.OutMethods[iter]);
#endif
                iter++;
            }
            return ret.ToArray();
        }

        private Item[] CreateInProperties(ILanguageProvider language)
        {
            var ret = new List<Item>();
            int iter = 0;
            foreach (string name in flowSourceObject.InPropertyNames)
            {
                ret.Add(new Item(flowSourceObject.InProperties[iter].ReplacedNames)
                {
                    Name = name,
                    Type = flowSourceObject.InProperties[iter].MemberInfo.PropertyType.FullName,
                    PropertyType = flowSourceObject.InProperties[iter].MemberInfo.PropertyType,
                    ToolTip = GetToolTip(language, flowSourceObject.InProperties[iter])
                });
#if DEBUG
                Write(flowSourceObject.InProperties[iter]);
                if (!propertyTypes.Contains(flowSourceObject.InProperties[iter].MemberInfo.PropertyType))
                {
                    propertyTypes.Add(flowSourceObject.InProperties[iter].MemberInfo.PropertyType);
                }
#endif
                iter++;
            }
            return ret.ToArray();
        }

        private Item[] CreateOutProperties(ILanguageProvider language)
        {
            var ret = new List<Item>();
            int iter = 0;
            foreach (string name in flowSourceObject.OutPropertyNames)
            {
                ret.Add(new Item(flowSourceObject.OutProperties[iter].ReplacedNames)
                {
                    Name = name,
                    PropertyType = flowSourceObject.OutProperties[iter].MemberInfo.PropertyType,
                    Type = flowSourceObject.OutProperties[iter].MemberInfo.PropertyType.FullName,
                    ToolTip = GetToolTip(language, flowSourceObject.OutProperties[iter]),
                    IsOut = true
                });
#if DEBUG
                Write(flowSourceObject.OutProperties[iter]);
                if (!propertyTypes.Contains(flowSourceObject.OutProperties[iter].MemberInfo.PropertyType))
                {
                    propertyTypes.Add(flowSourceObject.OutProperties[iter].MemberInfo.PropertyType);
                }
#endif
                iter++;
            }
            return ret.ToArray();
        }

        private string GetToolTip<T>(ILanguageProvider language, CustomMemberInfo<T> memberInfo)
            where T : MemberInfo
        {
            if (language == null)
            {
                return memberInfo.ToolTipText;
            }
            else
            {
                return language.GetFlowSourceToolTipText(memberInfo.ToolTipTextKey);
            }
        }

        public void UpdateLanguage(ILanguageProvider language)
        {
            initialized = false;
            Initialize(language);
        }

#if DEBUG
        private void Write<T>(CustomMemberInfo<T> memberInfo)
            where T : MemberInfo
        {
            Write(memberInfo.ToolTipTextKey, memberInfo.ToolTipText);
        }

        private static Dictionary<string, string> dict = new Dictionary<string, string>();

        private void Write(string key, string text)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, text);
            }
        }

        public static void WriteToText(string path)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                var list = new List<string>();
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    list.Add(String.Format("[{0}]{1}", kvp.Key, kvp.Value));
                }
                list.Sort();
                foreach (string line in list)
                {
                    writer.WriteLine(line);
                }
            }
        }

        public static void CheckPropertyType()
        {
            foreach (Type type in propertyTypes)
            {
                if (!FlowDrawPanel.TypeAliasDictionary.ContainsKey(type.FullName))
                {
                    Console.WriteLine("{0} doesn't have type alias", type.FullName);
                }
            }
        }

        public static void CheckPropertyColor(Dictionary<string, Color> colorDict)
        {
            foreach (Type type in propertyTypes)
            {
                if (!colorDict.ContainsKey(type.FullName))
                {
                    Console.WriteLine("{0} doesn't have own color", type.FullName);
                }
            }
        }

        public static void CheckEnumTypeFormatter()
        {
            foreach (Type type in propertyTypes)
            {
                if (type.IsEnum && TypeFormatterManager.GetFormatter(type) == null)
                {
                    Console.WriteLine("{0} doesn't have type formatter", type.FullName);
                }
            }
        }

        public static void CheckTypeSerializer()
        {
            foreach (Type type in propertyTypes)
            {
                if (!type.IsEnum && TypeSerializerManager.GetSerializer(type) == null)
                {
                    Console.WriteLine("{0} doesn't have type serializer", type.FullName);
                }
            }
        }
#endif

        public string ConvertPropertyType(Type type)
        {
            if (typeTextTypeDict[type.FullName] == null)
            {
                typeTextTypeDict.Add(type.FullName, type);
            }

            if (FlowDrawPanel.TypeAliasDictionary.ContainsKey(type.FullName))
            {
                return FlowDrawPanel.TypeAliasDictionary[type.FullName];
            }
            else
            {
                return type.FullName;
            }

        }

        public static Type GetTypeFromString(string typeString)
        {
            return typeTextTypeDict[typeString] as Type;
        }
    }
}
