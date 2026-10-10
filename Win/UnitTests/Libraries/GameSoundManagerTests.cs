using PPDCore;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;
using PPDFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// ボタンを押したときの音（ゲームの SoundManager）のテストです。音は鳴らさず、呼び出しを記録する。
    /// </summary>
    public class GameSoundManagerTests
    {
        sealed class RecordingSound : ISound
        {
            public List<string> Calls { get; } = [];
            public bool AddSound(string filename) => Add($"add {filename}");
            public bool AddSound(string filename, string dicname) => Add($"add {Path.GetFileName(filename)} {dicname}");
            public bool AddSound(byte[] bytes, string dicname) => Add($"add bytes {dicname}");
            public bool DeleteSound(string filename) => Add($"delete {filename}");
            public void DeleteSoundStartsWith(string name) => Add($"delete* {name}");
            public bool DeleteAllSound() => Add("delete all");
            public void Play(string filename, int vol) => Add($"play {filename} {vol}");
            public void Play(string filename, int vol, double playRatio) => Add($"play {filename} {vol} {playRatio}");
            public void Stop(string filename) => Add($"stop {filename}");
            public Disposable Disable() => null;

            bool Add(string call)
            {
                Calls.Add(call);
                return true;
            }
        }

        static (SoundManager Manager, RecordingSound Sound) Create(TempFolder folder, bool withSoundSet, params IEVDData[] events)
        {
            if (withSoundSet)
            {
                folder.Write("soundset.txt", "a.wav\r\nb.wav\r\n");
                folder.Write(Path.Combine("sound", "a.wav"), "");
                folder.Write(Path.Combine("sound", "b.wav"), "");
            }
            // ○は 0 秒に 1 番、2 秒に 2 番。×は 1 秒に 2 番の音
            SCDWriter.Write(folder.Combine("Easy.scd"), [new SCDData(0, ButtonType.Circle, 1), new SCDData(1, ButtonType.Cross, 2), new SCDData(2, ButtonType.Circle, 2)]);
            var song = new SongInformation();
            typeof(SongInformation).GetProperty(nameof(SongInformation.DirectoryPath)).SetValue(song, folder.Path);
            typeof(SongInformation).GetProperty(nameof(SongInformation.BPM)).SetValue(song, 120f);
            var utility = new PPDGameUtility { SongInformation = song, Difficulty = Difficulty.Easy };
            var sound = new RecordingSound();
            var script = (FlowScriptManager)RuntimeHelpers.GetUninitializedObject(typeof(FlowScriptManager));
            typeof(FlowScriptManager).GetProperty(nameof(FlowScriptManager.BPMManager)).SetValue(script, new PPDCoreModel.BPMManager());
            var stream = new MemoryStream();
            EVDWriter.Write(stream, events);
            stream.Position = 0;
            var manager = new SoundManager(new ExSound(sound), utility) { EventManager = new EventManager(utility, stream, MainGameConfigBase.Default, script) };
            return (manager, sound);
        }

        static bool[] Buttons(params ButtonType[] buttons)
        {
            var array = new bool[10];
            foreach (var button in buttons)
            {
                array[(int)button] = true;
            }
            return array;
        }

        [Fact]
        public void PressedButtons_PlayTheCurrentSound()
        {
            using var folder = new TempFolder();
            var (manager, sound) = Create(folder, true);
            Assert.Equal(["add a.wav 1", "add b.wav 2"], sound.Calls);
            sound.Calls.Clear();
            var volume = -100 * (100 - manager.EventManager.VolumePercents[(int)ButtonType.Circle]);

            manager.Update(0.5f, Buttons(ButtonType.Circle), Buttons(), 1);
            manager.Update(1.5f, Buttons(ButtonType.Cross), Buttons(), 1);
            manager.Update(2.5f, Buttons(ButtonType.Circle), Buttons(), 1);

            Assert.Equal([$"play 1 {volume}", $"play 2 {volume}", $"play 2 {volume}"], sound.Calls);

            // 途中から始めると、その時の音
            sound.Calls.Clear();
            manager.Seek(1.5f);
            manager.Update(1.5f, Buttons(ButtonType.Circle), Buttons(), 1);
            Assert.Equal([$"play 1 {volume}"], sound.Calls);

            // 指定した音量、番号で
            sound.Calls.Clear();
            Assert.True(manager.Play(MarkType.Circle, -500, 1));
            Assert.True(manager.Play(MarkType.Circle, 1));
            Assert.False(manager.Play((MarkType)20, 0, 1));
            Assert.True(manager.Stop(MarkType.Circle));
            Assert.False(manager.Stop((MarkType)20));
            Assert.False(manager.Play(5, 0, 1));
            Assert.Equal(["play 1 -500", $"play 1 {volume}", "stop 1"], sound.Calls);

            sound.Calls.Clear();
            manager.Dispose();
            Assert.Equal(["delete 0", "delete 1", "delete 2"], sound.Calls);
        }

        [Fact]
        public void KeepPlaying_StopsWhenReleased()
        {
            using var folder = new TempFolder();
            // ○は押している間鳴らし続ける
            var (manager, sound) = Create(folder, true, new ChangeSoundPlayModeEvent(0, true, (int)ButtonType.Circle + 1));
            manager.EventManager.Update(0);
            sound.Calls.Clear();

            manager.Update(0.5f, Buttons(ButtonType.Circle), Buttons(), 1);
            manager.Update(0.6f, Buttons(), Buttons(ButtonType.Circle), 1);

            Assert.Equal("stop 1", sound.Calls[^1]);

            // スクリプトからの鳴らし方
            sound.Calls.Clear();
            manager.SpPlay((int)ButtonType.Circle, 100, 1);
            manager.SpKeepPlay((int)ButtonType.Circle, 50, manager.EventManager.KeepPlayings, 1);
            manager.SpStopSound((int)ButtonType.Circle);
            manager.SpPlay(20, 100, 1);
            Assert.Equal(["play 1 0", "play 1 -5000", "stop 1"], sound.Calls);
        }

        [Fact]
        public void WithoutSoundSet_PlaysNothing()
        {
            using var folder = new TempFolder();
            var (manager, sound) = Create(folder, false);

            manager.Update(0.5f, Buttons(ButtonType.Circle), Buttons(), 1);
            manager.Seek(1);

            Assert.Empty(sound.Calls);
        }
    }
}
