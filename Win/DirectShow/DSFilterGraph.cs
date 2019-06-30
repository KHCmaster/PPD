using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace DirectShow
{
    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public class DSFilterGraphBase : COMHelper, IDisposable
    {
        #region Constants

        protected const int WM_GRAPHNOTIFY = 0x00008001;

        #endregion

        #region Helper Classes

        public class HelperForm : System.Windows.Forms.Form
        {
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_GRAPHNOTIFY)
                {
                    try
                    {
                        if (m.LParam != IntPtr.Zero)
                        {
                            var _graph = (DSFilterGraphBase)Marshal.GetObjectForIUnknown(m.LParam);
                            if (_graph != null)
                            {
                                if (_graph.ProcessGraphMessage())
                                {
                                    _graph.Stop();
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                base.WndProc(ref m);
            }
        }

        #endregion

        #region Variables

        protected Control m_VideoControl = null;
        protected Control m_EventControl = new HelperForm();
        protected IGraphBuilder m_GraphBuilder = null;
        protected IMediaControl m_MediaControl = null;
        protected IMediaSeeking m_MediaSeeking = null;
        protected IMediaPosition m_MediaPosition = null;
        protected IMediaEventEx m_MediaEventEx = null;
        protected IVideoWindow m_VideoWindow = null;
        protected IBasicAudio m_BasicAudio = null;
        protected IBasicVideo m_BasicVideo = null;
        protected IVideoFrameStep m_FrameStep = null;

        protected bool m_bMute = false;
        protected int m_iVolume = 0;
        protected double m_dRate = 1.0;
        protected bool m_bShouldPreview = true;

        protected List<DSFilter> m_Filters = new List<DSFilter>();

        ~DSFilterGraphBase()
        {
            Dispose();
        }

        #endregion

        #region Properties

        public IMediaEventEx MediaEventEx
        {
            get
            {
                return m_MediaEventEx;
            }
        }

        public int Width
        {
            get
            {
                m_BasicVideo.get_VideoWidth(out int width);
                return width;
            }
        }

        public int Height
        {
            get
            {
                m_BasicVideo.get_VideoHeight(out int height);
                return height;
            }
        }

        public bool IsRunning
        {
            get
            {
                if (m_MediaControl != null)
                {
                    FilterState _state;
                    int hr = 0;
                    do
                    {
                        hr = m_MediaControl.GetState(200, out _state);
                    }
                    while (hr == 0x00040237 || hr == 0x00040268);
                    if (hr == 0)
                    {
                        return _state == FilterState.Running;
                    }
                }
                return false;
            }
        }

        public bool IsPaused
        {
            get
            {
                if (m_MediaControl != null)
                {
                    FilterState _state;
                    int hr = 0;
                    do
                    {
                        hr = m_MediaControl.GetState(200, out _state);
                    }
                    while (hr == 0x00040237 || hr == 0x00040268);
                    if (hr == 0)
                    {
                        return _state == FilterState.Paused;
                    }
                }
                return false;
            }
        }

        public bool IsStopped
        {
            get
            {
                if (m_MediaControl != null)
                {
                    FilterState _state;
                    int hr = 0;
                    do
                    {
                        hr = m_MediaControl.GetState(200, out _state);
                    }
                    while (hr == 0x00040237 || hr == 0x00040268);
                    if (hr == 0)
                    {
                        return _state == FilterState.Stopped;
                    }
                }
                return true;
            }
        }

        public double Position
        {
            get
            {
                if (m_MediaPosition != null)
                {
                    int hr = m_MediaPosition.get_CurrentPosition(out double _time);
                    Debug.Assert(hr == 0);
                    if (hr == 0) return _time;
                }
                return -1;
            }
            set
            {
                if (m_MediaSeeking != null)
                {
                    int hr = m_MediaPosition.put_CurrentPosition(value);
                    Debug.Assert(hr >= 0);
                    OnPositionChange?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public double Duration
        {
            get
            {
                if (m_MediaPosition != null)
                {
                    int hr = m_MediaPosition.get_Duration(out double _time);
                    Debug.Assert(hr == 0);
                    if (hr == 0) return _time;
                }
                return -1;
            }
        }

        public int Volume
        {
            get { return m_iVolume; }
            set
            {
                m_iVolume = value;
                if (!m_bMute)
                {
                    SetVolume(value);
                }
            }
        }

        public bool Mute
        {
            get { return m_bMute; }
            set
            {
                if (m_bMute != value)
                {
                    m_bMute = value;
                    SetVolume(m_bMute ? -10000 : m_iVolume);
                }
            }
        }

        public double Rate
        {
            get
            {
                return m_dRate;
            }
            set
            {
                if (value > 0 && value <= 2.0)
                {
                    if (m_MediaSeeking != null)
                    {
                        int hr = m_MediaSeeking.SetRate(value);
                        if (hr == 0)
                        {
                            m_dRate = value;
                        }
                    }
                    else
                    {
                        m_dRate = value;
                    }
                }
            }
        }

        public Control VideoControl
        {
            get { return m_VideoControl; }
            set
            {
                if (m_VideoControl != value)
                {
                    m_VideoControl = value;
                    m_VideoControl.Resize += VideoControl_Resize;
                    m_VideoControl.VisibleChanged += VideoControl_VisibleChanged;
                }
            }
        }

        public bool Visible
        {
            get
            {
                bool bVisible = ((m_VideoControl != null) ? m_VideoControl.Visible : false);
                return m_bShouldPreview || bVisible;
            }
            set
            {
                if (m_bShouldPreview != value)
                {
                    m_bShouldPreview = value;
                    if (m_VideoWindow != null)
                    {
                        m_VideoWindow.put_AutoShow(m_bShouldPreview ? -1 : 0);
                        m_VideoWindow.put_Visible(m_bShouldPreview ? -1 : 0);
                    }
                }
            }
        }

        public List<DSFilter> Filters
        {
            get
            {
                if (m_MediaControl == null)
                {
                    while (m_Filters.Count > 0)
                    {
                        DSFilter _filter = m_Filters[0];
                        m_Filters.RemoveAt(0);
                        _filter.Dispose();
                    }
                    if (m_GraphBuilder != null)
                    {
                        if (SUCCEEDED(m_GraphBuilder.EnumFilters(out IEnumFilters pEnum)))
                        {
                            IBaseFilter[] aFilters = new IBaseFilter[1];
                            while (S_OK == pEnum.Next(1, aFilters, IntPtr.Zero))
                            {
                                m_Filters.Add(new DSFilter(aFilters[0]));
                            }
                            Marshal.ReleaseComObject(pEnum);
                        }
                    }
                }
                return m_Filters;
            }
        }

        public int Count
        {
            get { return Filters.Count; }
        }

        public DSFilter AudioRenderer
        {
            get
            {
                List<DSFilter> _filters = Filters;
                foreach (DSFilter _filter in _filters)
                {
                    if (_filter.IsSupported(typeof(IBasicAudio).GUID))
                    {
                        return _filter;
                    }
                }
                return null;
            }
        }

        public DSFilter VideoRenderer
        {
            get
            {
                List<DSFilter> _filters = Filters;
                foreach (DSFilter _filter in _filters)
                {
                    if (_filter.IsSupported(typeof(IVideoWindow)))
                    {
                        return _filter;
                    }
                }
                return null;
            }
        }

        public DSFilter this[int index]
        {
            get
            {
                try
                {
                    return Filters[index];
                }
                catch
                {
                    return null;
                }
            }
        }

        public DSFilter this[string _name]
        {
            get
            {
                List<DSFilter> _filters = Filters;
                foreach (DSFilter _filter in _filters)
                {
                    if (_filter.Name == _name)
                    {
                        return _filter;
                    }
                }
                return null;
            }
        }

        public bool IsAudioSupported
        {
            get
            {
                return ((AudioRenderer != null) && AudioRenderer == true);
            }
        }

        #endregion

        #region Events

        public event EventHandler OnPlaybackPrepared;
        public event EventHandler OnPlaybackStart;
        public event EventHandler OnPlaybackStop;
        public event EventHandler OnPlaybackPause;
        public event EventHandler OnPositionChange;
        public event EventHandler OnPlaybackReady;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            HRESULT hr = Load();
            if (hr != 0)
            {
                throw new Exception();
            }
        }

        public virtual HRESULT Start()
        {
            var hr = (HRESULT)m_MediaControl.Run();
            if (hr == 0)
            {
                OnPlaybackStart?.Invoke(this, EventArgs.Empty);
            }
            return (HRESULT)hr;
        }

        public virtual HRESULT Pause()
        {
            int hr = 0;
            if (m_MediaControl == null)
            {
                hr = Load();
                if (hr != 0) return (HRESULT)hr;
            }
            if (!IsPaused)
            {
                hr = m_MediaControl.Pause();
            }
            else
            {
                return Start();
            }
            if (hr >= 0)
            {
                OnPlaybackPause?.Invoke(this, EventArgs.Empty);
            }
            return (HRESULT)hr;
        }

        public virtual HRESULT Stop()
        {
            if (m_MediaControl == null) return (HRESULT)E_POINTER;
            m_MediaControl.Stop();
            OnPlaybackStop?.Invoke(this, EventArgs.Empty);
            return Unload();
        }

        public virtual HRESULT StepForward()
        {
            if (m_FrameStep != null)
            {
                if (!IsPaused)
                {
                    Pause();
                }
                int hr = m_FrameStep.Step(1, null);
                if (hr < 0)
                {
                    hr = m_MediaSeeking.GetCurrentPosition(out long _time);
                    DsLong _stop = (long)0;
                    var _ts = new TimeSpan(0, 0, 1);
                    _time += _ts.Ticks / 20;
                    DsLong _current = _time;
                    hr = m_MediaSeeking.SetPositions(_current, AMSeekingSeekingFlags.AbsolutePositioning, _stop, AMSeekingSeekingFlags.NoPositioning);
                }
                return (HRESULT)hr;
            }
            return (HRESULT)E_POINTER;
        }

        public virtual HRESULT StepBackward()
        {
            if (m_FrameStep != null)
            {
                if (!IsPaused)
                {
                    Pause();
                }
                int hr = m_MediaSeeking.GetCurrentPosition(out long _time);
                DsLong _stop = (long)0;
                var _ts = new TimeSpan(0, 0, 1);
                _time -= _ts.Ticks / 20;
                if (_time < 0) _time = 0;
                DsLong _current = _time;
                hr = m_MediaSeeking.SetPositions(_current, AMSeekingSeekingFlags.AbsolutePositioning, _stop, AMSeekingSeekingFlags.NoPositioning);
                return (HRESULT)hr;
            }
            return (HRESULT)E_POINTER;
        }

        #endregion

        #region Private Methods

        private void SetVolume(int _volume)
        {
            if (m_BasicAudio == null)
            {
                if (m_GraphBuilder == null)
                {
                    return;
                }
                else
                {
                    m_BasicAudio = (IBasicAudio)m_GraphBuilder;
                }
            }
            m_BasicAudio.put_Volume(_volume);
        }

        private HRESULT InitInterfaces()
        {
            CloseInterfaces();
            HRESULT hr = E_FAIL;
            try
            {
                m_GraphBuilder = (IGraphBuilder)new FilterGraph();
                hr = OnInitInterfaces();
                hr.Throw();
                return PreparePlayback();
            }
            catch (Exception _exception)
            {
                if (_exception is COMException)
                {
                    hr = (HRESULT)((COMException)_exception).ErrorCode;
                }
                else
                {
                    hr = E_UNEXPECTED;
                }
            }
            finally
            {
                if (hr.Succeeded)
                {
                    while (m_Filters.Count > 0)
                    {
                        DSFilter _filter = m_Filters[0];
                        m_Filters.RemoveAt(0);
                        _filter.Dispose();
                    }
                    if (SUCCEEDED(m_GraphBuilder.EnumFilters(out IEnumFilters pEnum)))
                    {
                        IBaseFilter[] aFilters = new IBaseFilter[1];
                        while (S_OK == pEnum.Next(1, aFilters, IntPtr.Zero))
                        {
                            m_Filters.Add(new DSFilter(aFilters[0]));
                        }
                        Marshal.ReleaseComObject(pEnum);
                    }
                }
            }
            CloseInterfaces();
            return hr;
        }

        private HRESULT CloseInterfaces()
        {
            try
            {
                OnCloseInterfaces();
                if (m_MediaEventEx != null)
                {
                    m_MediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
                    m_MediaEventEx = null;
                }
                if (m_VideoWindow != null)
                {
                    m_VideoWindow.put_Visible(0);
                    m_VideoWindow.put_Owner(IntPtr.Zero);
                    m_VideoWindow = null;
                }
                m_MediaSeeking = null;
                m_MediaPosition = null;
                m_BasicVideo = null;
                m_BasicAudio = null;
                m_MediaControl = null;
                while (m_Filters.Count > 0)
                {
                    DSFilter _filter = m_Filters[0];
                    m_Filters.RemoveAt(0);
                    _filter.Dispose();
                }
                if (m_GraphBuilder != null)
                {
                    Marshal.ReleaseComObject(m_GraphBuilder);
                    m_GraphBuilder = null;
                }
                GC.Collect();
                return (HRESULT)NOERROR;
            }
            catch
            {
                return (HRESULT)E_FAIL;
            }
        }

        #endregion

        #region Protected Methods

        protected virtual HRESULT Load()
        {
            HRESULT hr = InitInterfaces();
            if (hr.Succeeded)
            {
                OnPlaybackPrepared?.Invoke(this, EventArgs.Empty);
            }
            return hr;
        }

        protected virtual HRESULT Unload()
        {
            return CloseInterfaces();
        }

        protected virtual HRESULT OnInitInterfaces()
        {
            return (HRESULT)NOERROR;
        }

        protected virtual HRESULT OnCloseInterfaces()
        {
            return (HRESULT)NOERROR;
        }

        protected virtual void SettingUpVideoWindow()
        {
            if (m_VideoWindow != null)
            {
                if (m_VideoControl != null)
                {
                    m_VideoWindow.put_Owner(m_VideoControl.Handle);
                    m_VideoWindow.put_MessageDrain(m_VideoControl.Handle);
                    m_VideoWindow.put_WindowStyle(0x40000000 | 0x04000000);
                    ResizeVideoWindow();
                }
                m_VideoWindow.put_AutoShow(Visible ? -1 : 0);
                m_VideoWindow.put_Visible(Visible ? -1 : 0);
            }
        }

        protected virtual HRESULT PreparePlayback()
        {
            m_MediaControl = (IMediaControl)m_GraphBuilder;
            m_BasicVideo = (IBasicVideo)m_GraphBuilder;
            m_MediaSeeking = (IMediaSeeking)m_GraphBuilder;
            m_VideoWindow = (IVideoWindow)m_GraphBuilder;
            m_MediaEventEx = (IMediaEventEx)m_GraphBuilder;
            m_FrameStep = (IVideoFrameStep)m_GraphBuilder;
            m_MediaPosition = (IMediaPosition)m_GraphBuilder;
            SettingUpVideoWindow();
            int hr = m_MediaEventEx.SetNotifyWindow(m_EventControl.Handle, WM_GRAPHNOTIFY, Marshal.GetIUnknownForObject(this));
            Debug.Assert(hr == 0);
            SetVolume(m_bMute ? -10000 : m_iVolume);
            if (m_dRate != 1.0)
            {
                m_MediaSeeking.SetRate(m_dRate);
                m_MediaSeeking.GetRate(out m_dRate);
            }
            OnPlaybackReady?.Invoke(this, EventArgs.Empty);
            return (HRESULT)hr;
        }

        protected virtual bool ProcessGraphMessage()
        {
            if (m_MediaEventEx != null)
            {
                int hr = 0;
                while (hr == 0)
                {
                    hr = m_MediaEventEx.GetEvent(out DsEvCode _code, out int _param1, out int _param2, 20);
                    if (hr == 0)
                    {
                        hr = m_MediaEventEx.FreeEventParams(_code, _param1, _param2);

                        if (_code == DsEvCode.Complete)
                        {
                            return true;
                        }
                        if (_code == DsEvCode.DeviceLost)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected virtual void ResizeVideoWindow()
        {
            if (m_VideoWindow != null && m_VideoControl != null)
            {
                m_VideoWindow.SetWindowPosition(0, 0, m_VideoControl.Width, m_VideoControl.Height);
            }
        }

        #endregion

        #region Overridden Methods

        private void VideoControl_VisibleChanged(object sender, EventArgs e)
        {
            if (m_VideoWindow != null && m_VideoControl != null && m_bShouldPreview)
            {
                m_VideoWindow.put_AutoShow(m_VideoControl.Visible ? -1 : 0);
                m_VideoWindow.put_Visible(m_VideoControl.Visible ? -1 : 0);
            }
        }

        private void VideoControl_Resize(object sender, EventArgs e)
        {
            ResizeVideoWindow();
        }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            CloseInterfaces();
        }

        #endregion
    }

    public interface ISourceFileSupport
    {
        string FileName { get; set; }
        HRESULT Open();
        HRESULT Open(bool bStart);
        HRESULT Open(string sFileName);
        HRESULT Open(string sFileName, bool bStart);
    }

    public interface IFileDestSupport
    {
        string OutputFileName { get; set; }
        HRESULT Save();
        HRESULT Save(bool bStart);
        HRESULT Save(string sFileName);
        HRESULT Save(string sFileName, bool bStart);
    }

    public class DSFilePlayback : DSFilterGraphBase, ISourceFileSupport
    {
        #region Variables

        protected string m_sFileName = "";

        #endregion

        #region Properties

        public string FileName
        {
            get { return m_sFileName; }
            set { Open(value); }
        }

        #endregion

        #region Public Methods

        public HRESULT Open()
        {
            return Open(true);
        }

        public HRESULT Open(bool bStart)
        {
            return Open(m_sFileName, bStart);
        }

        public HRESULT Open(string sFileName)
        {
            return Open(sFileName, false);
        }

        public HRESULT Open(string sFileName, bool bStart)
        {
            if (sFileName != null && sFileName != "")
            {
                m_sFileName = sFileName;
                if (bStart)
                {
                    return Start();
                }
                else
                {
                    return Load();
                }
            }
            return E_POINTER;
        }

        #endregion

        #region Overridden Methods

        protected override HRESULT OnInitInterfaces()
        {
            int hr = m_GraphBuilder.RenderFile(m_sFileName, null);
            if (hr < 0) Marshal.ThrowExceptionForHR(hr);
            return (HRESULT)hr;
        }

        #endregion
    }

}