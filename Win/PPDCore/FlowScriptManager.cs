using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;
using PPDFramework.Logger;
using PPDFramework.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PPDCore
{
    public class FlowScriptManager : DisposableComponent
    {
        Engine engine;

        public BPMManager BPMManager
        {
            get;
            private set;
        }

        public UpdateManager UpdateManager
        {
            get;
            private set;
        }

        public CalculatePositionManager CalculatePositionManager
        {
            get;
            private set;
        }

        public ProcessEvaluateManager ProcessEvaluateManager
        {
            get;
            private set;
        }

        public CreateMarkManager CreateMarkManager
        {
            get;
            private set;
        }

        public ProcessMarkBPMManager ProcessMarkBPMManager
        {
            get;
            private set;
        }

        public ProcessMissPressManager ProcessMissPressManager
        {
            get;
            private set;
        }

        public ProcessAllowedButtonsManager ProcessAllowedButtonsManager
        {
            get;
            private set;
        }

        public StageManager StageManager
        {
            get;
            private set;
        }

        public GameResultManager GameResultManager
        {
            get;
            private set;
        }

        public InputManager InputManager
        {
            get;
            private set;
        }

        public PauseManager PauseManager
        {
            get;
            private set;
        }

        public bool SuppressErrorLog
        {
            get;
            set;
        }

        public ScriptResourceManager[] ScriptResourceManagers
        {
            get { return scriptList.Select(s => s.Value.ResourceManager).ToArray(); }
        }

        Dictionary<string, object> afterLoadList;
        List<KeyValuePair<MemoryStream, ScriptInfo>> scriptList;
        List<IPriorityManager> priorityManagers;

        private TcpDebugControllerClient controllerClient;
        private DebugController debugController;

        public FlowScriptManager(PPDDevice device, ISound sound)
        {
            priorityManagers = new List<IPriorityManager>();
            engine = new Engine();
            engine.Error += engine_Error;
            BPMManager = new BPMManager();
            UpdateManager = new UpdateManager(engine);
            CalculatePositionManager = new CalculatePositionManager(engine);
            ProcessEvaluateManager = new ProcessEvaluateManager(engine);
            CreateMarkManager = new CreateMarkManager(engine);
            ProcessMarkBPMManager = new ProcessMarkBPMManager(engine);
            ProcessMissPressManager = new ProcessMissPressManager(engine);
            ProcessAllowedButtonsManager = new ProcessAllowedButtonsManager(engine);
            InputManager = new InputManager();
            PauseManager = new PauseManager(engine);
            priorityManagers.AddRange(new IPriorityManager[]{
                UpdateManager,
                CalculatePositionManager,
                ProcessEvaluateManager,
                CreateMarkManager,
                ProcessMarkBPMManager,
                ProcessMissPressManager,
                ProcessAllowedButtonsManager,
            });
            GameResultManager = new GameResultManager();
            StageManager = new StageManager(device);
            afterLoadList = new Dictionary<string, object>();

            scriptList = new List<KeyValuePair<MemoryStream, ScriptInfo>>();
            ModManager.Instance.WaitForLoadFinish();
            if (!ModManager.Instance.Initialized)
            {
                ModManager.Instance.EnumerateClasses();
            }

            if (PPDSetting.Setting.IsDebug && TcpDebugControllerBase.IsListening())
            {
                debugController = new DebugController();
                controllerClient = new TcpDebugControllerClient(debugController);
                try
                {
                    controllerClient.Create();
                }
                catch
                {
                    MessageBox.Show("Failed to create debug connection");
                    debugController = null;
                    controllerClient = null;
                }
            }
        }

        void engine_Error(FlowExecutionException obj)
        {
            if (SuppressErrorLog)
            {
                return;
            }
            LogManager.Instance.AddLog(new LogInfo(LogLevel.Error, obj.ToString()));
        }

        public void Retry()
        {
            StageManager.Clear();
            InputManager.Initialize();
            PauseManager.Initialize();
            engine.Reset();
            engine.Start();
        }

        public void LoadScript(Stream stream, ScriptResourceManager scriptResourceManager, string fileName)
        {
            LoadScript(stream, scriptResourceManager, null, fileName);
        }

        public void LoadScript(Stream stream, ScriptResourceManager scriptResourceManager, ModInfo modInfo)
        {
            LoadScript(stream, scriptResourceManager, modInfo, null);
        }

        public void LoadScript(Stream stream, ScriptResourceManager scriptResourceManager, ModInfo modInfo, string fileName)
        {
            using (stream)
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                scriptList.Add(new KeyValuePair<MemoryStream, ScriptInfo>(new MemoryStream(buffer),
                    new ScriptInfo(scriptResourceManager, modInfo, fileName)));
            }
        }

        public void AddItem(string key, object value)
        {
            afterLoadList[key] = value;
        }

        public void RemoveItem(string key)
        {
            if (afterLoadList.ContainsKey(key))
            {
                afterLoadList.Remove(key);
            }
        }

        public void Initialize()
        {
            LoadScript();
            engine.Start();
        }

        private void LoadScript()
        {
            var mods = scriptList.Where(s => s.Value.ModInfo != null).Select(s => s.Value.ModInfo).Distinct().ToArray();
            var defaultModSettingManager = new ModSettingManager();
            var defaultScopeGuid = Guid.NewGuid();
            foreach (KeyValuePair<MemoryStream, ScriptInfo> kvp in scriptList)
            {
                kvp.Key.Seek(0, SeekOrigin.Begin);
                FlowSourceManager manager;
                if (kvp.Value.ModInfo == null)
                {
                    manager = engine.Load(kvp.Key, false, defaultScopeGuid, debugController);
                }
                else
                {
                    manager = engine.Load(kvp.Key, false, kvp.Value.ModInfo.Guid, debugController);
                }
                if (kvp.Value.FileName != null && controllerClient != null)
                {
                    if (controllerClient.BreakPoints.TryGetValue(kvp.Value.FileName, out int[] breakPoints))
                    {
                        foreach (var id in breakPoints)
                        {
                            manager.AddBreakPoint(id);
                        }
                    }
                    controllerClient.AddFlowSourceManager(kvp.Value.FileName, manager);
                }

                manager.Items["BPMManager"] = BPMManager;
                manager.Items["UpdateManager"] = UpdateManager;
                manager.Items["CalculatePositionManager"] = CalculatePositionManager;
                manager.Items["StageManager"] = StageManager;
                manager.Items["ProcessEvaluateManager"] = ProcessEvaluateManager;
                manager.Items["GameResultManager"] = GameResultManager;
                manager.Items["CreateMarkManager"] = CreateMarkManager;
                manager.Items["ProcessMarkBPMManager"] = ProcessMarkBPMManager;
                manager.Items["ProcessMissPressManager"] = ProcessMissPressManager;
                manager.Items["ProcessAllowedButtonsManager"] = ProcessAllowedButtonsManager;
                manager.Items["InputManager"] = InputManager;
                if (kvp.Value.ResourceManager != null)
                {
                    manager.Items["ResourceManager"] = kvp.Value.ResourceManager;
                }
                if (kvp.Value.ModInfo != null)
                {
                    manager.Items["ModSettingManager"] = kvp.Value.ModInfo.ModSettingManager;
                    manager.Items["ModInfo"] = kvp.Value.ModInfo;
                }
                else
                {
                    manager.Items["ModSettingManager"] = defaultModSettingManager;
                }
                manager.Items["ModInfos"] = mods;

                foreach (KeyValuePair<string, object> pair in afterLoadList)
                {
                    manager.Items[pair.Key] = pair.Value;
                }

                manager.Initialize();
                if (kvp.Value.ModInfo == null)
                {
                    manager.Call(ModSettingManager.MODSTART, null);
                }
            }
            foreach (var modSetting in defaultModSettingManager.ModSettings)
            {
                defaultModSettingManager[modSetting.Key] = modSetting.Default;
            }
        }

        public void Update(bool dontUpdateExtraProcess)
        {
            engine.Update();
            if (!dontUpdateExtraProcess)
            {
                StageManager.Update();
            }
        }

        public void Call(string callBackName, params object[] args)
        {
            engine.Call(callBackName, args);
        }

        public bool ContainsNode(string nodeName)
        {
            return engine.ContainsNode(nodeName);
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            foreach (KeyValuePair<MemoryStream, ScriptInfo> kvp in scriptList)
            {
                if (kvp.Value.ResourceManager != null)
                {
                    kvp.Value.ResourceManager.Dispose();
                }
            }
            if (controllerClient != null)
            {
                controllerClient.Close();
                controllerClient = null;
            }
        }
    }
}
