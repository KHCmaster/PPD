using SharpDX;
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
