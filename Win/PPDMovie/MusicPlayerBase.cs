using DirectShow;
using PPDFramework;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

namespace PPDMovie
{
    public abstract class MusicPlayerBase : PlayerBase
    {
        protected IGraphBuilder graphBuilder2;
        protected IMediaSeeking mediaSeeking;
        protected IMediaControl mediaControl;
        protected IBasicAudio basicAudio;
        protected IMediaEventEx mediaeventEx;
        protected IMediaPosition mediaPosition;
        protected bool autoRender = true;
        private const int WM_GRAPHNOTIFY = 0x0400 + 13;

        private int userVolume = 100;

        public int UserVolume
        {
            get { return userVolume; }
            set
            {
                if (userVolume != value)
                {
                    userVolume = value;
                    if (initialized)
                    {
                        Volume = innerVolume;
                    }
                }
            }
        }

        public override int Volume
        {
            get
            {
                return base.Volume;
            }
            set
            {
                base.Volume = value;
                if (this.basicAudio != null)
                {
                    var innerVolumeRatio = (innerVolume - VolumeSilence) / (float)-VolumeSilence;
                    var userVolumeRatio = userVolume / 100f;
                    var volume = Math.Min(0, SoundMasterControl.Instance.GetVolume((int)(innerVolumeRatio * userVolumeRatio * 10000 + VolumeSilence), SoundType.Movie));
                    var hr = this.basicAudio.put_Volume(volume);
                }
            }
        }

        public override double PlayRate
        {
            get
            {
                if (mediaSeeking != null)
                {
                    mediaSeeking.GetRate(out double rate);
                    return rate;
                }
                return 1;
            }
            set
            {
                if (mediaSeeking != null)
                {
                    mediaSeeking.SetRate(value);
                }
            }
        }

        protected MusicPlayerBase(PPDDevice device, string filename)
            : base(device, filename)
        {
        }

        public override int Initialize()
        {
            if (!File.Exists(filename))
            {
                return -1;
            }

            int hr = 0;
            if (autoRender)
            {
                hr = this.graphBuilder2.RenderFile(filename, null);
                DsError.ThrowExceptionForHR(hr);
            }
            // QueryInterface for DirectShow interfaces
            this.mediaControl = (IMediaControl)this.graphBuilder2;
            this.mediaPosition = (IMediaPosition)this.graphBuilder2;
            this.mediaSeeking = (IMediaSeeking)this.graphBuilder2;

            // Query for audio interfaces, which may not be relevant for video-only files
            this.basicAudio = this.graphBuilder2 as IBasicAudio;
            this.mediaeventEx = this.graphBuilder2 as IMediaEventEx;


            if (mediaControl == null)
            {
                var exp = new Exception("Failed to get mediacontrol");
                throw exp;
            }
            if (mediaSeeking == null)
            {
                var exp = new Exception("Failed to get mediaseeking");
                throw exp;
            }
            if (mediaPosition == null)
            {
                var exp = new Exception("Failed to get mediaposition");
                throw exp;
            }
            if (basicAudio == null)
            {
                var exp = new Exception("Failed to get basicaudio");
                throw exp;
            }
            if (mediaeventEx == null)
            {
                var exp = new Exception("Failed to get mediaeventEx");
                throw exp;
            }

            DumpFilterLog();

            hr = this.mediaPosition.get_Duration(out this.length);
            DsError.ThrowExceptionForHR(hr);
            state = MovieFadeState.None;
            Volume = MaximumVolume;
            hr = mediaeventEx.SetNotifyWindow(MovieUtility.Window, WM_GRAPHNOTIFY, IntPtr.Zero);
            DsError.ThrowExceptionForHR(hr);
            SoundMasterControl.Instance.MovieVolumeChanged += Instance_MovieVolumeChanged;
            this.initialized = true;

            return 0;
        }

        private void DumpFilterLog()
        {
            //グラフを解析してプリント
            var hr = this.graphBuilder2.EnumFilters(out IEnumFilters fils);
            IBaseFilter[] fil = new IBaseFilter[1];
            IntPtr ptr = IntPtr.Zero;
            using (StreamWriter sw = new StreamWriter(Path.Combine(Utility.AppDir, "filterinfo.log")))
            {
                try
                {
                    while (true)
                    {
                        hr = fils.Next(1, fil, ptr);
                        DsError.ThrowExceptionForHR(hr);
                        var fi = new FilterInfo();
                        hr = fil[0].QueryFilterInfo(fi);
                        try
                        {
                            DsError.ThrowExceptionForHR(hr);
                        }
                        catch
                        {
                            //next works fine 
                            continue;
                        }
                        sw.WriteLine(fi.achName);
                    }
                }
                catch
                {

                }
            }
        }

        public override void Seek(double time)
        {
            if (!initialized)
            {
                return;
            }

            if (time <= this.length && time >= 0)
            {
                var hr = this.mediaPosition.put_CurrentPosition(time);
                DsError.ThrowExceptionForHR(hr);
            }
        }

        public override void Play()
        {
            if (!initialized)
            {
                return;
            }

            var hr = this.mediaControl.Run();
            DsError.ThrowExceptionForHR(hr);
            Playing = true;
        }

        public override void Stop()
        {
            if (!initialized) return;
            var hr = this.mediaControl.StopWhenReady();
            DsError.ThrowExceptionForHR(hr);
            Playing = false;
        }

        public override void Pause()
        {
            if (!initialized) return;
            var hr = this.mediaControl.Pause();
            DsError.ThrowExceptionForHR(hr);
            Playing = false;
        }

        void Instance_MovieVolumeChanged()
        {
            Volume = innerVolume;
        }

        public override void releaseCOM()
        {
            if (!initialized) return;
            int hr = 0;
            // Stop previewing data
            if (this.mediaControl != null)
            {
                hr = this.mediaControl.Stop();
                DsError.ThrowExceptionForHR(hr);
                hr = mediaControl.GetState(0, out FilterState state);
                DsError.ThrowExceptionForHR(hr);
                while (state != FilterState.Stopped)
                {
                    System.Threading.Thread.Sleep(10);
                    hr = mediaControl.GetState(0, out state);
                }
            }
            // Relinquish ownership (IMPORTANT!) of the video window.
            // Failing to call put_Owner can lead to assert failures within
            // the video renderer, as it still assumes that it has a valid
            // parent window.
            if (mediaeventEx != null)
            {
                mediaeventEx.SetNotifyWindow(IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero);
                mediaeventEx = null;
            }

            if (mediaControl != null)
            {
                Marshal.ReleaseComObject(this.mediaControl); this.mediaControl = null;
            }
            if (mediaPosition != null)
            {
                Marshal.ReleaseComObject(this.mediaPosition); this.mediaPosition = null;
            }
            if (mediaSeeking != null)
            {
                Marshal.ReleaseComObject(this.mediaSeeking); this.mediaSeeking = null;
            }
            if (basicAudio != null)
            {
                Marshal.ReleaseComObject(this.basicAudio); this.basicAudio = null;
            }
            if (this.graphBuilder2 != null)
            {
                //RemoveAllFilters();
                Marshal.ReleaseComObject(this.graphBuilder2); this.graphBuilder2 = null;
            }
            SoundMasterControl.Instance.MovieVolumeChanged -= Instance_MovieVolumeChanged;
            this.initialized = false;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void RemoveAllFilters()
        {
            int hr = 0;
            var filtersArray = new ArrayList();

            hr = this.graphBuilder2.EnumFilters(out IEnumFilters enumFilters);
            DsError.ThrowExceptionForHR(hr);

            IBaseFilter[] filters = new IBaseFilter[1];

            while (enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
            {
                filtersArray.Add(filters[0]);
            }

            foreach (IBaseFilter filter in filtersArray)
            {
                hr = this.graphBuilder2.RemoveFilter(filter);
                var retryCount = 0;
                while (Marshal.ReleaseComObject(filter) > 0)
                {
                    if (retryCount >= 10)
                    {
                        break;
                    }
                    retryCount++;
                }
            }
        }

        public override double MoviePosition
        {
            get
            {
                if (!initialized)
                {
                    return 0;
                }

                this.mediaPosition.get_CurrentPosition(out double check);
                return check;
            }
        }

        public override IMediaEventEx MediaEventEx
        {
            get { return mediaeventEx; }
        }
    }
}
