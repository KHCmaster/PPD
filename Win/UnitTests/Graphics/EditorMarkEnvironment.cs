using PPDEditor;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using EditorUtility = PPDEditor.Utility;

namespace UnitTests.Graphics
{
    /// <summary>
    /// エディターのマークを作れるようにします（スキンの画像をテストの赤い画像にし、デバイスとリソースを渡す）。終わったら元に戻します。
    /// </summary>
    sealed class EditorMarkEnvironment : IDisposable
    {
        readonly TempFolder folder = new();
        readonly List<(FieldInfo Field, object Value)> savedSkin = [];
        readonly (PPDDevice, PPDFramework.Resource.ResourceManager, Vector2[]) savedUtility;

        public PPDFramework.Resource.ResourceManager Resources { get; } = new();

        public EditorMarkEnvironment(GraphicsDevice graphics)
        {
            var image = folder.Combine("mark.png");
            using (var bitmap = new System.Drawing.Bitmap(8, 8))
            {
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.Red);
                }
                bitmap.Save(image, System.Drawing.Imaging.ImageFormat.Png);
            }
            var path = PathObject.Absolute(image);
            var skin = PPDEditorSkin.Skin;
            foreach (var field in typeof(PPDEditorSkin).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (field.FieldType == typeof(PathObject))
                {
                    savedSkin.Add((field, field.GetValue(skin)));
                    field.SetValue(skin, path);
                }
                else if (field.FieldType == typeof(PathObject[]))
                {
                    var paths = (PathObject[])field.GetValue(skin);
                    savedSkin.Add((field, paths.Clone()));
                    Array.Fill(paths, path);
                }
            }
            savedUtility = (EditorUtility.Device, EditorUtility.ResourceManager, EditorUtility.CirclePoints);
            EditorUtility.Device = graphics.Device;
            EditorUtility.ResourceManager = Resources;
            EditorUtility.CirclePoints = [new Vector2(0, -10), new Vector2(10, 0), new Vector2(0, 10), new Vector2(-10, 0)];
        }

        public void Dispose()
        {
            foreach (var (field, value) in savedSkin)
            {
                if (value is PathObject[] paths)
                {
                    Array.Copy(paths, (PathObject[])field.GetValue(PPDEditorSkin.Skin), paths.Length);
                }
                else
                {
                    field.SetValue(PPDEditorSkin.Skin, value);
                }
            }
            (EditorUtility.Device, EditorUtility.ResourceManager, EditorUtility.CirclePoints) = savedUtility;
            Resources.Dispose();
            folder.Dispose();
        }
    }
}
