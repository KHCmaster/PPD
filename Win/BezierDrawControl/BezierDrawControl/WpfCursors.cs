using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BezierDrawControl
{
    /// <summary>
    /// <see cref="BezierCursor"/> を WPF のカーソルにします。
    /// </summary>
    public static class WpfCursors
    {
        const int CursorSize = 32;

        static readonly Dictionary<string, Cursor> images = [];
        static readonly Dictionary<int, Cursor> arrows = [];

        public static Cursor Get(BezierCursor cursor)
        {
            return cursor.Kind switch
            {
                BezierCursorKind.SizeAll => Cursors.SizeAll,
                BezierCursorKind.Rotate => FromImage("rotate.png"),
                BezierCursorKind.Anchor => FromImage("anchor.png"),
                BezierCursorKind.AnchorPlus => FromImage("anchorplus.png"),
                BezierCursorKind.Handle => FromImage("handle.png"),
                BezierCursorKind.Arrow => CreateArrow(cursor.Angle),
                _ => Cursors.Arrow,
            };
        }

        /// <summary>
        /// 修飾キーを <see cref="BezierModifierKeys"/> にします。
        /// </summary>
        public static BezierModifierKeys ToBezierModifierKeys(ModifierKeys keys)
        {
            var result = BezierModifierKeys.None;
            if (keys.HasFlag(ModifierKeys.Shift)) result |= BezierModifierKeys.Shift;
            if (keys.HasFlag(ModifierKeys.Control)) result |= BezierModifierKeys.Control;
            if (keys.HasFlag(ModifierKeys.Alt)) result |= BezierModifierKeys.Alt;
            return result;
        }

        private static Cursor FromImage(string fileName)
        {
            if (!images.TryGetValue(fileName, out var cursor))
            {
                var source = new BitmapImage(new Uri("pack://application:,,,/BezierDrawControl;component/img/" + fileName));
                cursor = Create(source);
                images[fileName] = cursor;
            }
            return cursor;
        }

        private static Cursor CreateArrow(int angle)
        {
            if (!arrows.TryGetValue(angle, out var cursor))
            {
                // 以前（GDI+ で描いていたとき）と同じ形の両矢印を、中心で回転して描く
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    Point[] points = [new(8, 0), new(5, 3), new(5, 1), new(-5, 1), new(-5, 3), new(-8, 0), new(-5, -3), new(-5, -1), new(5, -1), new(5, -3)];
                    context.BeginFigure(points[0], true, true);
                    for (var i = 1; i < points.Length; i++)
                    {
                        context.LineTo(points[i], false, false);
                    }
                }
                geometry.Transform = new TransformGroup
                {
                    Children = { new RotateTransform(angle), new TranslateTransform(CursorSize / 2, CursorSize / 2) }
                };
                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    dc.DrawGeometry(Brushes.Black, null, geometry);
                }
                var bitmap = new RenderTargetBitmap(CursorSize, CursorSize, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);
                cursor = Create(bitmap);
                arrows[angle] = cursor;
            }
            return cursor;
        }

        /// <summary>
        /// 32x32 の画像から、中心をホットスポットにしたカーソルを作ります（以前の Bitmap.GetHicon と同じ位置）。
        /// </summary>
        private static Cursor Create(BitmapSource source)
        {
            var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            var stride = CursorSize * 4;
            var pixels = new byte[stride * CursorSize];
            converted.CopyPixels(new Int32Rect(0, 0, Math.Min(CursorSize, converted.PixelWidth), Math.Min(CursorSize, converted.PixelHeight)), pixels, stride, 0);

            // .cur 形式（32bpp の DIB と AND マスク）にする
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
            {
                var maskStride = CursorSize / 8;
                var imageSize = 40 + pixels.Length + maskStride * CursorSize;
                writer.Write((short)0);
                writer.Write((short)2);
                writer.Write((short)1);
                writer.Write((byte)CursorSize);
                writer.Write((byte)CursorSize);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((short)(CursorSize / 2));
                writer.Write((short)(CursorSize / 2));
                writer.Write(imageSize);
                writer.Write(22);
                writer.Write(40);
                writer.Write(CursorSize);
                writer.Write(CursorSize * 2);
                writer.Write((short)1);
                writer.Write((short)32);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                for (var y = CursorSize - 1; y >= 0; y--)
                {
                    writer.Write(pixels, y * stride, stride);
                }
                writer.Write(new byte[maskStride * CursorSize]);
            }
            stream.Position = 0;
            return new Cursor(stream);
        }
    }
}
