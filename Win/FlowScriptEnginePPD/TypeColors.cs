using FlowScriptEngine;
using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.Vertex;
using PPDSound;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FlowScriptEnginePPD
{
    public class TypeColors : TypeColorBase
    {
        Dictionary<Type, Color> dictionary;
        public TypeColors()
        {
            dictionary = new Dictionary<Type, Color>
            {
                { typeof(EffectObject), Color.FromArgb(255, 175, 159) },
                { typeof(EffectPool), Color.FromArgb(253, 211, 202) },
                { typeof(GameComponent), Color.FromArgb(231, 23, 23) },
                { typeof(PictureObject), Color.FromArgb(206, 93, 71) },
                { typeof(NumberPictureObject), Color.FromArgb(155, 72, 57) },
                { typeof(SoundResource), Color.FromArgb(57, 155, 117) },
                { typeof(TextureString), Color.FromArgb(200, 198, 43) },
                { typeof(SpriteObject), Color.FromArgb(217, 217, 217) },
                { typeof(RectangleComponent), Color.FromArgb(127, 250, 160) },
                { typeof(PolygonObject), Color.FromArgb(200, 200, 200) },
                { typeof(ColoredTexturedVertex), Color.FromArgb(160, 160, 160) },
                { typeof(VertexInfo), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.Mod.ModInfo), Color.FromArgb(0, 150, 60) },
                { typeof(ResultInfo), Color.FromArgb(0, 100, 60) },
                { typeof(PPDFramework.PPDStructure.PPDData.MarkDataBase), Color.FromArgb(0, 50, 60) },
                { typeof(PPDFramework.ScreenFilters.ScreenFilterBase), Color.FromArgb(0, 0, 0) },
                { typeof(PPDFramework.Shaders.ColorFilterBase), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.ScreenFilters.GaussianFilter), Color.FromArgb(0, 0, 0) },
                { typeof(PPDFramework.ScreenFilters.ColorScreenFilter), Color.FromArgb(0, 0, 0) },
                { typeof(PPDFramework.Shaders.SaturationColorFilter), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.Shaders.NTSCGrayScaleColorFilter), Color.FromArgb(20, 20, 20) },
                { typeof(PPDFramework.Shaders.MiddleGrayScaleColorFilter), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.Shaders.MedianGrayScaleColorFilter), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.Shaders.MaxGrayScaleColorFilter), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.Shaders.InvertColorFilter), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.Shaders.HueColorFilter), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.Shaders.HDTVGrayScaleColorFilter), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.Shaders.GreenGrayScaleColorFilter), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.Shaders.ColorFilter), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.Shaders.BrightnessColorFilter), Color.FromArgb(120, 120, 120) },
                { typeof(PPDFramework.Shaders.AverageGrayScaleColorFilter), Color.FromArgb(120, 120, 120) },

                { typeof(MarkType), Color.FromArgb(0, 70, 102) },
                { typeof(Effect2D.EffectManager.PlayType), Color.FromArgb(0, 70, 102) },
                { typeof(Effect2D.EffectManager.PlayState), Color.FromArgb(0, 70, 102) },
                { typeof(EffectType), Color.FromArgb(0, 70, 102) },
                { typeof(LayerType), Color.FromArgb(0, 70, 102) },
                { typeof(MarkEvaluateType), Color.FromArgb(0, 70, 102) },
                { typeof(PPDFrameworkCore.Difficulty), Color.FromArgb(0, 70, 102) },
                { typeof(Alignment), Color.FromArgb(0, 70, 102) },
                { typeof(ResultEvaluateType), Color.FromArgb(0, 70, 102) },
                { typeof(PPDFramework.NoteType), Color.FromArgb(0, 70, 102) },
                { typeof(Effect2D.BlendMode), Color.FromArgb(0, 70, 102) },
                { typeof(PPDFramework.Shaders.MaskType), Color.FromArgb(0, 70, 102) },
                { typeof(PPDFramework.PrimitiveType), Color.FromArgb(0, 70, 102) }
            };
        }

        public override IEnumerable<KeyValuePair<Type, Color>> EnumerateColors()
        {
            return dictionary;
        }
    }
}
