using CommunityToolkit.Mvvm.ComponentModel;
using PPDExpansionCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PPDExpansion.Model
{
    public class Player(PlayerInfo playerInfo) : ObservableObject
    {
        public ObservableCollection<KeyValuePair<DateTime, int>> Scores
        {
            get;
            private set;
        } = [];

        public ImageSource Image
        {
            get;
            private set;
        } = new BitmapImage(new Uri(String.Format(@"http://projectdxxx.me/api/get-avator/s/16/id/{0}", playerInfo.AcccountId)));

        public Brush Brush
        {
            get;
            private set;
        } = new SolidColorBrush(Color.FromRgb(playerInfo.R, playerInfo.G, playerInfo.B));

        public PlayerInfo PlayerInfo
        {
            get;
            private set;
        } = playerInfo;

        public void AddScore(DateTime dateTime, int score)
        {
            Scores.Add(new KeyValuePair<DateTime, int>(dateTime, score));
        }
    }
}
