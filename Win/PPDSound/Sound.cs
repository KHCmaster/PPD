using SharpGen.Runtime;
using PPDFramework;
using Vortice.DirectSound;
using Vortice.Multimedia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PPDSound
{
    public class Sound : IDisposable, ISound
    {
        private const int CHUNKSIZE = 4096;
        private IDirectSound soundDevice;
        private Form window;

        private Dictionary<string, BufferInfo> sounds;
        private Dictionary<string, SoundPlayingInfo> playingSounds;
        protected FMODEX.System system;

        bool enabled = true;
        bool disposed;

        public Sound(Form window)
        {
            this.window = window;
            sounds = new Dictionary<string, BufferInfo>();
            playingSounds = new Dictionary<string, SoundPlayingInfo>();
            SoundMasterControl.Instance.SeVolumeChanged += Instance_SeVolumeChanged;
        }

        public void Initialize()
        {
            soundDevice = DSound.DirectSoundCreate();
            soundDevice.SetCooperativeLevel(window.Handle, CooperativeLevel.Priority).CheckError();

            FMODEX.RESULT result;
            result = FMODEX.Factory.System_Create(ref system);
            CheckError(result);
            result = system.init(1, FMODEX.INITFLAGS.NORMAL, (IntPtr)null);
            CheckError(result);
        }

        public bool AddSound(string filename)
        {
            return AddSound(filename, filename);
        }

        public bool AddSound(string filename, string dicname)
        {
            if (!File.Exists(filename)) return false;
            if (sounds.ContainsKey(dicname)) return true;
            try
            {
                FMODEX.Sound sound = null;
                FMODEX.RESULT result;
                result = system.createSound(filename, FMODEX.MODE.OPENONLY | FMODEX.MODE.ACCURATETIME, ref sound);
                CheckError(result);
                var stream = GetStream(sound);
                sound.release();
                CheckError(result);
                AddSound(stream, dicname);
                return true;
            }
            catch
            {
                MessageBox.Show(PPDExceptionContentProvider.Provider.GetContent(PPDExceptionType.SoundReadError));
                return false;
            }
        }

        public bool AddSound(byte[] data, string dicname)
        {
            if (sounds.ContainsKey(dicname)) return true;
            try
            {
                // first write to temp file
                var temp = Path.GetTempFileName();
                File.WriteAllBytes(temp, data);

                FMODEX.Sound sound = null;
                FMODEX.RESULT result;
                result = system.createSound(temp, FMODEX.MODE.OPENONLY | FMODEX.MODE.ACCURATETIME, ref sound);
                CheckError(result);
                var stream = GetStream(sound);
                AddSound(stream, dicname);
                sound.release();
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
                return true;
            }
            catch
            {
                MessageBox.Show(PPDExceptionContentProvider.Provider.GetContent(PPDExceptionType.SoundReadError));
                return false;
            }
        }

        private void AddSound(Stream stream, string dictName)
        {
            if (sounds.ContainsKey(dictName))
            {
                return;
            }

            using (stream)
            {
                ReadWave(stream, out WaveFormat format, out int length);
                byte[] data = new byte[length];
                if (ReadFully(stream, data) != length)
                {
                    MessageBox.Show(PPDExceptionContentProvider.Provider.GetContent(PPDExceptionType.SoundReadError));
                    return;
                }
                var buffer = CreateSecondarySoundBuffer(format, length);
                buffer.Write(data, 0, LockFlags.EntireBuffer);
                try
                {
                    this.sounds.Add(dictName, new BufferInfo(buffer));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }
            }
        }

        /// <summary>
        /// GetStream で作った RIFF WAVE を読み、fmt チャンクの形式と data チャンクの長さを返します。
        /// ストリームは data チャンクの中身の先頭に位置します。
        /// </summary>
        private static void ReadWave(Stream stream, out WaveFormat format, out int length)
        {
            var reader = new BinaryReader(stream);
            if (new string(reader.ReadChars(4)) != "RIFF")
            {
                throw new InvalidDataException("Not a RIFF file");
            }
            reader.ReadInt32();
            if (new string(reader.ReadChars(4)) != "WAVE")
            {
                throw new InvalidDataException("Not a WAVE file");
            }
            format = null;
            while (true)
            {
                var id = new string(reader.ReadChars(4));
                var size = reader.ReadInt32();
                if (id == "fmt ")
                {
                    var next = stream.Position + size;
                    reader.ReadInt16();
                    var channels = reader.ReadInt16();
                    var sampleRate = reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt16();
                    var bits = reader.ReadInt16();
                    format = new WaveFormat(sampleRate, bits, channels);
                    stream.Position = next;
                }
                else if (id == "data")
                {
                    if (format == null)
                    {
                        throw new InvalidDataException("fmt chunk not found");
                    }
                    length = size;
                    return;
                }
                else
                {
                    stream.Position += size;
                }
            }
        }

        private static int ReadFully(Stream stream, byte[] data)
        {
            var total = 0;
            int read;
            while (total < data.Length && (read = stream.Read(data, total, data.Length - total)) > 0)
            {
                total += read;
            }
            return total;
        }

        private IDirectSoundBuffer CreateSecondarySoundBuffer(WaveFormat format, int size)
        {
            return soundDevice.CreateSoundBuffer(new SoundBufferDescription
            {
                Flags = BufferFlags.ControlVolume | BufferFlags.ControlPan | BufferFlags.ControlFrequency | BufferFlags.GlobalFocus,
                Format = format,
                BufferBytes = size
            }, null);
        }

        private Stream GetStream(FMODEX.Sound sound)
        {
            IntPtr data = IntPtr.Zero;
            MemoryStream stream = null;
            try
            {
                FMODEX.RESULT result;
                byte[] buffer = new byte[CHUNKSIZE];
                data = Marshal.AllocHGlobal(CHUNKSIZE);
                uint length = 0, read = 0;
                uint bytesread = 0;
                FMODEX.SOUND_FORMAT format = FMODEX.SOUND_FORMAT.AT9;
                FMODEX.SOUND_TYPE type = FMODEX.SOUND_TYPE.AIFF;
                int channels = 0, bits = 0;
                uint samplingrate = 0, ms = 0, pcm = 0;

                stream = new MemoryStream();
                result = sound.getLength(ref length, FMODEX.TIMEUNIT.PCMBYTES);
                CheckError(result);
                sound.getFormat(ref type, ref format, ref channels, ref bits);
                CheckError(result);
                sound.getLength(ref ms, FMODEX.TIMEUNIT.MS);
                CheckError(result);
                sound.getLength(ref pcm, FMODEX.TIMEUNIT.PCM);
                CheckError(result);
                samplingrate = 1000 * pcm / ms;

                byte[] bytes = new byte[4];
                stream.WriteByte((byte)'R');
                stream.WriteByte((byte)'I');
                stream.WriteByte((byte)'F');
                stream.WriteByte((byte)'F');
                stream.Write(bytes, 0, bytes.Length);
                stream.WriteByte((byte)'W');
                stream.WriteByte((byte)'A');
                stream.WriteByte((byte)'V');
                stream.WriteByte((byte)'E');
                stream.WriteByte((byte)'f');
                stream.WriteByte((byte)'m');
                stream.WriteByte((byte)'t');
                stream.WriteByte((byte)' ');
                bytes = BitConverter.GetBytes(16);
                stream.Write(bytes, 0, bytes.Length);
                stream.WriteByte((byte)1);
                stream.WriteByte((byte)0);
                bytes = BitConverter.GetBytes((ushort)channels);
                stream.Write(bytes, 0, bytes.Length);
                bytes = BitConverter.GetBytes(samplingrate);
                stream.Write(bytes, 0, bytes.Length);
                bytes = BitConverter.GetBytes((uint)(samplingrate * channels * bits / 8));
                stream.Write(bytes, 0, bytes.Length);
                bytes = BitConverter.GetBytes((ushort)(bits / 8 * channels));
                stream.Write(bytes, 0, bytes.Length);
                bytes = BitConverter.GetBytes((ushort)bits);
                stream.Write(bytes, 0, bytes.Length);
                stream.WriteByte((byte)'d');
                stream.WriteByte((byte)'a');
                stream.WriteByte((byte)'t');
                stream.WriteByte((byte)'a');
                bytes = BitConverter.GetBytes(length);
                stream.Write(bytes, 0, bytes.Length);

                bytesread = 0;
                do
                {
                    result = sound.readData(data, CHUNKSIZE, ref read);
                    Marshal.Copy(data, buffer, 0, CHUNKSIZE);
                    stream.Write(buffer, 0, (int)read);
                    bytesread += read;
                }
                while (result == FMODEX.RESULT.OK && read == CHUNKSIZE);
                stream.Seek(4, SeekOrigin.Begin);
                bytes = BitConverter.GetBytes((uint)(stream.Length - 8));
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
            }
            catch
            {

            }
            return stream;
        }
        public bool DeleteAllSound()
        {
            try
            {
                foreach (var buffer in sounds.Values)
                {
                    buffer.Buffer.Dispose();
                }
                playingSounds.Clear();
                sounds.Clear();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }

        public bool DeleteSound(string filename)
        {
            if (sounds.TryGetValue(filename, out BufferInfo buffer))
            {
                sounds.Remove(filename);
                buffer.Buffer.Dispose();
                playingSounds.Remove(filename);
                return true;
            }
            return false;
        }

        public void DeleteSoundStartsWith(string name)
        {
            var removeList = new List<string>();
            foreach (var kvp in sounds)
            {
                if (kvp.Key.StartsWith(name))
                {
                    removeList.Add(kvp.Key);
                }
            }

            foreach (string removeName in removeList)
            {
                sounds[removeName].Buffer.Dispose();
                sounds.Remove(removeName);
                playingSounds.Remove(removeName);
            }
        }

        public void Play(string filename, int vol)
        {
            Play(filename, vol, 1);
        }

        public void Play(string filename, int vol, double playRatio)
        {
            Play(filename, vol, playRatio, true);
        }

        private void Play(string filename, int vol, double playRatio, bool checkError)
        {
            if (sounds.TryGetValue(filename, out BufferInfo bufferInfo) && enabled)
            {
                try
                {
                    if (bufferInfo.Buffer.Status == (int)BufferStatus.BufferLost)
                    {
                        bufferInfo.Buffer.Restore();
                    }
                    bufferInfo.Buffer.Stop();
                    bufferInfo.Buffer.SetCurrentPosition(0);
                    bufferInfo.Buffer.SetVolume(SoundMasterControl.Instance.GetVolume(vol, SoundType.Se));
                    bufferInfo.Buffer.SetPan(0);
                    bufferInfo.Buffer.Frequency = (int)(bufferInfo.Frequency * playRatio);
                    bufferInfo.Buffer.Play(0, PlayFlags.None);
                    playingSounds.Remove(filename);
                    playingSounds.Add(filename, new SoundPlayingInfo(filename, vol, bufferInfo));
                }
                catch (SharpGenException e)
                {
                    if (checkError)
                    {
                        if (e.ResultCode.Code == (int)BufferStatus.BufferLost)
                        {
                            bufferInfo.Buffer.Restore();
                            Play(filename, vol, playRatio, false);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public void Stop(string filename)
        {
            if (sounds.TryGetValue(filename, out BufferInfo buffer))
            {
                buffer.Buffer.Stop();
                playingSounds.Remove(filename);
            }
        }

        public void GetSoundInfo(string filename, out FMODEX.SOUND_TYPE soundType, out FMODEX.SOUND_FORMAT soundFormat, out int channelCount, out int bits, out float length)
        {
            soundType = FMODEX.SOUND_TYPE.AIFF;
            soundFormat = FMODEX.SOUND_FORMAT.AT9;
            channelCount = 0;
            bits = 0;
            length = 0;
            uint uLength = 0;
            FMODEX.Sound sound = null;
            FMODEX.RESULT result;
            result = system.createSound(filename, FMODEX.MODE.HARDWARE, ref sound);
            CheckError(result);
            result = sound.getFormat(ref soundType, ref soundFormat, ref channelCount, ref bits);
            CheckError(result);
            result = sound.getLength(ref uLength, FMODEX.TIMEUNIT.MS);
            length = uLength / 1000f;
            CheckError(result);
            result = sound.release();
            CheckError(result);
        }

        void Instance_SeVolumeChanged()
        {
            foreach (SoundPlayingInfo info in playingSounds.Values)
            {
                if (info.BufferInfo.Buffer == null || info.BufferInfo.Buffer.NativePointer == IntPtr.Zero)
                {
                    continue;
                }
                info.BufferInfo.Buffer.SetVolume(SoundMasterControl.Instance.GetVolume(info.Volume, SoundType.Se));
            }
        }

        public Disposable Disable()
        {
            enabled = false;
            return new Disposable(() =>
            {
                enabled = true;
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void CheckError(FMODEX.RESULT result)
        {
            if (result != FMODEX.RESULT.OK)
            {
                throw new Exception("Error in Sound");
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    FMODEX.RESULT result;
                    foreach (var buffer in sounds.Values)
                    {
                        buffer.Buffer.Dispose();
                    }
                    sounds.Clear();
                    soundDevice.Dispose();
                    result = system.close();
                    CheckError(result);
                    result = system.release();
                    CheckError(result);
                    SoundMasterControl.Instance.SeVolumeChanged -= Instance_SeVolumeChanged;
                }
            }
            disposed = true;
        }
    }
}

