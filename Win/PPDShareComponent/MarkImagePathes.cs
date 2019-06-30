using PPDConfiguration;
using PPDCore;
using PPDFramework;
using System.IO;

namespace PPDShareComponent
{
    /// <summary>
    /// マークの画像のパスクラス
    /// </summary>
    public class MarkImagePaths : MarkImagePathsBase
    {
        static PathObject[] colorimagepaths = new PathObject[(int)ButtonType.Start + 4];
        static PathObject[] imagepaths = new PathObject[(int)ButtonType.Start + 4];
        static PathObject[] traceimagepaths = new PathObject[(int)ButtonType.Start + 4];
        static PathObject longnotecirclepath;
        static float _innerradius;
        static float _outerradius;
        static PathObject axisimagepath;
        static PathObject clockaxisimagepath;
        static PathObject[] effectpaths = new PathObject[(int)MarkEvaluateType.Worst + 1];
        static PathObject appeareffectpath;
        static PathObject slideeffectpath;
        static PathObject holdpath;
        static float holdx;
        static float holdy;

        static MarkImagePaths()
        {
            if (File.Exists(@"skins\defaultskin.ini"))
            {
                var pathManager = new PathManager(@"img\PPD\main_game");
                var sr = new StreamReader(@"skins\defaultskin.ini");
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                for (ButtonType type = ButtonType.Square; type < ButtonType.Start; type++)
                {
                    colorimagepaths[(int)type] = pathManager.Combine(setting.ReadString("Color" + type));
                    imagepaths[(int)type] = pathManager.Combine(setting.ReadString(type.ToString()));
                    traceimagepaths[(int)type] = pathManager.Combine(setting.ReadString("Trace" + type));
                }
                colorimagepaths[(int)ButtonType.Start] = pathManager.Combine(setting["ColorSliderRight"]);
                colorimagepaths[(int)ButtonType.Start + 1] = pathManager.Combine(setting["ColorSliderLeft"]);
                colorimagepaths[(int)ButtonType.Start + 2] = pathManager.Combine(setting["ColorSliderRightExtra"]);
                colorimagepaths[(int)ButtonType.Start + 3] = pathManager.Combine(setting["ColorSliderLeftExtra"]);
                imagepaths[(int)ButtonType.Start] = pathManager.Combine(setting["SliderRight"]);
                imagepaths[(int)ButtonType.Start + 1] = pathManager.Combine(setting["SliderLeft"]);
                imagepaths[(int)ButtonType.Start + 2] = pathManager.Combine(setting["SliderRightExtra"]);
                imagepaths[(int)ButtonType.Start + 3] = pathManager.Combine(setting["SliderLeftExtra"]);
                traceimagepaths[(int)ButtonType.Start] =
                    traceimagepaths[(int)ButtonType.Start + 1] =
                    traceimagepaths[(int)ButtonType.Start + 2] =
                    traceimagepaths[(int)ButtonType.Start + 3] = pathManager.Combine(setting["TraceSlider"]);
                longnotecirclepath = pathManager.Combine(setting.ReadString("LongNoteCirle"));
                _innerradius = float.Parse(setting.ReadString("InnerRadius"));
                _outerradius = float.Parse(setting.ReadString("OuterRadius"));
                axisimagepath = pathManager.Combine(setting.ReadString("CircleAxis"));
                clockaxisimagepath = pathManager.Combine(setting.ReadString("ClockAxis"));
                for (MarkEvaluateType type = MarkEvaluateType.Cool; type <= MarkEvaluateType.Worst; type++)
                {
                    effectpaths[(int)type] = pathManager.Combine(setting.ReadString("Effect" + type));
                }
                appeareffectpath = pathManager.Combine(setting.ReadString("EffectAppear"));
                slideeffectpath = pathManager.Combine(setting.ReadString("EffectSlide"));
                holdpath = pathManager.Combine(setting.ReadString("Hold"));
                holdx = float.Parse(setting.ReadString("HoldX"));
                holdy = float.Parse(setting.ReadString("HoldY"));
            }
        }

        public override PathObject GetMarkColorImagePath(ButtonType buttontype)
        {
            return colorimagepaths[(int)buttontype];
        }

        public override PathObject GetMarkImagePath(ButtonType buttontype)
        {
            return imagepaths[(int)buttontype];
        }

        public override PathObject GetTraceImagePath(ButtonType buttontype)
        {
            return traceimagepaths[(int)buttontype];
        }

        public override void GetLongNoteCircleInfo(out PathObject imagepath, out float innerradius, out float outerradius)
        {
            imagepath = longnotecirclepath;
            innerradius = _innerradius;
            outerradius = _outerradius;
        }

        public override PathObject GetClockAxisImagePath()
        {
            return clockaxisimagepath;
        }

        public override PathObject GetCircleAxisImagePath()
        {
            return axisimagepath;
        }

        public override PathObject GetEvaluateEffectPath(MarkEvaluateType evatype)
        {
            return effectpaths[(int)evatype];
        }
        public override PathObject GetAppearEffectPath()
        {
            return appeareffectpath;
        }
        public override PathObject GetSlideEffectPath()
        {
            return slideeffectpath;
        }
        public override void GetHoldInfo(out PathObject imagepath, out float x, out float y)
        {
            imagepath = holdpath;
            x = holdx;
            y = holdy;
        }
    }
}
