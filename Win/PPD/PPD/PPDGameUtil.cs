using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;


namespace testgame
{
    public static class FontUtl
    {
        public static string fontname = "ＭＳ ゴシック";
        public static FontWeight fontweight = FontWeight.Normal;
        public static int fontsizeratio = 32;
        public const int basesize = 32;
    }
    public class PPDGameUtil
    {
        public PPDGameUtil()
        {

        }
        public string Directory
        {
            get;
            set;
        }
        public int SelectNumber
        {
            get;
            set;
        }
        public int Difficulty
        {
            get;
            set;
        }
        public string DifficultString
        {
            get;
            set;
        }
        public int ProfileNumber
        {
            get;
            set;
        }
        public bool Auto
        {
            get;
            set;
        }
        public bool Random
        {
            get;
            set;
        }
        public float BPM
        {
            get;
            set;
        }
        public float SpeedScale
        {
            get;
            set;
        }
        public float StartTime
        {
            get;
            set;
        }
        public float[] Evals
        {
            get;
            set;
        }
        public int[] KeyChange
        {
            get;
            set;
        }
        public float AdjustGaptime
        {
            get;
            set;
        }
    }
}
