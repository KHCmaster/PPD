using PPDFramework;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Vortice.Mathematics;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// 描画のテストで使う DirectX のデバイスです。表示しないウィンドウのハンドルで作る（画面には何も出ない）。
    /// </summary>
    /// <remarks>
    /// デバイスは 1 つのスレッドから使う前提なので、描画のテストは <see cref="GraphicsCollection"/> で順に動かす。
    /// </remarks>
    public sealed class GraphicsDevice : IDisposable
    {
        public const int Width = 800;
        public const int Height = 450;

        static readonly MethodInfo drawStart = typeof(PPDDevice).GetMethod("DrawStart", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly MethodInfo drawEnd = typeof(PPDDevice).GetMethod("DrawEnd", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly MethodInfo clear = typeof(PPDDevice).GetMethod("Clear", BindingFlags.Instance | BindingFlags.NonPublic, [typeof(Color4)]);

        readonly System.Windows.Forms.Control window;

        public GraphicsDevice()
        {
            // Show しないコントロールのハンドル（スワップチェーンを作るのに要る）
            window = new System.Windows.Forms.Control { Width = Width, Height = Height };
            Device = PPDDevice.Initialize(window.Handle, Width, Height, Width, Height, 16f / 9);
            // GameCoreBase.InitializeDirectX と同じモジュールを入れる（どれも internal なので名前で作る）
            var register = typeof(PPDDevice).GetMethod("RegisterModule", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var (name, withDevice) in new[]
            {
                ("PPDFramework.AlphaBlendContextCache", false), ("PPDFramework.ShaderCommon", true), ("PPDFramework.ColorTextureAllcator", true),
                ("PPDFramework.Chars.CharCacheManager", true), ("PPDFramework.Shaders.AlphaBlend", true), ("PPDFramework.ScreenFilters.Impl.GaussianFilter", true),
                ("PPDFramework.ScreenFilters.Impl.ColorScreenFilter", false), ("PPDFramework.Shaders.GlassFilter", true),
                ("PPDFramework.Shaders.BorderFilter", true), ("PPDFramework.Shaders.MosaicFilter", true),
            })
            {
                var type = typeof(PPDDevice).Assembly.GetType(name, true);
                object[] args = name.EndsWith("AlphaBlendContextCache") ? [1024] : withDevice ? [Device] : [];
                register.Invoke(Device, [Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null)]);
            }
            var shaderCommon = typeof(PPDDevice).GetMethod("GetModule").MakeGenericMethod(typeof(PPDDevice).Assembly.GetType("PPDFramework.ShaderCommon", true)).Invoke(Device, null);
            shaderCommon.GetType().GetMethod("InitializeScreenVertex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(shaderCommon, [Device]);
            ResourceManager = new PPDFramework.Resource.ResourceManager();
        }

        public PPDDevice Device { get; }

        public PPDFramework.Resource.ResourceManager ResourceManager { get; }

        /// <summary>
        /// 黒で消してから描き、バックバッファーの色（BGRA）を返します。
        /// </summary>
        public uint[] Render(params GameComponent[] components)
        {
            drawStart.Invoke(Device, null);
            clear.Invoke(Device, [new Color4(0, 0, 0, 1)]);
            foreach (var component in components)
            {
                component.Update();
                component.Draw();
            }
            drawEnd.Invoke(Device, null);
            var pixels = new uint[Width * Height];
            var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                Device.CopyBackBuffer(handle.AddrOfPinnedObject(), Width * 4);
            }
            finally
            {
                handle.Free();
            }
            return pixels;
        }

        public static uint Pixel(uint[] pixels, int x, int y) => pixels[y * Width + x];

        /// <summary>
        /// BGRA の色を (R, G, B) にします。
        /// </summary>
        public static (int R, int G, int B) Rgb(uint bgra) => ((int)(bgra >> 16) & 0xFF, (int)(bgra >> 8) & 0xFF, (int)bgra & 0xFF);

        public void Dispose()
        {
            ResourceManager.Dispose();
            Device.Dispose();
            window.Dispose();
        }
    }

    [CollectionDefinition(Name)]
    public class GraphicsCollection : ICollectionFixture<GraphicsDevice>
    {
        public const string Name = "Graphics";
    }
}
