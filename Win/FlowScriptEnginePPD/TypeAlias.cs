using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;
using PPDFramework.Vertex;
using PPDSound;
using System;
using System.Collections.Generic;

namespace FlowScriptEnginePPD
{
    public class TypeAlias : TypeAliasBase
    {
        Dictionary<Type, string> dictionary;
        public TypeAlias()
        {
            dictionary = new Dictionary<Type, string>
            {
                { typeof(RectangleComponent), "PPD.Graphics.Rectangle" },
                { typeof(PictureObject), "PPD.Graphics.Image" },
                { typeof(TextureString), "PPD.Graphics.Text" },
                { typeof(NumberPictureObject), "PPD.Graphics.Number" },
                { typeof(EffectObject), "PPD.Graphics.Effect" },
                { typeof(SpriteObject), "PPD.Graphics.Sprite" },
                { typeof(PolygonObject), "PPD.Graphics.Polygon" },
                { typeof(ColoredTexturedVertex), "PPD.Graphics.Vertex" },
                { typeof(VertexInfo), "PPD.Graphics.VertexBuffer" },
                { typeof(SoundResource), "PPD.Audio.Sound" },
                { typeof(GameComponent), "PPD.Graphics" },
                { typeof(PPDCoreModel.Data.EffectType), "PPD.Mark.EffectType" },
                { typeof(PPDCoreModel.Data.LayerType), "PPD.LayerType" },
                { typeof(PPDCoreModel.Data.MarkType), "PPD.MarkType" },
                { typeof(Effect2D.EffectManager.PlayType), "PPD.Graphics.Effect.PlayType" },
                { typeof(Effect2D.EffectManager.PlayState), "PPD.Graphics.Effect.PlayState" },
                { typeof(EffectPool), "PPD.Graphics.Effect.Pool" },
                { typeof(PPDFramework.MarkEvaluateType), "PPD.EvaluateType" },
                { typeof(PPDFrameworkCore.Difficulty), "PPD.Difficulty" },
                { typeof(Alignment), "PPD.Graphics.Number.Alignment" },
                { typeof(PPDFramework.Mod.ModInfo), "PPD.Mod.ModInfo" },
                { typeof(ResultInfo), "PPD.Song.Result" },
                { typeof(ResultEvaluateType), "PPD.Song.Result.ResultType" },
                { typeof(PPDFramework.NoteType), "PPD.NoteType" },
                { typeof(PPDFramework.PPDStructure.PPDData.MarkDataBase), "PPD.Mark" },
                { typeof(PPDFramework.ScreenFilters.ScreenFilterBase), "PPD.Graphics.ScreenFilter" },
                { typeof(PPDFramework.Shaders.ColorFilterBase), "PPD.Graphics.ColorFilter" },
                { typeof(Effect2D.BlendMode), "PPD.Graphics.Blend" },
                { typeof(PPDFramework.Shaders.MaskType), "PPD.Graphics.MaskType" },
                { typeof(PPDFramework.PrimitiveType), "PPD.Graphics.PrimitiveType" },
                { typeof(PPDFramework.ScreenFilters.GaussianFilter), "PPD.Graphics.ScreenFilter.Gaussian" },
                { typeof(PPDFramework.ScreenFilters.ColorScreenFilter), "PPD.Graphics.ScreenFilter.Color" },
                { typeof(PPDFramework.Shaders.SaturationColorFilter), "PPD.Graphics.ColorFilter.Saturation" },
                { typeof(PPDFramework.Shaders.NTSCGrayScaleColorFilter), "PPD.Graphics.ColorFilter.NTSCGrayScale" },
                { typeof(PPDFramework.Shaders.MiddleGrayScaleColorFilter), "PPD.Graphics.ColorFilter.MiddleGrayScale" },
                { typeof(PPDFramework.Shaders.MedianGrayScaleColorFilter), "PPD.Graphics.ColorFilter.MedianGrayScale" },
                { typeof(PPDFramework.Shaders.MaxGrayScaleColorFilter), "PPD.Graphics.ColorFilter.MaxGrayScale" },
                { typeof(PPDFramework.Shaders.InvertColorFilter), "PPD.Graphics.ColorFilter.Invert" },
                { typeof(PPDFramework.Shaders.HueColorFilter), "PPD.Graphics.ColorFilter.Hue" },
                { typeof(PPDFramework.Shaders.HDTVGrayScaleColorFilter), "PPD.Graphics.ColorFilter.HDTVGrayScale" },
                { typeof(PPDFramework.Shaders.GreenGrayScaleColorFilter), "PPD.Graphics.ColorFilter.GreenGrayScale" },
                { typeof(PPDFramework.Shaders.ColorFilter), "PPD.Graphics.ColorFilter.Color" },
                { typeof(PPDFramework.Shaders.BrightnessColorFilter), "PPD.Graphics.ColorFilter.Brightness" },
                { typeof(PPDFramework.Shaders.AverageGrayScaleColorFilter), "PPD.Graphics.ColorFilter.AverageGrayScale" }
            };
        }

        public override IEnumerable<KeyValuePair<Type, string>> EnumerateAlias()
        {
            return dictionary;
        }
    }
}
