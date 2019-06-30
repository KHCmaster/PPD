using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;

namespace PPD
{
    public class EventManager : UpdatableGameComponent
    {
        public delegate void VolumeHandler(int volume);
        public event VolumeHandler ChangeMovieVolume;
        IEVDData[] data;
        bool initialized;
        int iter;
        int[] volumePercents = new int[10];
        bool[] keepPlaying = new bool[10];
        bool[] releaseSounds = new bool[10];

        float defaultbpm;
        float currentbpm;
        float targetbpm;
        List<float[]> stoplist = new List<float[]>();
        SortedList<float, DisplayState> DstateCache;
        SortedList<float, bool> ACCache;
        SortedList<float, ButtonType[]> InitializeOrderCache;
        ButtonType[] defaultInitializeOrder;
        public EventManager(PPDGameUtility ppdgameutility)
        {
            loaddata(ppdgameutility.SongInformation.DirectoryPath, ppdgameutility.Difficulty);
            InnerStruct(ppdgameutility);
        }
        public EventManager(PPDGameUtility ppdgameutility, Stream stream)
        {
            loaddata(stream);
            InnerStruct(ppdgameutility);
        }

        private void InnerStruct(PPDGameUtility ppdgameutility)
        {
            this.defaultbpm = ppdgameutility.SongInformation.BPM;
            currentbpm = defaultbpm;
            targetbpm = defaultbpm;
            DstateCache = new SortedList<float, DisplayState>();
            ACCache = new SortedList<float, bool>();
            InitializeOrderCache = new SortedList<float, ButtonType[]>();
            defaultInitializeOrder = new ButtonType[10];
            for (int i = 0; i < 10; i++)
            {
                defaultInitializeOrder[i] = (ButtonType)i;
            }
            for (int i = 0; i < volumePercents.Length; i++)
            {
                volumePercents[i] = 90;
                keepPlaying[i] = false;
            }
            if (initialized)
            {
                initializestate();
            }
        }

        private void initializestate()
        {
            float stopstarttime = -1;
            foreach (IEVDData evddata in data)
            {
                switch (evddata.EventType)
                {
                    case EventType.ChangeDisplayState:
                        ChangeDisplayStateEvent cdse = evddata as ChangeDisplayStateEvent;
                        DstateCache.Add(cdse.Time, cdse.DisplayState);
                        break;
                    case EventType.ChangeMoveState:
                        ChangeMoveStateEvent cmse = evddata as ChangeMoveStateEvent;
                        if (stopstarttime < 0)
                        {
                            if (cmse.MoveState == MoveState.stop)
                            {
                                stopstarttime = cmse.Time;
                            }
                        }
                        else
                        {
                            if (cmse.MoveState == MoveState.normal)
                            {
                                float[] temp = new float[] { stopstarttime, cmse.Time };
                                stoplist.Add(temp);
                                stopstarttime = -1;
                            }
                        }
                        break;
                    case EventType.ChangeACMode:
                        ChangeACEvent cace = evddata as ChangeACEvent;
                        ACCache.Add(cace.Time, cace.AC);
                        break;
                    case EventType.ChangeInitializeOrder:
                        ChangeInitializeOrderEvent cioe = evddata as ChangeInitializeOrderEvent;
                        InitializeOrderCache.Add(cioe.Time, cioe.InitializeOrder);
                        break;
                }
            }
        }
        private void loaddata(string dir, Difficulty difficulty)
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

        private void loaddata(Stream stream)
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
        public void retry()
        {
            iter = 0;
            if (ChangeMovieVolume != null) ChangeMovieVolume.Invoke(-1000);
            for (int i = 0; i < volumePercents.Length; i++)
            {
                volumePercents[i] = 90;
                keepPlaying[i] = false;
                releaseSounds[i] = false;
            }
            currentbpm = defaultbpm;
            targetbpm = defaultbpm;
        }
        public void Seek(float time)
        {
            retry();
            if (!initialized) return;
            changeevent(time);
        }
        private void changeevent(float time)
        {
            if (iter >= data.Length) return;
            IEVDData evddata = data[iter];
            while (time >= evddata.Time)
            {
                switch (evddata.EventType)
                {
                    case EventType.ChangeBPM:
                        ChangeBPMEvent cbe = evddata as ChangeBPMEvent;
                        targetbpm = cbe.BPM;
                        break;
                    case EventType.ChangeSoundPlayMode:
                        ChangeSoundPlayModeEvent cspme = evddata as ChangeSoundPlayModeEvent;
                        keepPlaying[cspme.Channel - 1] = cspme.KeepPlaying;
                        break;
                    case EventType.ChangeVolume:
                        ChangeVolumeEvent cve = evddata as ChangeVolumeEvent;
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
                        RapidChangeBPMEvent rcbe = evddata as RapidChangeBPMEvent;
                        if (rcbe.Rapid)
                        {
                            currentbpm = rcbe.BPM;
                        }
                        targetbpm = rcbe.BPM;
                        break;
                    case EventType.ChangeReleaseSound:
                        ChangeReleaseSoundEvent crse = evddata as ChangeReleaseSoundEvent;
                        releaseSounds[crse.Channel - 1] = crse.ReleaseSound;
                        break;
                }
                iter++;
                if (iter >= data.Length) return;
                evddata = data[iter];
            }
        }
        public DisplayState GetCorrectDisplaystate(float marktime)
        {
            DisplayState ret = DisplayState.Normal;
            foreach (KeyValuePair<float, DisplayState> data in DstateCache)
            {
                if (marktime < data.Key) break;
                ret = data.Value;
            }
            return ret;
        }
        public bool GetACMode(float marktime)
        {
            bool ret = false;
            foreach (KeyValuePair<float, bool> data in ACCache)
            {
                if (marktime < data.Key) break;
                ret = data.Value;
            }
            return ret;
        }
        public float GetCorrectTime(float currenttime, float marktime)
        {
            float ret = currenttime;
            for (int i = 0; i < stoplist.Count; i++)
            {
                float[] temp = stoplist[i];
                if (marktime <= temp[1]) break;
                if (currenttime >= temp[1]) continue;
                if (currenttime >= temp[0] && currenttime <= temp[1])
                {
                    ret = temp[1];
                }
                if (currenttime < temp[0])
                {
                    ret += temp[1] - temp[0];
                }
            }
            return ret;
        }
        public ButtonType[] GetInitlaizeOrder(float markTime)
        {
            ButtonType[] ret = defaultInitializeOrder;
            foreach (KeyValuePair<float, ButtonType[]> data in InitializeOrderCache)
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
                changeevent(time);
                if (currentbpm != targetbpm)
                {
                    if (targetbpm - currentbpm > 0)
                    {
                        currentbpm++;
                        if (targetbpm - currentbpm <= 0)
                        {
                            currentbpm = targetbpm;
                        }
                    }
                    else
                    {
                        currentbpm--;
                        if (targetbpm - currentbpm >= 0)
                        {
                            currentbpm = targetbpm;
                        }
                    }
                }
            }
        }
        public float BPM
        {
            get
            {
                return currentbpm;
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
        public bool[] KeepPlaying
        {
            get
            {
                return keepPlaying;
            }
        }

        public override void Update()
        {

        }
    }
}
