using System;
using System.Collections.Generic;
using System.Text;
using PPDFramework;

namespace testgame
{
    class SelectSongManager
    {
        public event EventHandler SongChanged;
        public event EventHandler DirectoryChanged;
        string directory;
        int selectedindex;
        Stack<KeyValuePair<string, int>> directorystack;
        SongInformation[] songinfos;
        public SelectSongManager()
        {
            directorystack = new Stack<KeyValuePair<string, int>>();
            selectedindex = 0;
        }
        private void FireSongChangeEvent()
        {
            if (SongChanged != null) SongChanged.Invoke(this, EventArgs.Empty);
        }
        private void FireDirectoryChangeEvent()
        {
            if (DirectoryChanged != null) DirectoryChanged.Invoke(this, EventArgs.Empty);
        }
        public string Directory
        {
            get { return directory; }
            set
            {
                directory = value;
                songinfos = SongInformation.GetFromDirectory(directory);
                FireDirectoryChangeEvent();
            }
        }
        public SongInformation[] SongInformations
        {
            get
            {
                return songinfos;
            }
        }
        public int Count
        {
            get
            {
                return songinfos.Length;
            }
        }
        public int SelectedIndex
        {
            get
            {
                return selectedindex;
            }
            set
            {
                selectedindex = value;
                FireSongChangeEvent();
            }
        }
        public SongInformation SelectedSongInformation
        {
            get
            {
                return songinfos[selectedindex];
            }
        }
        public void NextSong()
        {
            selectedindex++;
            if (selectedindex >= songinfos.Length) selectedindex = 0;
            FireSongChangeEvent();
        }
        public void PreviousSong()
        {
            selectedindex--;
            if (selectedindex < 0) selectedindex = songinfos.Length - 1;
            FireSongChangeEvent();
        }
        public void SeekSong(int move)
        {
            selectedindex += move;
            if (selectedindex < 0) selectedindex = songinfos.Length - 1;
            else if (selectedindex >= songinfos.Length) selectedindex = 0;
            FireSongChangeEvent();
        }
        public void GoDownDirectory()
        {
            if (!CanGoDown) return;
            directorystack.Push(new KeyValuePair<string, int>(directory, selectedindex));
            selectedindex = 0;
            directory = songinfos[selectedindex].DirectoryPath;
            songinfos = SongInformation.GetFromDirectory(directory);
            FireDirectoryChangeEvent();
            FireSongChangeEvent();
        }
        public void GoUpDirectory()
        {
            if (!CanGoUp) return;
            KeyValuePair<string, int> pair = directorystack.Pop();
            selectedindex = pair.Value;
            directory = pair.Key;
            songinfos = SongInformation.GetFromDirectory(directory);
            FireDirectoryChangeEvent();
            FireSongChangeEvent();
        }
        public bool CanGoDown
        {
            get
            {
                return !songinfos[selectedindex].IsPPDSong;
            }
        }
        public bool CanGoUp
        {
            get
            {
                return directorystack.Count > 0;
            }
        }
    }
}
