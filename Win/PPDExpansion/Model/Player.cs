using GalaSoft.MvvmLight;
using PPDExpansionCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PPDExpansion.Model
{
    public class Player : ViewModelBase
    {
        public ObservableCollection<KeyValuePair<DateTime, int>> Scores
        {
            get;
            private set;
        }

        public ImageSource Image
        {
            get;
            private set;
        }

        public Brush Brush
        {
            get;
            private set;
        }

        public PlayerInfo PlayerInfo
        {
            get;
            private set;
        }

        public Player(PlayerInfo playerInfo)
        {
            Scores = new ObservableCollection<KeyValuePair<DateTime, int>>();
            PlayerInfo = playerInfo;
            Image = new BitmapImage(new Uri(String.Format(@"http://projectdxxx.me/api/get-avator/s/16/id/{0}", playerInfo.AcccountId)));
            Brush = new SolidColorBrush(Color.FromRgb(playerInfo.R, playerInfo.G, playerInfo.B));
        }

        public void AddScore(DateTime dateTime, int score)
        {
            Scores.Add(new KeyValuePair<DateTime, int>(dateTime, score));
        }
    }
}
