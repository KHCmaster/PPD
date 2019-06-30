using PPDFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace PPD
{
    class GameLoader
    {
        static ClassType[] classTypeList = Enum.GetValues(typeof(ClassType)) as ClassType[];
        static Dictionary<Type, ClassType> typeDictionary;
        enum ClassType
        {
            LoadingBase = 0,
            EntrySceneManagerBase,
            GameInformationBase
        }
        public string Path
        {
            get;
            private set;
        }
        public string Version
        {
            get;
            private set;
        }

        Dictionary<ClassType, string> Names;
        private Assembly assembly;
        private FileVersionInfo fileVersionInfo;

        static GameLoader()
        {
            typeDictionary = new Dictionary<Type, ClassType>
            {
                {typeof(EntrySceneManagerBase),ClassType.EntrySceneManagerBase},                {typeof(GameInformationBase),ClassType.GameInformationBase},                {typeof(LoadingBase),ClassType.LoadingBase}            };
        }

        public GameLoader(string path)
        {
            Path = path;
            Names = new Dictionary<ClassType, string>();
        }
        public bool Load()
        {
            try
            {
                var dic = new Dictionary<ClassType, bool>();
                foreach (ClassType classType in classTypeList)
                {
                    dic.Add(classType, false);
                }
                assembly = System.Reflection.Assembly.LoadFrom(Path);
                foreach (Type t in assembly.GetTypes())
                {
                    if (t.IsClass && t.IsPublic && !t.IsAbstract)
                    {
                        CheckClass(t, dic);
                    }
                }
                foreach (KeyValuePair<ClassType, bool> kvp in dic)
                {
                    if (!kvp.Value)
                    {
                        throw new PPDException(PPDExceptionType.SkinIsNotCorrectlyImplemented,
                            String.Format("{0} is not implemented", kvp.Key), null);
                    }
                }

                fileVersionInfo = FileVersionInfo.GetVersionInfo(Path);
                Version = fileVersionInfo.FileVersion;

                return true;
            }
            catch (Exception e)
            {
                throw new PPDException(PPDExceptionType.SkinIsNotCorrectlyImplemented, e);
            }
        }

        private void CheckClass(Type t, Dictionary<ClassType, bool> dic)
        {
            foreach (KeyValuePair<Type, ClassType> kvp in typeDictionary)
            {
                if (t.IsSubclassOf(kvp.Key))
                {
                    dic[kvp.Value] = true;
                    Names.Add(kvp.Value, t.FullName);
                }
            }
        }

        public string GetFullName()
        {
            return assembly.GetName().Name;
        }

        public LoadingBase GetLoading(PPDDevice device)
        {
            try
            {
                var ret = (LoadingBase)assembly.CreateInstance(Names[ClassType.LoadingBase], false,
                    BindingFlags.Public | BindingFlags.Instance, null, new object[] { device }, null, null);
                return ret;
            }
            catch
            {
                return null;
            }
        }

        public EntrySceneManagerBase GetEntrySceneManager()
        {
            try
            {
                var ret = (EntrySceneManagerBase)assembly.CreateInstance(Names[ClassType.EntrySceneManagerBase]);
                return ret;
            }
            catch
            {
                return null;
            }
        }

        public GameInformationBase GetGameInformation()
        {
            try
            {
                var ret = (GameInformationBase)assembly.CreateInstance(Names[ClassType.GameInformationBase]);
                return ret;
            }
            catch
            {
                return null;
            }
        }
    }
}
