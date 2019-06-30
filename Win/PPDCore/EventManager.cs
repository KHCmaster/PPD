using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;
using PPDFrameworkCore;
using System.Collections.Generic;
using System.IO;

namespace PPDCore
{
    public class EventManager : UpdatableGameComponent, IEventManager
    {
        public delegate void VolumeHandler(int volume);
        public event VolumeHandler ChangeMovieVolume;
        IEVDData[] data;
        bool initialized;
        int iter;
        int[] volumePercents = new int[10];
        bool[] keepPlayings = new bool[10];
        bool[] releaseSounds = new bool[10];

        float defaultbpm;
        FlowScriptManager scriptManager;
        List<float[]> stoplist = new List<float[]>();
        SortedList<float, DisplayState> displayStateCache;
        SortedList<float, NoteType> noteTypeCache;
        SortedList<float, ButtonType[]> initializeOrderCache;
        SortedList<float, float> slideScaleCache;
        ButtonType[] defaultInitializeOrder;
        MainGameConfigBase config;

        private BPMManager BPMManager
        {
            get
            {
                return scriptManager.BPMManager;
            }
        }

        public EventManager(PPDGameUtility ppdgameutility, MainGameConfigBase config, FlowScriptManager scriptManager)
        {
            LoadData(ppdgameutility.SongInformation.DirectoryPath, ppdgameutility.Difficulty);
            InnerStruct(ppdgameutility, config, scriptManager);
        }
        public EventManager(PPDGameUtility ppdgameutility, Stream stream, MainGameConfigBase config, FlowScriptManager scriptManager)
        {
            LoadData(stream);
            InnerStruct(ppdgameutility, config, scriptManager);
        }

        private void InnerStruct(PPDGameUtility ppdgameutility, MainGameConfigBase config, FlowScriptManager scriptManager)
        {
            this.config = config;
            this.defaultbpm = ppdgameutility.SongInformation.BPM;
            this.scriptManager = scriptManager;

            BPMManager.CurrentBPM = defaultbpm;
            BPMManager.TargetBPM = defaultbpm;

            displayStateCache = new SortedList<float, DisplayState>();
            noteTypeCache = new SortedList<float, NoteType>();
            initializeOrderCache = new SortedList<float, ButtonType[]>();
            slideScaleCache = new SortedList<float, float>();
            defaultInitializeOrder = new ButtonType[10];
            for (int i = 0; i < 10; i++)
            {
                defaultInitializeOrder[i] = (ButtonType)i;
            }
            for (int i = 0; i < volumePercents.Length; i++)
            {
                volumePercents[i] = 90;
                keepPlayings[i] = false;
            }
            if (initialized)
            {
                InitializeState();
            }
        }

        private void InitializeState()
        {
            float stopstarttime = -1;
            foreach (IEVDData evddata in data)
            {
                switch (evddata.EventType)
                {
                    case EventType.ChangeDisplayState:
                        var cdse = evddata as ChangeDisplayStateEvent;
                        displayStateCache.Add(cdse.Time, cdse.DisplayState);
                        break;
                    case EventType.ChangeMoveState:
                        var cmse = evddata as ChangeMoveStateEvent;
                        if (stopstarttime < 0)
                        {
                            if (cmse.MoveState == MoveState.Stop)
                            {
                                stopstarttime = cmse.Time;
                            }
                        }
                        else
                        {
                            if (cmse.MoveState == MoveState.Normal)
                            {
                                var temp = new float[] { stopstarttime, cmse.Time };
                                stoplist.Add(temp);
                                stopstarttime = -1;
                            }
                        }
                        break;
                    case EventType.ChangeNoteType:
                        var cace = evddata as ChangeNoteTypeEvent;
                        noteTypeCache.Add(cace.Time, cace.NoteType);
                        break;
                    case EventType.ChangeInitializeOrder:
                        var cioe = evddata as ChangeInitializeOrderEvent;
                        initializeOrderCache.Add(cioe.Time, cioe.InitializeOrder);
                        break;
                    case EventType.ChangeSlideScale:
                        var csse = evddata as ChangeSlideScaleEvent;
                        slideScaleCache.Add(csse.Time, csse.SlideScale);
                        break;
                }
            }
        }
        private void LoadData(string dir, Difficulty difficulty)
        {
            string filename = DifficultyUtility.ConvertDifficulty(difficulty) + ".evd";
            string path = dir + "\\" + filename;
            if (File.Exists(path))
            {
                data = EVDReader.Read(path);
                initialized = true;
            }
            else
            {
                initialized = false;
            }
        }

        private void LoadData(Stream stream)
        {
            try
            {
                data = EVDReader.Read(stream);
                initialized = true;
            }
            catch
            {
                initialized = false;
            }
        }
        public void Retry()
        {
            iter = 0;
            if (ChangeMovieVolume != null) ChangeMovieVolume.Invoke(-1000);
            for (int i = 0; i < volumePercents.Length; i++)
            {
                volumePercents[i] = 90;
                keepPlayings[i] = false;
                releaseSounds[i] = false;
            }

            BPMManager.CurrentBPM = defaultbpm;
            BPMManager.TargetBPM = defaultbpm;
        }
        public void Seek(float time)
        {
            Retry();
            if (!initialized) return;
            ChangeEvent(time);
        }
        private void ChangeEvent(float time)
        {
            if (iter >= data.Length) return;
            IEVDData evddata = data[iter];
            while (time >= evddata.Time)
            {
                switch (evddata.EventType)
                {
                    case EventType.ChangeBPM:
                        var cbe = evddata as ChangeBPMEvent;
                        BPMManager.TargetBPM = cbe.BPM;
                        break;
                    case EventType.ChangeSoundPlayMode:
                        var cspme = evddata as ChangeSoundPlayModeEvent;
                        keepPlayings[cspme.Channel - 1] = cspme.KeepPlaying;
                        break;
                    case EventType.ChangeVolume:
                        var cve = evddata as ChangeVolumeEvent;
                        if (cve.Channel == 0)
                        {
                            if (ChangeMovieVolume != null) ChangeMovieVolume.Invoke(-100 * (100 - cve.Volume));
                        }
                        else if (cve.Channel > 0 && cve.Channel <= 10)
                        {
                            volumePercents[cve.Channel - 1] = cve.Volume;
                        }
                        break;
                    case EventType.RapidChangeBPM:
                        var rcbe = evddata as RapidChangeBPMEvent;
                        if (rcbe.Rapid)
                        {
                            BPMManager.CurrentBPM = rcbe.BPM;
                        }
                        BPMManager.TargetBPM = rcbe.BPM;
                        break;
                    case EventType.ChangeReleaseSound:
                        var crse = evddata as ChangeReleaseSoundEvent;
                        releaseSounds[crse.Channel - 1] = crse.ReleaseSound;
                        break;
                }
                iter++;
                if (iter >= data.Length) return;
                evddata = data[iter];
            }
        }

        public float GetSlideScale(float markTime)
        {
            float ret = 1;
            foreach (KeyValuePair<float, float> data in slideScaleCache)
            {
                if (markTime < data.Key) break;
                ret = data.Value;
            }
            return ret;
        }

        public DisplayState GetCorrectDisplaystate(float markTime)
        {
            if (config.DisplayState == DisplayState.Normal)
            {
                DisplayState ret = DisplayState.Normal;
                foreach (KeyValuePair<float, DisplayState> data in displayStateCache)
                {
                    if (markTime < data.Key) break;
                    ret = data.Value;
                }
                return ret;
            }
            else
            {
                return config.DisplayState;
            }
        }
        public NoteType GetNoteType(float markTime)
        {
            NoteType ret = NoteType.Normal;
            foreach (KeyValuePair<float, NoteType> data in noteTypeCache)
            {
                if (markTime < data.Key) break;
                ret = data.Value;
            }
            return ret;
        }
        public float GetCorrectTime(float currentTime, float markTime)
        {
            float ret = currentTime;
            for (int i = 0; i < stoplist.Count; i++)
            {
                float[] temp = stoplist[i];
                if (markTime <= temp[1]) break;
                if (currentTime >= temp[1]) continue;
                if (currentTime >= temp[0] && currentTime <= temp[1])
                {
                    ret = temp[1];
                }
                if (currentTime < temp[0])
                {
                    ret += temp[1] - temp[0];
                }
            }
            return ret;
        }
        public ButtonType[] GetInitlaizeOrder(float markTime)
        {
            ButtonType[] ret = defaultInitializeOrder;
            foreach (KeyValuePair<float, ButtonType[]> data in initializeOrderCache)
            {
                if (markTime < data.Key) break;
                ret = data.Value;
            }
            return ret;
        }
        public void Update(float time)
        {
            if (initialized)
            {
                ChangeEvent(time);
                BPMManager.Step();
            }
        }
        public float BPM
        {
            get
            {
                return BPMManager.CurrentBPM;
            }
        }
        public int GetVolPercent(int i)
        {
            if (i >= 0 && i < 10)
            {
                return volumePercents[i];
            }
            return 0;
        }
        public bool GetReleaseSound(int i)
        {
            if (i >= 0 && i < 10)
            {
                return releaseSounds[i];
            }
            return false;
        }
        public int[] VolumePercents
        {
            get
            {
                return volumePercents;
            }
        }
        public bool[] KeepPlayings
        {
            get
            {
                return keepPlayings;
            }
        }
        public bool[] ReleaseSounds
        {
            get
            {
                return releaseSounds;
            }
        }

        public bool SetVolumePercent(MarkType button, int volPercent)
        {
            if (0 <= button && button <= MarkType.L)
            {
                volumePercents[(int)button] = volPercent;
                return true;
            }
            return false;
        }

        public bool SetKeepPlaying(MarkType button, bool keepPlaying)
        {
            if (0 <= button && button <= MarkType.L)
            {
                this.keepPlayings[(int)button] = keepPlaying;
                return true;
            }
            return false;
        }

        public bool SetReleaseSound(MarkType button, bool releaseSound)
        {
            if (0 <= button && button <= MarkType.L)
            {
                releaseSounds[(int)button] = releaseSound;
                return true;
            }
            return false;
        }

        public int GetVolumePercent(MarkType button)
        {
            return volumePercents[(int)button];
        }

        public bool GetKeepPlaying(MarkType button)
        {
            return keepPlayings[(int)button];
        }

        public bool GetReleaseSound(MarkType button)
        {
            return releaseSounds[(int)button];
        }
    }
}
