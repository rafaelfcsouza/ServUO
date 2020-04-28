#region References
using System.Drawing.Imaging;
#endregion

namespace Ultima
{
    public static class Settings
    {
#if MONO
        public const PixelFormat PixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppPArgb;
#else
        public const PixelFormat PixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppArgb1555;
#endif
    }
}
