using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FlowScriptEngine
{
    public static class FlowSourceEnumerator
    {
        public static IEnumerable<AssemblyAndType> EnumerateFromFile(string filePath, Type[] enumerateTypes)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("{0} does not exist.", filePath);
                yield break;
            }

            var asm = System.Reflection.Assembly.LoadFrom(filePath);
            foreach (Type t in asm.GetTypes())
            {
                if (t.IsClass && t.IsPublic && !t.IsAbstract)
                {
                    var asmAndType = new AssemblyAndType(asm, t);
                    foreach (Type enumerateType in enumerateTypes)
                    {
                        if (t.IsSubclassOf(enumerateType))
                        {
                            yield return asmAndType;
                        }
                    }

                    if (!ShouldBeIgnored(t))
                    {
                        if (t.IsSubclassOf(typeof(FlowSourceObjectBase)))
                        {
                            FlowSourceObjectManager.AddFlowSourceObject(asmAndType);
                        }
                        else if (t.IsSubclassOf(typeof(TypeConverterBase)))
                        {
                            TypeConverterManager.AddConverter((TypeConverterBase)asmAndType.Assembly.CreateInstance(asmAndType.Type.FullName));
                        }
                        else if (t.IsSubclassOf(typeof(TypeConverterFromBase)))
                        {
                            TypeConverterManager.AddConverter((TypeConverterFromBase)asmAndType.Assembly.CreateInstance(asmAndType.Type.FullName));
                        }
                        else if (t.IsSubclassOf(typeof(TypeConverterToBase)))
                        {
                            TypeConverterManager.AddConverter((TypeConverterToBase)asmAndType.Assembly.CreateInstance(asmAndType.Type.FullName));
                        }
                        else if (t.IsSubclassOf(typeof(TypeFormatterBase)))
                        {
                            TypeFormatterManager.AddFormatter((TypeFormatterBase)asmAndType.Assembly.CreateInstance(asmAndType.Type.FullName));
                        }
                        else if (t.IsSubclassOf(typeof(TypeSerializerBase)))
                        {
                            TypeSerializerManager.AddSerializer((TypeSerializerBase)asmAndType.Assembly.CreateInstance(asmAndType.Type.FullName));
                        }
                    }
                }
            }
        }

        private static bool ShouldBeIgnored(MemberInfo memberInfo)
        {
            var objects = memberInfo.GetCustomAttributes(false);
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] is IgnoreAttribute)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
