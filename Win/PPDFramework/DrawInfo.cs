using System.Numerics;
using PPDMath;
using Matrix = System.Numerics.Matrix4x4;
using Color4 = Vortice.Mathematics.Color4;
using System.Runtime.InteropServices;

namespace PPDFramework
{
    [StructLayout(LayoutKind.Sequential)]
    struct DrawInfo
    {
        public Matrix Matrix;
        public Color4 OverlayColor;
        public float Alpha;
    }
}
