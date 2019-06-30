using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Permissions;

namespace DirectShow
{
    #region Basic COM helper utilites classes

    public class COMHelper
    {
        #region BOOL

        public static BOOL TRUE { get { return (BOOL)1; } }
        public static BOOL FALSE { get { return (BOOL)0; } }

        #endregion

        #region FOURCC

        public static FOURCC MAKEFOURCC(char a1, char a2, char a3, char a4) { return new FOURCC(a1, a2, a3, a4); }
        public static FOURCC MAKEFOURCC(byte a1, byte a2, byte a3, byte a4) { return new FOURCC(a1, a2, a3, a4); }

        public static readonly FOURCC BI_RGB = (FOURCC)0;

        #endregion

        #region Helper Methods and constants

        public const int WM_NULL = 0x0000;
        public const int TIME_ONESHOT = 0x0000;
        public const int TIME_PERIODIC = 0x0001;
        public const object NULL = null;

        public static void _TRACE(string _message) { if (!string.IsNullOrEmpty(_message)) Trace.WriteLine(_message); }
        public static void TRACE(string _message) { if (!string.IsNullOrEmpty(_message)) _message += "\n"; API.OutputDebugString(_message); }

        public static void TRACE_ENTER()
        {
            MethodBase _method = (new StackTrace(1, false)).GetFrame(0).GetMethod();
            TRACE(string.Format("{0}::{1}", _method.ReflectedType.Name, _method.Name));
        }
        public static bool SUCCEEDED(int hr) { return (hr >= 0); }
        public static bool FAILED(int hr) { return (hr < 0); }
        public static void ASSERT(object _object) { if (_object is BOOL) Debug.Assert((bool)_object); if (_object is HRESULT) Debug.Assert((bool)_object); else if (_object is bool) Debug.Assert((bool)_object); else Debug.Assert(_object != null); }
        public static int HIWORD(int _value) { return ((_value >> 16) & 0xffff); }
        public static int LOWORD(int _value) { return (_value & 0xffff); }
        public const long MAX_LONG = 0x7FFFFFFFFFFFFFFF;

        public const long MILLISECONDS = (1000);            // 10 ^ 3
        public const long NANOSECONDS = (1000000000);       // 10 ^ 9
        public const long UNITS = (NANOSECONDS / 100);      // 10 ^ 7

        #region Align

        public static int ALIGN16(int SZ) { unchecked { return (((SZ + 15) >> 4) << 4); } } // round up to a multiple of 16
        public static int ALIGN32(int SZ) { unchecked { return (((SZ + 31) >> 5) << 5); } } // round up to a multiple of 32
        public static uint ALIGN16(uint SZ) { unchecked { return (((SZ + 15) >> 4) << 4); } } // round up to a multiple of 16
        public static uint ALIGN32(uint SZ) { unchecked { return (((SZ + 31) >> 5) << 5); } } // round up to a multiple of 32
        public static long ALIGN16(long SZ) { unchecked { return (((SZ + 15) >> 4) << 4); } } // round up to a multiple of 16
        public static long ALIGN32(long SZ) { unchecked { return (((SZ + 31) >> 5) << 5); } } // round up to a multiple of 32
        public static ulong ALIGN16(ulong SZ) { unchecked { return (((SZ + 15) >> 4) << 4); } } // round up to a multiple of 16
        public static ulong ALIGN32(ulong SZ) { unchecked { return (((SZ + 31) >> 5) << 5); } } // round up to a multiple of 32

        #endregion

        #endregion

        #region HRESULT

        public static HRESULT S_OK { get { return (HRESULT)0; } }
        public static HRESULT S_FALSE { get { return (HRESULT)1; } }
        public static HRESULT NOERROR { get { return S_OK; } }
        public static HRESULT E_INVALIDARG { get { unchecked { return (HRESULT)0x80070057; } } }
        public static HRESULT E_NOINTERFACE { get { unchecked { return (HRESULT)0x80004002; } } }
        public static HRESULT E_NOTIMPL { get { unchecked { return (HRESULT)0x80004001; } } }
        public static HRESULT E_UNEXPECTED { get { unchecked { return (HRESULT)0x8000FFFF; } } }
        public static HRESULT E_FAIL { get { unchecked { return (HRESULT)0x80004005; } } }
        public static HRESULT E_POINTER { get { unchecked { return (HRESULT)0x80004003; } } }
        public static HRESULT E_OUTOFMEMORY { get { unchecked { return (HRESULT)0x8007000E; } } }
        public static HRESULT E_FILE_NOT_FOUND { get { unchecked { return (HRESULT)0x80070002; } } }
        public static HRESULT VFW_E_NOT_FOUND { get { unchecked { return (HRESULT)0x80040216; } } }
        public static HRESULT VFW_E_CANNOT_CONNECT { get { unchecked { return (HRESULT)0x80040217; } } }
        public static HRESULT VFW_E_NOT_CONNECTED { get { unchecked { return (HRESULT)0x80040209; } } }
        public static HRESULT VFW_E_TYPE_NOT_ACCEPTED { get { unchecked { return (HRESULT)0x8004022A; } } }
        public static HRESULT VFW_E_NO_ACCEPTABLE_TYPES { get { unchecked { return (HRESULT)0x80040207; } } }
        public static HRESULT VFW_E_INVALID_DIRECTION { get { unchecked { return (HRESULT)0x80040208; } } }
        public static HRESULT VFW_E_ALREADY_CONNECTED { get { unchecked { return (HRESULT)0x80040204; } } }
        public static HRESULT VFW_E_NOT_STOPPED { get { unchecked { return (HRESULT)0x80040224; } } }
        public static HRESULT VFW_E_NO_ALLOCATOR { get { unchecked { return (HRESULT)0x8004020A; } } }
        public static HRESULT VFW_S_NO_MORE_ITEMS { get { unchecked { return (HRESULT)0x00040103; } } }
        public static HRESULT VFW_E_INVALIDMEDIATYPE { get { unchecked { return (HRESULT)0x80040200; } } }
        public static HRESULT VFW_E_RUNTIME_ERROR { get { unchecked { return (HRESULT)0x8004020B; } } }
        public static HRESULT VFW_E_WRONG_STATE { get { unchecked { return (HRESULT)0x80040227; } } }
        public static HRESULT VFW_E_ENUM_OUT_OF_SYNC { get { unchecked { return (HRESULT)0x80040203; } } }
        public static HRESULT VFW_E_NO_CLOCK { get { unchecked { return (HRESULT)0x80040213; } } }
        public static HRESULT VFW_E_NOT_IN_GRAPH { get { unchecked { return (HRESULT)0x8004025F; } } }
        public static HRESULT VFW_E_START_TIME_AFTER_END { get { unchecked { return (HRESULT)0x80040228; } } }
        public static HRESULT VFW_S_STATE_INTERMEDIATE { get { unchecked { return (HRESULT)0x00040237; } } }
        public static HRESULT VFW_E_SAMPLE_REJECTED { get { unchecked { return (HRESULT)0x8004022B; } } }
        public static HRESULT VFW_E_STATE_CHANGED { get { unchecked { return (HRESULT)0x80040223; } } }
        public static HRESULT VFW_E_SAMPLE_TIME_NOT_SET { get { unchecked { return (HRESULT)0x80040249; } } }
        public static HRESULT VFW_E_TIMEOUT { get { unchecked { return (HRESULT)0x8004022E; } } }

        #endregion

        #region API

        public static class API
        {
            [DllImport("kernel32.dll", EntryPoint = "OutputDebugStringW", CharSet = CharSet.Unicode)]
            public static extern void OutputDebugString(string _text);

            [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
            public static extern void CopyMemory(IntPtr Destination, IntPtr Source, [MarshalAs(UnmanagedType.U4)] int Length);

            [DllImport("ole32.dll")]
            public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

            [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
            public static extern int MkParseDisplayName(IBindCtx pbc, string szUserName, ref int pchEaten, out IMoniker ppmk);

            [DllImport("olepro32.dll")]
            public static extern int OleCreatePropertyFrame(
                IntPtr hwndOwner,
                int x,
                int y,
                [MarshalAs(UnmanagedType.LPWStr)] string lpszCaption,
                int cObjects,
                [MarshalAs(UnmanagedType.Interface, ArraySubType = UnmanagedType.IUnknown)]
                ref object ppUnk,
                int cPages,
                IntPtr lpPageClsID,
                int lcid,
                int dwReserved,
                IntPtr lpvReserved
                );
        }

        #endregion
    }

    public sealed class BOOL : COMHelper, IComparable, ICloneable
    {
        #region Variables

        private bool m_Result = false;

        #endregion

        #region Constructor

        public BOOL()
        {
        }

        public BOOL(int _value)
        {
            m_Result = _value == 0 ? false : true;
        }

        public BOOL(bool _value)
        {
            m_Result = _value;
        }

        public BOOL(BOOL _value)
            : this(_value.m_Result)
        {
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is BOOL)
            {
                return m_Result.CompareTo(((BOOL)obj).m_Result);
            }
            return m_Result.CompareTo(obj);
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new BOOL(m_Result);
        }

        #endregion

        #region Overridden Methods

        public override bool Equals(object obj)
        {
            if (obj is BOOL)
            {
                return m_Result.Equals(((BOOL)obj).m_Result);
            }
            return m_Result.Equals(obj);
        }

        public override int GetHashCode()
        {
            return m_Result.GetHashCode();
        }

        public override string ToString()
        {
            return m_Result.ToString();
        }

        #endregion

        #region Methods

        public void Assert()
        {
            ASSERT(m_Result);
        }

        #endregion

        #region Static Methods

        public static new BOOL TRUE
        {
            get { return COMHelper.TRUE; }
        }

        public static new BOOL FALSE
        {
            get { return COMHelper.FALSE; }
        }

        #endregion

        #region Operators

        public static bool operator ==(BOOL _src, BOOL _dest)
        {
            if (System.Object.ReferenceEquals(_src, _dest))
            {
                return true;
            }
            if ((_src as Object) == (_dest as Object)) return true;

            if ((_src as Object) != null && (_dest as Object) != null)
            {
                return _src.m_Result == _dest.m_Result;
            }
            return false;
        }

        public static bool operator !=(BOOL _src, BOOL _dest)
        {
            return !(_src == _dest);
        }

        public static bool operator ==(BOOL _src, HRESULT _dest)
        {
            return _src == (BOOL)_dest;

        }

        public static bool operator !=(BOOL _src, HRESULT _dest)
        {
            return !(_src == _dest);
        }

        public static implicit operator bool(BOOL _bool)
        {
            if ((_bool as Object) == null) return false;
            return _bool.m_Result;
        }

        public static implicit operator int(BOOL _bool)
        {
            if ((_bool as Object) == null) return 0;
            return _bool.m_Result ? 1 : 0;
        }

        public static implicit operator HRESULT(BOOL _bool)
        {
            if ((_bool as Object) == null) return S_FALSE;
            return _bool.m_Result ? S_OK : S_FALSE;
        }

        public static explicit operator BOOL(HRESULT hr)
        {
            if ((hr as Object) == null) return FALSE;
            return new BOOL(hr == S_OK);
        }

        public static explicit operator BOOL(int _value)
        {
            return new BOOL(_value != 0);
        }

        public static explicit operator BOOL(bool _bool)
        {
            return new BOOL(_bool);
        }

        #endregion
    }

    public class HRESULT : COMHelper, IComparable, ICloneable
    {
        #region Variables

        private int m_Result = 0;

        #endregion

        #region Constructor

        public HRESULT()
        {
        }

        public HRESULT(int hr)
        {
            m_Result = hr;
        }

        public HRESULT(HRESULT hr)
            : this(hr.m_Result)
        {
        }

        #endregion

        #region Properties

        public bool Failed
        {
            get { return FAILED(m_Result); }
        }

        public bool Succeeded
        {
            get { return SUCCEEDED(m_Result); }
        }

        public string Text
        {
            get { return GetErrorText(); }
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is HRESULT)
            {
                return m_Result.CompareTo(((HRESULT)obj).m_Result);
            }
            return m_Result.CompareTo(obj);
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new HRESULT(m_Result);
        }

        #endregion

        #region Overridden Methods

        public override bool Equals(object obj)
        {
            if (obj is HRESULT)
            {
                return m_Result.Equals(((HRESULT)obj).m_Result);
            }
            return m_Result.Equals(obj);
        }

        public override int GetHashCode()
        {
            return m_Result.GetHashCode();
        }

        public override string ToString()
        {
            string _text = GetErrorText();
            if (_text != "")
            {
                _text = " ( " + _text + " )";
            }
            _text = "0x" + m_Result.ToString("x8") + _text;
            return _text;
        }

        #endregion

        #region Virtual Methods

        protected virtual string GetErrorText()
        {
            string _text = "";
            uint _length = 160;
            IntPtr _ptr = Marshal.AllocCoTaskMem((int)_length);
            try
            {
                if (AMGetErrorText(m_Result, _ptr, _length) > 0)
                {
                    _text = Marshal.PtrToStringAuto(_ptr);
                }
            }
            catch
            {
            }
            Marshal.FreeCoTaskMem(_ptr);
            return _text;
        }

        #endregion

        #region Methods

        public void Assert()
        {
            ASSERT(SUCCEEDED(m_Result));
        }

        public void Throw()
        {
            if (FAILED(m_Result))
            {
                Marshal.ThrowExceptionForHR(m_Result);
            }
        }

        public void TraceWrite()
        {
            if (Failed)
            {
                StackFrame _frame = (new StackTrace(1, true)).GetFrame(0);
                MethodBase _method = _frame.GetMethod();
                string _file = _frame.GetFileName();
                _file = !string.IsNullOrEmpty(_file) ? Path.GetFileName(_file) : "";
                TRACE(string.Format("Method {0}::{1}, HRESULT : {2},File : {3} Line : {4} ",
                    _method.ReflectedType.Name,
                    _method.Name,
                    ToString(),
                    _file,
                    _frame.GetFileLineNumber()
                    ));
            }
        }

        #endregion

        #region Imports

        [DllImport("quartz.dll", EntryPoint = "AMGetErrorText", CharSet = CharSet.Auto)]
        private static extern uint AMGetErrorText(int hr, IntPtr sText, [In] uint _length);

        #endregion

        #region Operators

        public static bool operator ==(HRESULT _src, HRESULT _dest)
        {
            if (System.Object.ReferenceEquals(_src, _dest))
            {
                return true;
            }
            if ((_src as Object) == (_dest as Object)) return true;

            if ((_src as Object) != null && (_dest as Object) != null)
            {
                return _src.m_Result == _dest.m_Result;
            }
            return false;
        }

        public static bool operator !=(HRESULT _src, HRESULT _dest)
        {
            return !(_src == _dest);
        }

        public static implicit operator bool(HRESULT hr)
        {
            if ((hr as Object) == null) return true;
            return (SUCCEEDED(hr.m_Result));
        }

        public static implicit operator int(HRESULT hr)
        {
            if ((hr as Object) == null) return 0;
            return hr.m_Result;
        }

        public static explicit operator HRESULT(int hr)
        {
            return new HRESULT(hr);
        }

        public static explicit operator HRESULT(uint hr)
        {
            unchecked
            {
                return new HRESULT((int)hr);
            }
        }

        #endregion
    }

    public sealed class FOURCC : COMHelper, IComparable, ICloneable
    {
        #region Constants

        private const ushort m_usB = 0x0000;
        private const ushort m_usC = 0x0010;
        private static readonly byte[] m_abtD = { 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71 };

        #endregion

        #region Variables

        private uint m_uiFourcc = 0;

        #endregion

        #region Constructors

        public FOURCC(int _value)
        {
            unchecked
            {
                m_uiFourcc = (uint)_value;
            }
        }

        public FOURCC(uint _value)
        {
            m_uiFourcc = _value;
        }

        public FOURCC(Guid _guid)
        {
            byte[] _data = _guid.ToByteArray();
            byte[] _result = { _data[0], _data[1], _data[2], _data[3] };
            m_uiFourcc = BitConverter.ToUInt32(_result, 0);
        }

        public FOURCC(string _value)
        {
            byte[] _data = new byte[4];
            for (int i = 0; i < _data.Length; i++)
            {
                if (i < _value.Length)
                {
                    _data[i] = (byte)_value[i];
                }
                else
                {
                    _data[i] = 0;
                }
            }
            m_uiFourcc = BitConverter.ToUInt32(_data, 0);
        }

        public FOURCC(byte[] _value)
        {
            byte[] _data = new byte[4];
            for (int i = 0; i < _data.Length; i++)
            {
                if (i < _value.Length)
                {
                    _data[i] = _value[i];
                }
                else
                {
                    _data[i] = 0;
                }
            }
            m_uiFourcc = BitConverter.ToUInt32(_data, 0);
        }

        public FOURCC(char[] _value)
        {
            byte[] _data = new byte[4];
            for (int i = 0; i < _data.Length; i++)
            {
                if (i < _value.Length)
                {
                    _data[i] = (byte)_value[i];
                }
                else
                {
                    _data[i] = 0;
                }
            }
            m_uiFourcc = BitConverter.ToUInt32(_data, 0);
        }

        public FOURCC(char a1, char a2, char a3, char a4)
        {
            var _data = new byte[] { (byte)a1, (byte)a2, (byte)a3, (byte)a4 };
            m_uiFourcc = BitConverter.ToUInt32(_data, 0);
        }

        public FOURCC(byte a1, byte a2, byte a3, byte a4)
        {
            var _data = new byte[] { a1, a2, a3, a4 };
            m_uiFourcc = BitConverter.ToUInt32(_data, 0);
        }

        #endregion

        #region Overridden Methods

        public override bool Equals(object obj)
        {
            if (obj is FOURCC)
            {
                return m_uiFourcc.Equals(((FOURCC)obj).m_uiFourcc);
            }
            return m_uiFourcc.Equals(obj);
        }

        public override int GetHashCode()
        {
            return m_uiFourcc.GetHashCode();
        }

        public override string ToString()
        {
            byte[] _data = BitConverter.GetBytes(m_uiFourcc);
            string _result = " ( ";
            for (int i = 0; i < _data.Length; i++)
            {
                if (_data[i] <= 30 || _data[i] > 128)
                {
                    _result = "";
                    break;
                }
                else
                {
                    _result += (char)_data[i];
                }
            }
            if (_result != "")
            {
                _result += " )";
            }
            return m_uiFourcc.ToString("x4") + _result;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is FOURCC)
            {
                return m_uiFourcc.CompareTo(((FOURCC)obj).m_uiFourcc);
            }
            return m_uiFourcc.CompareTo(obj);
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new FOURCC(m_uiFourcc);
        }

        #endregion

        #region Operators

        public static bool operator ==(FOURCC _src, FOURCC _dest)
        {
            if (System.Object.ReferenceEquals(_src, _dest))
            {
                return true;
            }
            if ((_src as Object) == (_dest as Object)) return true;

            if ((_src as Object) != null && (_dest as Object) != null)
            {
                return _src.m_uiFourcc == _dest.m_uiFourcc;
            }
            return false;
        }

        public static bool operator !=(FOURCC _src, FOURCC _dest)
        {
            return !(_src == _dest);
        }

        public static explicit operator FOURCC(int _value)
        {
            return new FOURCC(_value);
        }

        public static explicit operator FOURCC(uint _value)
        {
            return new FOURCC(_value);
        }

        public static explicit operator FOURCC(Guid _value)
        {
            return new FOURCC(_value);
        }

        public static explicit operator FOURCC(string _value)
        {
            if (_value == null) return new FOURCC(0);
            return new FOURCC(_value);
        }

        public static explicit operator FOURCC(Array _value)
        {
            if (_value == null) return new FOURCC(0);
            byte[] _data = new byte[4];
            for (int i = 0; i < _data.Length; i++)
            {
                if (i < _value.Length)
                {
                    _data[i] = (byte)Convert.ToByte(_value.GetValue(i));
                }
                else
                {
                    _data[i] = 0;
                }
            }
            return new FOURCC(_data);
        }

        public static implicit operator int(FOURCC _fcc)
        {
            if ((_fcc as Object) == null) return 0;
            unchecked
            {
                return (int)_fcc.m_uiFourcc;
            }
        }

        public static implicit operator uint(FOURCC _fcc)
        {
            if ((_fcc as Object) == null) return 0;
            return _fcc.m_uiFourcc;
        }

        public static implicit operator byte[] (FOURCC _fcc)
        {
            if ((_fcc as Object) == null) return null;
            byte[] _data = BitConverter.GetBytes(_fcc.m_uiFourcc);
            byte[] _result = new byte[_data.Length];
            for (int i = 0; i < _data.Length; i++)
            {
                _result[3 - i] = _data[i];
            }
            return _result;
        }

        public static implicit operator char[] (FOURCC _fcc)
        {
            if ((_fcc as Object) == null) return null;
            byte[] _data = BitConverter.GetBytes(_fcc.m_uiFourcc);
            char[] _result = new char[_data.Length];
            for (int i = 0; i < _data.Length; i++)
            {
                _result[3 - i] = (char)_data[i];
            }
            return _result;
        }

        public static implicit operator string(FOURCC _fcc)
        {
            if ((_fcc as Object) == null) return "";
            byte[] _data = BitConverter.GetBytes(_fcc.m_uiFourcc);
            string _result = "";
            for (int i = 0; i < _data.Length; i++)
            {
                _result += (char)_data[i];
            }
            return _result;
        }

        public static implicit operator Guid(FOURCC _fcc)
        {
            if ((_fcc as Object) == null) return Guid.Empty;
            return new Guid(_fcc.m_uiFourcc, m_usB, m_usC, m_abtD[0], m_abtD[1], m_abtD[2], m_abtD[3], m_abtD[4], m_abtD[5], m_abtD[6], m_abtD[7]);
        }

        #endregion
    }

    #endregion

    #region Base COM object

    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public class DSObject<T> : COMHelper, IDisposable where T : class
    {
        #region Variables

        protected T m_pUnknown = null;
        protected bool m_bReleaseOnDestroy = false;

        #endregion

        #region Properties

        public T Value
        {
            get { return m_pUnknown; }
            set { m_pUnknown = value; }
        }

        public bool IsReleased
        {
            get { return m_pUnknown == null; }
        }

        public bool ReleaseOnDestroy
        {
            get { return m_bReleaseOnDestroy; }
            set { m_bReleaseOnDestroy = value; }
        }

        public Guid GUID
        {
            get { return typeof(T).GUID; }
        }

        public object this[Type _type]
        {
            get { return QueryInterface(_type); }
        }

        #endregion

        #region Constructor

        public DSObject()
        {
            Guid _guid = this.GetType().GUID;
            if (_guid != Guid.Empty)
            {
                try
                {
                    m_pUnknown = Activator.CreateInstance(Type.GetTypeFromCLSID(_guid)) as T;
                    GC.SuppressFinalize(m_pUnknown);
                    m_bReleaseOnDestroy = true;
                }
                catch
                {
                }
            }
        }

        public DSObject(Guid _guid)
        {
            if (_guid != Guid.Empty)
            {
                try
                {
                    m_pUnknown = Activator.CreateInstance(Type.GetTypeFromCLSID(_guid)) as T;
                    GC.SuppressFinalize(m_pUnknown);
                    m_bReleaseOnDestroy = true;
                }
                catch
                {
                }
            }
        }

        protected DSObject(IntPtr _ptr)
        {
            if (_ptr != IntPtr.Zero)
            {
                m_pUnknown = (T)Marshal.GetObjectForIUnknown(_ptr);
                GC.SuppressFinalize(m_pUnknown);
                m_bReleaseOnDestroy = true;
            }
        }

        protected DSObject(T _unknown)
        {
            m_pUnknown = _unknown;
            GC.SuppressFinalize(m_pUnknown);
            m_bReleaseOnDestroy = true;
        }

        protected DSObject(T _unknown, bool bReleaseOnDestroy)
            : this(_unknown)
        {
            m_bReleaseOnDestroy = bReleaseOnDestroy;
        }

        ~DSObject()
        {
            Dispose();
        }

        #endregion

        #region Overriden Methods

        public override bool Equals(object obj)
        {
            if (obj == m_pUnknown)
            {
                return true;
            }
            if (obj is DSObject<T>)
            {
                return (obj as DSObject<T>).m_pUnknown == m_pUnknown;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (m_pUnknown != null) return m_pUnknown.GetHashCode();
            return base.GetHashCode();
        }

        /*
        public virtual new Type GetType()
        {
            Type _type new _co
            //return base.GetType();
            Type
        }
        */

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            if (m_bReleaseOnDestroy && m_pUnknown != null)
            {
                if (Marshal.IsComObject(m_pUnknown))
                {
                    Marshal.ReleaseComObject(m_pUnknown);
                }
                GC.ReRegisterForFinalize(m_pUnknown);
                m_pUnknown = null;
            }
        }

        #endregion

        #region Public Methods

        public object QueryInterface(Guid _guid)
        {
            object _object = null;
            if (m_pUnknown != null)
            {
                IntPtr _this = Marshal.GetIUnknownForObject(m_pUnknown);
                IntPtr _interface = IntPtr.Zero;
                try
                {
                    int hr = Marshal.QueryInterface(_this, ref _guid, out _interface);

                    if (SUCCEEDED(hr) && _interface != IntPtr.Zero)
                    {
                        _object = Marshal.GetObjectForIUnknown(_interface);
                    }
                }
                finally
                {
                    Marshal.Release(_this);
                    if (_interface != IntPtr.Zero) Marshal.Release(_interface);
                }
            }
            return _object;
        }

        public object QueryInterface(Type _type)
        {
            object _object = null;
            if (m_pUnknown != null)
            {
                IntPtr _this = Marshal.GetIUnknownForObject(m_pUnknown);
                try
                {
                    _object = Marshal.GetTypedObjectForIUnknown(_this, _type);
                }
                catch
                {
                }
                finally
                {
                    Marshal.Release(_this);
                }
            }
            return _object;
        }

        public bool IsSupported(Guid _guid)
        {
            if (m_pUnknown != null)
            {
                IntPtr _this = Marshal.GetIUnknownForObject(m_pUnknown);
                IntPtr _interface = IntPtr.Zero;
                try
                {
                    return SUCCEEDED(Marshal.QueryInterface(_this, ref _guid, out _interface));
                }
                finally
                {
                    Marshal.Release(_this);
                    if (_interface != IntPtr.Zero)
                    {
                        Marshal.Release(_interface);
                    }
                }
            }
            return false;
        }

        public bool IsSupported(Type _type)
        {
            Guid _guid = _type.GUID;
            if (_guid != Guid.Empty)
            {
                return IsSupported(_guid);
            }
            return false;
        }

        #endregion

        #region Operators

        public static bool operator ==(DSObject<T> _src, DSObject<T> _dest)
        {
            if (System.Object.ReferenceEquals(_src, _dest))
            {
                return true;
            }
            if ((_src as Object) == (_dest as Object)) return true;

            if (((_src as Object) == null || _src.m_pUnknown == null)
                && ((_dest as Object) == null || _dest.m_pUnknown == null))
            {
                return true;
            }
            if ((_src as Object) != null && (_dest as Object) != null)
            {
                return _src.m_pUnknown == _dest.m_pUnknown;
            }
            return false;
        }

        public static bool operator !=(DSObject<T> _src, DSObject<T> _dest)
        {
            return !(_src == _dest);
        }

        public static implicit operator T(DSObject<T> _object)
        {
            if (((object)_object) == null)
            {
                return null;
            }
            return _object.m_pUnknown;
        }

        public static implicit operator IntPtr(DSObject<T> _object)
        {
            if (((_object as Object) == null) || _object.m_pUnknown == null)
            {
                return IntPtr.Zero;
            }
            return Marshal.GetIUnknownForObject(_object.m_pUnknown);
        }

        public static implicit operator bool(DSObject<T> _object)
        {
            return (((object)_object) != null && _object.m_pUnknown != null);
        }

        public static explicit operator DSObject<T>(IntPtr _ptr)
        {
            return new DSObject<T>(_ptr);
        }

        public static explicit operator DSObject<T>(T _object)
        {
            return new DSObject<T>(_object);
        }

        #endregion
    }

    #endregion

    #region Base Moniker Enumerator object

    public class MonikerInfo : DSObject<IMoniker>
    {
        #region Fields

        public Guid ClassID
        {
            get
            {
                Guid _guid = Guid.Empty;
                if (m_pUnknown != null)
                {
                    m_pUnknown.GetClassID(out _guid);
                }
                return _guid;

            }
        }

        public string Name
        {
            get { return GetPropBagValue("FriendlyName"); }
        }

        public string DevicePath
        {
            get
            {
                string _out = "";
                if (m_pUnknown != null)
                {
                    m_pUnknown.GetDisplayName(null, null, out _out);
                }
                return _out;
            }
        }

        public string this[string _value]
        {
            get { return GetPropBagValue(_value); }
        }

        #endregion

        #region Constructor

        public MonikerInfo()
            : base(IntPtr.Zero)
        {

        }

        public MonikerInfo(IMoniker _moniker)
            : base(_moniker)
        {

        }

        public MonikerInfo(string _moniker)
        {
            IBindCtx _context = null;
            try
            {
                if (SUCCEEDED(API.CreateBindCtx(0, out _context)))
                {
                    int n = 0;
                    ASSERT(SUCCEEDED(API.MkParseDisplayName(_context, _moniker, ref n, out m_pUnknown)));
                }
            }
            finally
            {
                if (_context != null)
                {
                    Marshal.ReleaseComObject(_context);
                }
            }
        }

        #endregion

        #region Public Methods

        public object Bind(Type _type)
        {
            object _object = null;
            if (m_pUnknown != null)
            {
                Guid _guid = _type.GUID;
                if (_guid != Guid.Empty)
                {
                    try
                    {
                        m_pUnknown.BindToObject(null, null, ref _guid, out _object);
                    }
                    catch
                    {
                        _object = null;
                    }
                }
            }
            return _object;
        }

        #endregion

        #region Protected Methods

        protected string GetPropBagValue(string sPropName)
        {
            if (m_pUnknown == null) return null;
            IPropertyBag _property = null;
            try
            {
                Guid _guid = typeof(IPropertyBag).GUID;
                m_pUnknown.BindToStorage(null, null, ref _guid, out object _object);
                _property = (IPropertyBag)_object;
                int hr = _property.Read(sPropName, out object _value, null);
                ASSERT(hr == S_OK);
                if (SUCCEEDED(hr))
                {
                    return _value as string;
                }
            }
            catch
            {
            }
            finally
            {
                if (_property != null)
                {
                    Marshal.ReleaseComObject(_property);
                    _property = null;
                }
            }
            return null;
        }

        #endregion
    }

    public class DSEnumerator<T> : DSObject<ICreateDevEnum>, IEnumerable<T> where T : MonikerInfo, new()
    {
        #region Variabless

        protected List<T> m_Objects = new List<T>();
        protected IEnumMoniker m_pEnumMoniker = null;
        protected Guid m_Category = Guid.Empty;

        #endregion

        #region Properties

        public Guid Category
        {
            get { return m_Category; }
        }

        public T this[int index]
        {
            get { return m_Objects[index]; }
        }

        public T this[string name]
        {
            get
            {
                foreach (T _info in m_Objects)
                {
                    if (_info.Name == name || _info.DevicePath == name)
                    {
                        return _info;
                    }
                }
                return null;
            }
        }

        public T this[Guid _clsid]
        {
            get
            {
                foreach (T _info in m_Objects)
                {
                    if (_info.ClassID == _clsid)
                    {
                        return _info;
                    }
                }
                return null;
            }
        }

        public int Count
        {
            get { return m_Objects.Count; }
        }

        public List<T> Objects
        {
            get { return m_Objects; }
        }

        #endregion

        #region Constructor

        public DSEnumerator(Guid _category)
            : base(new Guid("62BE5D10-60EB-11d0-BD3B-00A0C911CE86"))
        {
            int hr = m_pUnknown.CreateClassEnumerator(ref _category, out m_pEnumMoniker, 0);
            ASSERT(hr == S_OK);
            if (SUCCEEDED(hr))
            {
                if (hr != S_FALSE)
                {
                    IMoniker[] aMonikers = new IMoniker[1];
                    while ((m_pEnumMoniker.Next(1, aMonikers, IntPtr.Zero) == 0))
                    {
                        var _moniker = new T
                        {
                            Value = aMonikers[0]
                        };
                        m_Objects.Add(_moniker);
                    }
                }
            }
        }

        #endregion

        #region IDisposable Members

        public override void Dispose()
        {
            if (m_pEnumMoniker != null)
            {
                Marshal.ReleaseComObject(m_pEnumMoniker);
                m_pEnumMoniker = null;
            }
            while (m_Objects.Count > 0)
            {
                T _moniker = m_Objects[0];
                _moniker.Dispose();
                m_Objects.RemoveAt(0);
            }

            base.Dispose();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return m_Objects.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_Objects.GetEnumerator();
        }

        #endregion
    }

    #endregion

    #region Device Categories

    public class DSDevice : MonikerInfo
    {
        #region Fields

        public DSFilter Filter
        {
            get
            {
                DSFilter _filter = null;
                if (m_pUnknown != null)
                {
                    _filter = new DSFilter(this.DevicePath);
                    if (_filter.IsReleased)
                    {
                        _filter.Dispose();
                        _filter = null;
                    }
                }
                return _filter;
            }
        }

        #endregion

        #region Overridden Methods

        public override string ToString()
        {
            string _value = this.Name;
            if (_value != null && _value != "") return _value;
            _value = this.DevicePath;
            if (_value != null && _value != "") return _value;
            return base.ToString();
        }

        #endregion
    }

    public class DSCategory : DSEnumerator<DSDevice>
    {
        #region Properties

        public new DSFilter this[int index]
        {
            get
            {
                DSDevice _device = base[index];
                if (_device)
                {
                    return _device.Filter;
                }
                return null;
            }
        }

        public new DSFilter this[string name]
        {
            get
            {
                DSDevice _device = base[name];
                if (_device)
                {
                    return _device.Filter;
                }
                return null;
            }
        }

        public new DSFilter this[Guid _clsid]
        {
            get
            {
                DSDevice _device = base[_clsid];
                if (_device)
                {
                    return _device.Filter;
                }
                return null;
            }
        }

        #endregion

        #region Constructor

        public DSCategory(Guid _category)
            : base(_category)
        {

        }

        #endregion
    }

    #endregion

    #region Pin Helper

    public class DSPin : DSObject<IPin>
    {
        #region Constructor

        public DSPin(IPin _pin)
            : base(_pin)
        {

        }

        #endregion

        #region Properties

        public PinDirection Direction
        {
            get
            {
                m_pUnknown.QueryDirection(out PinDirection _direction);
                return _direction;
            }
        }

        public bool IsConnected
        {
            get
            {
                return ((ConnectedTo != null) && ConnectedTo == true);
            }
        }

        public string Name
        {
            get
            {
                if (this)
                {
                    if (SUCCEEDED(m_pUnknown.QueryPinInfo(out PinInfo _info)))
                    {
                        return _info.name;
                    }
                }
                return "";
            }
        }

        public DSFilter Filter
        {
            get
            {
                if (this)
                {
                    if (SUCCEEDED(m_pUnknown.QueryPinInfo(out PinInfo _info)))
                    {
                        return new DSFilter(_info.filter);
                    }
                }
                return null;
            }
        }

        public DSPin ConnectedTo
        {
            get
            {
                if (this)
                {
                    if (SUCCEEDED(m_pUnknown.ConnectedTo(out IPin pConnected)))
                    {
                        return new DSPin(pConnected);
                    }
                }
                return null;
            }
        }

        public AMMediaType ConnectionMediaType
        {
            get
            {
                if (this)
                {
                    var mt = new AMMediaType();
                    if (SUCCEEDED(m_pUnknown.ConnectionMediaType(mt)))
                    {
                        return mt;
                    }
                    mt.Free();
                }
                return null;
            }
        }

        public AMMediaType Format
        {
            get
            {
                if (this)
                {
                    var _config = (IAMStreamConfig)QueryInterface(typeof(IAMStreamConfig));
                    if (_config != null)
                    {
                        if (SUCCEEDED(_config.GetFormat(out AMMediaType mt)))
                        {
                            return mt;
                        }
                    }
                }
                return null;
            }
            set
            {
                if (this)
                {
                    var _config = (IAMStreamConfig)QueryInterface(typeof(IAMStreamConfig));
                    if (_config != null)
                    {
                        int hr = _config.SetFormat(value);
                        ASSERT(SUCCEEDED(hr));
                    }
                }
            }
        }

        public List<AMMediaType> MediaTypes
        {
            get
            {
                var _list = new List<AMMediaType>();
                if (this)
                {
                    if (SUCCEEDED(m_pUnknown.EnumMediaTypes(out IEnumMediaTypes pEnum)))
                    {
                        IntPtr ulMediaCount = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(uint)));
                        IntPtr pTypes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(IntPtr)));

                        try
                        {
                            do
                            {
                                int hr = pEnum.Next(1, pTypes, ulMediaCount);
                                if (hr != S_OK)
                                {
                                    break;
                                }
                                ASSERT(Marshal.ReadInt32(ulMediaCount) == 1);
                                IntPtr _ptrStructure = Marshal.ReadIntPtr(pTypes);
                                try
                                {
                                    if (_ptrStructure != IntPtr.Zero)
                                    {
                                        _list.Add((AMMediaType)Marshal.PtrToStructure(_ptrStructure, typeof(AMMediaType)));
                                    }
                                }
                                finally
                                {
                                    if (_ptrStructure != IntPtr.Zero)
                                    {
                                        Marshal.FreeCoTaskMem(_ptrStructure);
                                    }
                                }
                            }
                            while (true);
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(ulMediaCount);
                            Marshal.FreeCoTaskMem(pTypes);
                        }
                    }
                }
                return _list;
            }
        }

        public Guid Category
        {
            get
            {
                Guid _result = Guid.Empty;
                int iSize = Marshal.SizeOf(typeof(Guid));
                IntPtr ipOut = Marshal.AllocCoTaskMem(iSize);
                try
                {

                    var pKs = (IKsPropertySet)QueryInterface(typeof(IKsPropertySet));
                    if (pKs != null)
                    {
                        Guid g = PropSetID.Pin;
                        int hr = pKs.Get(g, (int)AMPropertyPin.Category, IntPtr.Zero, 0, ipOut, iSize, out int cbBytes);
                        if (SUCCEEDED(hr))
                        {
                            _result = (Guid)Marshal.PtrToStructure(ipOut, typeof(Guid));
                        }
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ipOut);
                    ipOut = IntPtr.Zero;
                }
                return _result;
            }
        }

        public AMMediaType this[int index]
        {
            get
            {
                try
                {
                    return MediaTypes[index];
                }
                catch
                {
                    return null;
                }
            }
        }

        public int Count
        {
            get { return MediaTypes.Count; }
        }

        #endregion

        #region Methods

        public HRESULT ConnectDirect(DSPin _pin)
        {
            try
            {
                if (!_pin)
                {
                    return Render();
                }
                if (!this) return (HRESULT)E_POINTER;
                IGraphBuilder _graph = Filter.FilterGraph;
                if (_graph != null)
                {
                    if (Direction != _pin.Direction)
                    {
                        if (Direction == PinDirection.Input)
                        {
                            return (HRESULT)_graph.ConnectDirect(_pin.m_pUnknown, m_pUnknown, null);
                        }
                        else
                        {
                            return (HRESULT)_graph.ConnectDirect(m_pUnknown, _pin.m_pUnknown, null);
                        }
                    }
                    return (HRESULT)VFW_E_INVALID_DIRECTION;
                }
                return (HRESULT)VFW_E_NOT_IN_GRAPH;
            }
            catch
            {
                return (HRESULT)E_UNEXPECTED;
            }
        }

        public HRESULT Connect(DSPin _pin)
        {
            try
            {
                HRESULT hr = ConnectDirect(_pin);
                if (hr.Succeeded)
                {
                    return hr;
                }
                if (!_pin)
                {
                    return Render();
                }
                if (!this) return (HRESULT)E_POINTER;
                IGraphBuilder _graph = Filter.FilterGraph;
                if (_graph != null)
                {
                    if (Direction != _pin.Direction)
                    {
                        if (Direction == PinDirection.Input)
                        {
                            return (HRESULT)_graph.Connect(_pin.m_pUnknown, m_pUnknown);
                        }
                        else
                        {
                            return (HRESULT)_graph.Connect(m_pUnknown, _pin.m_pUnknown);
                        }
                    }
                    return (HRESULT)VFW_E_INVALID_DIRECTION;
                }
                return (HRESULT)VFW_E_NOT_IN_GRAPH;
            }
            catch
            {
                return (HRESULT)E_UNEXPECTED;
            }
        }

        public HRESULT Disconnect()
        {
            DSPin _pin = null;
            try
            {
                if (!this) return (HRESULT)E_POINTER;
                IGraphBuilder _graph = Filter.FilterGraph;
                if (_graph != null)
                {
                    _pin = ConnectedTo;
                    if (!_pin) return (HRESULT)VFW_E_NOT_CONNECTED;
                    int hr = _graph.Disconnect(m_pUnknown);
                    if (SUCCEEDED(hr))
                    {
                        return (HRESULT)_graph.Disconnect(_pin.m_pUnknown);
                    }
                    return (HRESULT)hr;
                }
                return (HRESULT)VFW_E_NOT_IN_GRAPH;
            }
            catch
            {
                return (HRESULT)E_UNEXPECTED;
            }
            finally
            {
                if (_pin)
                {
                    _pin.Dispose();
                    _pin = null;
                }
            }
        }

        public HRESULT Render()
        {
            try
            {
                if (!this) return (HRESULT)E_POINTER;
                IGraphBuilder _graph = Filter.FilterGraph;
                if (_graph != null)
                {
                    return (HRESULT)_graph.Render(m_pUnknown);
                }
                return (HRESULT)VFW_E_NOT_IN_GRAPH;
            }
            catch
            {
                return (HRESULT)E_UNEXPECTED;
            }
        }

        public HRESULT IsAccepted(AMMediaType mt)
        {
            if (this)
            {
                return (HRESULT)m_pUnknown.QueryAccept(mt);
            }
            return (HRESULT)E_FAIL;
        }

        public DSPin RemoveFiltersChain()
        {
            DSPin _connected = ConnectedTo;
            if (_connected != null && _connected)
            {
                AMMediaType _type = ConnectionMediaType;
                Disconnect();
                List<DSPin> _pins = _connected.Filter.Pins;
                var _disconnect = new List<DSPin>();
                bool bShouldProceed = true;
                foreach (DSPin _pin in _pins)
                {
                    if (_pin.IsConnected)
                    {
                        if (_pin.Direction != Direction)
                        {
                            bShouldProceed = false;
                            break;
                        }
                        else
                        {
                            AMMediaType mt = _pin.ConnectionMediaType;
                            if (mt.majorType != _type.majorType)
                            {
                                bShouldProceed = false;
                                break;
                            }
                            _disconnect.Add(_pin);
                        }
                    }
                }
                if (bShouldProceed)
                {
                    DSPin _result = null;
                    foreach (DSPin _pin in _disconnect)
                    {
                        _result = _pin.RemoveFiltersChain();
                    }
                    DSFilter _filter = _connected.Filter;
                    _filter.FilterGraph = null;
                    _filter.Dispose();
                    if (Direction == PinDirection.Output)
                    {
                        _connected = this;
                    }
                    else
                    {
                        _connected = _result;
                    }
                }
            }
            return _connected;
        }

        #endregion

        #region Overriden Methods

        public override string ToString()
        {
            return " { " + base.ToString() + " } Name: '" + Name + "' ( " + Direction.ToString() + " )" + (IsConnected ? " Connected" : " Not Connected"); ;

        }

        #endregion
    }

    #endregion

    #region Filter Helper

    public class DSFilter : DSObject<IBaseFilter>
    {
        #region Constructor

        protected DSFilter()
        {

        }

        public DSFilter(IBaseFilter _filter)
            : base(_filter)
        {
        }

        public DSFilter(Guid _filter)
            : base(_filter)
        {

        }

        public DSFilter(string _moniker)
        {
            var _info = new MonikerInfo(_moniker);
            m_pUnknown = (IBaseFilter)_info.Bind(typeof(IBaseFilter));
        }

        #endregion

        #region Properties

        public List<DSPin> Pins
        {
            get
            {
                var _pins = new List<DSPin>();
                if (this)
                {
                    IPin[] pPins = new IPin[1];
                    if (SUCCEEDED(m_pUnknown.EnumPins(out IEnumPins pEnum)))
                    {
                        while (pEnum.Next(1, pPins, IntPtr.Zero) == S_OK)
                        {
                            if (pPins[0] != null)
                            {
                                _pins.Add(new DSPin(pPins[0]));
                            }
                        }
                    }
                }
                return _pins;
            }
        }

        public List<DSPin> Output
        {
            get
            {
                List<DSPin> _pins = Pins;
                var _output = new List<DSPin>();
                foreach (DSPin _pin in _pins)
                {
                    if (_pin.Direction == PinDirection.Output)
                    {
                        _output.Add(_pin);
                    }
                }
                return _output;
            }
        }

        public List<DSPin> Input
        {
            get
            {
                List<DSPin> _pins = Pins;
                var _output = new List<DSPin>();
                foreach (DSPin _pin in _pins)
                {
                    if (_pin.Direction == PinDirection.Input)
                    {
                        _output.Add(_pin);
                    }
                }
                return _output;
            }
        }

        public string Name
        {
            get
            {
                if (this)
                {
                    var _info = new FilterInfo();
                    if (SUCCEEDED(m_pUnknown.QueryFilterInfo(_info)))
                    {
                        if (_info.pUnk != null)
                        {
                            Marshal.ReleaseComObject(_info.pUnk);
                        }
                        return _info.achName;
                    }
                }
                return "";
            }
        }

        public bool HaveProperties
        {
            get { return IsSupported(typeof(ISpecifyPropertyPages)); }
        }

        public IGraphBuilder FilterGraph
        {
            get
            {
                IGraphBuilder _graph = null;
                if (this)
                {
                    var _info = new FilterInfo();
                    if (SUCCEEDED(m_pUnknown.QueryFilterInfo(_info)))
                    {
                        if (_info.pUnk != null)
                        {
                            _graph = (IGraphBuilder)_info.pUnk;
                            Marshal.ReleaseComObject(_info.pUnk);
                        }
                    }
                }
                return _graph;
            }
            set
            {
                if (this)
                {
                    if (value == null)
                    {
                        var _info = new FilterInfo();
                        if (SUCCEEDED(m_pUnknown.QueryFilterInfo(_info)))
                        {
                            if (_info.pUnk != null)
                            {
                                var _graph = (IGraphBuilder)_info.pUnk;
                                _graph.RemoveFilter(m_pUnknown);
                                Marshal.ReleaseComObject(_info.pUnk);
                            }
                        }
                    }
                    else
                    {
                        int hr = value.AddFilter(m_pUnknown, Name);
                        ASSERT(SUCCEEDED(hr));
                    }
                }
            }
        }

        public DSPin InputPin
        {
            get
            {
                List<DSPin> _pins = Input;
                if (_pins.Count > 0)
                    return _pins[0];
                return null;
            }
        }

        public DSPin OutputPin
        {
            get
            {
                List<DSPin> _pins = Output;
                if (_pins.Count > 0)
                    return _pins[0];
                return null;
            }
        }

        public DSPin this[int index]
        {
            get
            {
                return Pins[index];
            }
        }

        public DSPin this[string _name]
        {
            get
            {
                List<DSPin> _pins = Pins;
                foreach (DSPin _pin in _pins)
                {
                    if (_pin.Name == _name) return _pin;
                }
                return null;
            }
        }

        public int Count
        {
            get { return Pins.Count; }
        }

        #endregion

        #region Public Methods

        public HRESULT ShowProperties()
        {
            return ShowProperties(IntPtr.Zero);
        }

        public HRESULT ShowProperties(IntPtr _hwnd)
        {
            try
            {
                var pProp = (ISpecifyPropertyPages)QueryInterface(typeof(ISpecifyPropertyPages));
                if (pProp != null)
                {
                    pProp.GetPages(out DsCAUUID pCAUUID);
                    var oDevice = (object)m_pUnknown;
                    int hr = API.OleCreatePropertyFrame(_hwnd, 0, 0, Name, 1, ref oDevice, pCAUUID.cElems, pCAUUID.pElems, 0, 0, IntPtr.Zero);
                    if (hr < 0) Marshal.ThrowExceptionForHR(hr);
                    Marshal.FreeCoTaskMem(pCAUUID.pElems);
                    pProp = null;
                    return (HRESULT)hr;
                }
            }
            catch (Exception)
            {
            }
            return (HRESULT)E_FAIL;
        }

        public DSPin GetPin(PinDirection _direction)
        {
            return GetPin(_direction, 0);
        }

        public DSPin GetPin(PinDirection _direction, int index)
        {
            List<DSPin> _pins = null;
            switch (_direction)
            {
                case PinDirection.Input:
                    _pins = Input;
                    break;
                case PinDirection.Output:
                    _pins = Output;
                    break;
            }
            ASSERT(_pins);
            if (index >= 0 && _pins.Count < index)
            {
                return _pins[index];
            }
            return null;
        }

        public DSPin GetPin(string _name)
        {
            List<DSPin> _pins = Pins;
            foreach (DSPin _pin in _pins)
            {
                if (_pin.Name == _name) return _pin;
            }
            return null;
        }

        public DSPin GetPin(Guid _category)
        {
            List<DSPin> _pins = Pins;
            foreach (DSPin _pin in _pins)
            {
                if (_pin.Category == _category) return _pin;
            }
            return null;
        }

        public HRESULT Connect(DSFilter _filter)
        {
            if (_filter)
            {
                List<DSPin> _pins;
                {
                    _pins = _filter.Input;
                    foreach (DSPin _pin in _pins)
                    {
                        HRESULT hr = Connect(_pin);
                        if (hr.Succeeded) return hr;
                    }
                }
                {
                    _pins = _filter.Output;
                    foreach (DSPin _pin in _pins)
                    {
                        HRESULT hr = Connect(_pin);
                        if (hr.Succeeded) return hr;
                    }
                }
                return VFW_E_CANNOT_CONNECT;
            }
            return E_POINTER;
        }

        public HRESULT Connect(DSPin _pin)
        {
            if (_pin)
            {
                List<DSPin> _pins;
                if (_pin.Direction == PinDirection.Output)
                {
                    _pins = Input;
                }
                else
                {
                    _pins = Output;
                }
                foreach (DSPin _connect in _pins)
                {
                    if (!_connect.IsConnected)
                    {
                        HRESULT hr = _connect.Connect(_pin);
                        if (hr.Succeeded) return hr;
                    }
                }
                return VFW_E_CANNOT_CONNECT;
            }
            return E_POINTER;
        }

        public DSPin RemoveFiltersChain()
        {
            DSPin _input = null;
            DSPin _output = null;
            _output = RemoveFiltersChain(PinDirection.Output);
            _input = RemoveFiltersChain(PinDirection.Input);
            if (_output != null) return _output;
            return _input;
        }

        public DSPin RemoveFiltersChain(PinDirection _direction)
        {
            List<DSPin> _pins;
            if (_direction == PinDirection.Input)
            {
                _pins = Input;
            }
            else
            {
                _pins = Output;
            }
            DSPin _result = null;
            foreach (DSPin _pin in _pins)
            {
                DSPin _removed = _pin.RemoveFiltersChain();
                if (_removed != null && _result == null)
                {
                    _result = _removed;
                }
            }
            return _result;
        }

        #endregion

        #region Overriden Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    public class DSBaseSourceFilter : DSFilter
    {
        #region Properties

        public string FileName
        {
            get
            {
                var _source = (IFileSourceFilter)QueryInterface(typeof(IFileSourceFilter));
                if (_source != null)
                {
                    int hr = _source.GetCurFile(out string _path, null);
                    if (SUCCEEDED(hr))
                    {
                        return _path;
                    }
                }
                return null;
            }
            set
            {
                var _source = (IFileSourceFilter)QueryInterface(typeof(IFileSourceFilter));
                if (_source != null)
                {
                    int hr = _source.Load(value, null);
                    ASSERT(SUCCEEDED(hr));
                }
            }
        }

        #endregion

        #region Constructor

        protected DSBaseSourceFilter()
        {
        }

        public DSBaseSourceFilter(IBaseFilter _filter)
            : base(_filter)
        {
        }

        public DSBaseSourceFilter(Guid _filter)
            : base(_filter)
        {
        }

        public DSBaseSourceFilter(string _moniker)
            : base(_moniker)
        {
        }

        #endregion
    }

    public class DSBaseWriterFilter : DSFilter
    {
        #region Properties

        public string FileName
        {
            get
            {
                var _sink = (IFileSinkFilter)QueryInterface(typeof(IFileSinkFilter));
                if (_sink != null)
                {
                    int hr = _sink.GetCurFile(out string _path, null);
                    if (SUCCEEDED(hr))
                    {
                        return _path;
                    }
                }
                return null;
            }
            set
            {
                var _sink = (IFileSinkFilter)QueryInterface(typeof(IFileSinkFilter));
                if (_sink != null)
                {
                    int hr = _sink.SetFileName(value, null);
                    ASSERT(SUCCEEDED(hr));
                }
            }
        }

        #endregion

        #region Constructor

        protected DSBaseWriterFilter()
        {
        }

        public DSBaseWriterFilter(IBaseFilter _filter)
            : base(_filter)
        {
        }

        public DSBaseWriterFilter(Guid _filter)
            : base(_filter)
        {
        }

        public DSBaseWriterFilter(string _moniker)
            : base(_moniker)
        {
        }

        #endregion
    }

    #endregion

    #region Basic Types

    #region Categories

    public class DSVideoCaptureCategory : DSCategory
    {
        #region Constructor

        public DSVideoCaptureCategory()
            : base(FilterCategory.VideoInputDevice)
        {

        }

        #endregion
    }

    public class DSAudioCaptureCategory : DSCategory
    {
        #region Constructor

        public DSAudioCaptureCategory()
            : base(FilterCategory.AudioInputDevice)
        {

        }

        #endregion
    }

    public class DSVideoCompressorsCategory : DSCategory
    {
        #region Constructor

        public DSVideoCompressorsCategory()
            : base(FilterCategory.VideoCompressorCategory)
        {

        }

        #endregion
    }

    public class DSAudioCompressorsCategory : DSCategory
    {
        #region Constructor

        public DSAudioCompressorsCategory()
            : base(FilterCategory.AudioCompressorCategory)
        {

        }

        #endregion
    }

    public class DSAudioRenderersCategory : DSCategory
    {
        #region Constructor

        public DSAudioRenderersCategory()
            : base(FilterCategory.AudioRendererCategory)
        {

        }

        #endregion
    }

    #endregion

    #region Filters

    [Guid("f8388a40-d5bb-11d0-be5a-0080c706568e")]
    public class DSInfTeeFilter : DSFilter
    {
    }

    [Guid("cc58e280-8aa1-11d1-b3f1-00aa003761c5")]
    public class DSSmartTeeFilter : DSFilter
    {
    }

    [Guid("8596e5f0-0da5-11d0-bd21-00a0c911ce86")]
    public class DSFileWriter : DSBaseWriterFilter
    {
    }

    [Guid("e436ebb5-524f-11ce-9f53-0020af0ba770")]
    public class DSFileSourceAsync : DSBaseSourceFilter
    {
    }

    [Guid("79376820-07d0-11cf-a24d-0020afd79767")]
    public class DSDSoundRendererFilter : DSFilter
    {
    }

    [Guid("70e102b0-5556-11ce-97c0-00aa0055595a")]
    public class DSVideoRenderer : DSFilter
    {
    }

    [Guid("c1f400a4-3f08-11d3-9f0b-006008039e37")]
    public class DSNullRenderer : DSFilter
    {
    }

    [Guid("c1f400a0-3f08-11d3-9f0b-006008039e37")]
    public class DSSampleGrabberFilter : DSFilter, ISampleGrabberCB
    {
        #region Delegates

        public delegate void SampleEventHandler(DSSampleGrabberFilter _filter, IntPtr _buffer, int _length);

        #endregion

        #region Variables

        protected ISampleGrabber m_SampleGrabber = null;
        protected bool m_BufferedSamples = false;
        protected bool m_bOneShot = false;

        #endregion

        #region Events

        public event SampleEventHandler OnSample = null;

        #endregion

        #region Properties

        public AMMediaType MediaType
        {
            get
            {
                if (m_SampleGrabber != null)
                {
                    var mt = new AMMediaType();
                    if (SUCCEEDED(m_SampleGrabber.GetConnectedMediaType(mt)))
                    {
                        return mt;
                    }
                    mt.Free();
                }
                return null;
            }
            set
            {
                if (m_SampleGrabber != null)
                {
                    var hr = (HRESULT)m_SampleGrabber.SetMediaType(value);
                    hr.Assert();
                }
            }
        }

        public bool BufferedSamples
        {
            get { return m_BufferedSamples; }
            set
            {
                if (m_BufferedSamples != value)
                {
                    HRESULT hr = NOERROR;
                    if (m_SampleGrabber != null)
                    {
                        hr = (HRESULT)m_SampleGrabber.SetBufferSamples(value);
                        hr.Assert();
                    }
                    if (hr.Succeeded)
                    {
                        m_BufferedSamples = value;
                    }
                }
            }
        }

        public bool OneShot
        {
            get { return m_bOneShot; }
            set
            {
                if (m_bOneShot != value)
                {
                    HRESULT hr = NOERROR;
                    if (m_SampleGrabber != null)
                    {
                        hr = (HRESULT)m_SampleGrabber.SetOneShot(value);
                        hr.Assert();
                    }
                    if (hr.Succeeded)
                    {
                        m_bOneShot = value;
                    }
                }
            }
        }

        #endregion

        #region Constructor

        public DSSampleGrabberFilter()
        {
            m_SampleGrabber = (ISampleGrabber)QueryInterface(typeof(ISampleGrabber));
            ASSERT(m_SampleGrabber);
            if (m_SampleGrabber != null)
            {
                var hr = (HRESULT)m_SampleGrabber.SetCallback(this, 1);
                hr.Assert();
                hr = (HRESULT)m_SampleGrabber.SetBufferSamples(m_BufferedSamples);
                hr.Assert();
                hr = (HRESULT)m_SampleGrabber.SetOneShot(m_bOneShot);
                hr.Assert();
            }
        }

        #endregion

        #region ISampleGrabberCB Members

        public int SampleCB(double _time, IntPtr pSample)
        {
            return NOERROR;
        }

        public int BufferCB(double _time, IntPtr pBuffer, int _length)
        {
            OnSample?.Invoke(this, pBuffer, _length);
            return NOERROR;
        }

        #endregion

        #region Overridden Methods

        public override void Dispose()
        {
            m_SampleGrabber = (ISampleGrabber)QueryInterface(typeof(ISampleGrabber));
            if (m_SampleGrabber != null)
            {
                m_SampleGrabber.SetCallback(null, 0);
                m_SampleGrabber = null;
            }
            base.Dispose();
        }

        #endregion
    }

    [Guid("7c23220e-55bb-11d3-8b16-00c04fb6bd3d")]
    public class DSWMAsfWritter : DSBaseWriterFilter
    {
    }

    [Guid("187463A0-5BB7-11d3-ACBE-0080C75E246E")]
    public class DSWMAsfReader : DSBaseSourceFilter
    {
    }

    [Guid("E2510970-F137-11CE-8B67-00AA00A3F1A6")]
    public class DSAviMuxFilter : DSFilter
    {
    }

    #endregion

    #region Classes

    [Guid("e436ebb3-524f-11ce-9f53-0020af0ba770")]
    public class DSGraphBuilder : DSObject<IGraphBuilder>
    {
    }

    [Guid("BF87B6E1-8C27-11d0-B3F0-00AA003761C5")]
    public class DSCaptureGraphBuilder2 : DSObject<ICaptureGraphBuilder2>
    {
    }

    #endregion

    #endregion
}