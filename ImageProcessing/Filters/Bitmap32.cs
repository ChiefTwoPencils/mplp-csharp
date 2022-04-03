using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using static System.Math;

namespace Filters
{
    public class Bitmap32
    {
        // Provide public access to the picture's byte data.
        public byte[] ImageBytes;
        public int RowSizeBytes;
        public const int PixelDataSize = 32;

        public Bitmap BitMap { get; private set; }

        // A reference to the Bitmap.
        private Bitmap m_Bitmap;

        // Save a reference to the bitmap.
        public Bitmap32(Bitmap bm)
        {
            m_Bitmap = bm;
        }

        // Bitmap data.
        private BitmapData m_BitmapData;

        // Return the image's dimensions.
        public int Width
        {
            get
            {
                return m_Bitmap.Width;
            }
        }
        public int Height
        {
            get
            {
                return m_Bitmap.Height;
            }
        }

        // Provide easy access to the color values.
        public void GetPixel(int x, int y, out byte red, out byte green, out byte blue, out byte alpha)
        {
            int i = y * m_BitmapData.Stride + x * 4;
            blue = ImageBytes[i++];
            green = ImageBytes[i++];
            red = ImageBytes[i++];
            alpha = ImageBytes[i];
        }
        public void SetPixel(int x, int y, byte red, byte green, byte blue, byte alpha)
        {
            int i = y * m_BitmapData.Stride + x * 4;
            ImageBytes[i++] = blue;
            ImageBytes[i++] = green;
            ImageBytes[i++] = red;
            ImageBytes[i] = alpha;
        }
        public byte GetBlue(int x, int y)
        {
            int i = y * m_BitmapData.Stride + x * 4;
            return ImageBytes[i];
        }
        public void SetBlue(int x, int y, byte blue)
        {
            int i = y * m_BitmapData.Stride + x * 4;
            ImageBytes[i] = blue;
        }
        public byte GetGreen(int x, int y)
        {
            int i = y * m_BitmapData.Stride + x * 4;
            return ImageBytes[i + 1];
        }
        public void SetGreen(int x, int y, byte green)
        {
            int i = y * m_BitmapData.Stride + x * 4;
            ImageBytes[i + 1] = green;
        }
        public byte GetRed(int x, int y)
        {
            int i = y * m_BitmapData.Stride + x * 4;
            return ImageBytes[i + 2];
        }
        public void SetRed(int x, int y, byte red)
        {
            int i = y * m_BitmapData.Stride + x * 4;
            ImageBytes[i + 2] = red;
        }
        public byte GetAlpha(int x, int y)
        {
            int i = y * m_BitmapData.Stride + x * 4;
            return ImageBytes[i + 3];
        }
        public void SetAlpha(int x, int y, byte alpha)
        {
            int i = y * m_BitmapData.Stride + x * 4;
            ImageBytes[i + 3] = alpha;
        }

        // Lock the bitmap's data.
        public void LockBitmap()
        {
            // Lock the bitmap data.
            Rectangle bounds = new Rectangle(
                0, 0, m_Bitmap.Width, m_Bitmap.Height);
            m_BitmapData = m_Bitmap.LockBits(bounds,
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            RowSizeBytes = m_BitmapData.Stride;

            // Allocate room for the data.
            int total_size = m_BitmapData.Stride * m_BitmapData.Height;
            ImageBytes = new byte[total_size];

            // Copy the data into the ImageBytes array.
            Marshal.Copy(m_BitmapData.Scan0, ImageBytes, 0, total_size);
        }

        // Copy the data back into the Bitmap
        // and release resources.
        public void UnlockBitmap()
        {
            // Copy the data back into the bitmap.
            int total_size = m_BitmapData.Stride * m_BitmapData.Height;
            Marshal.Copy(ImageBytes, 0, m_BitmapData.Scan0, total_size);

            // Unlock the bitmap.
            m_Bitmap.UnlockBits(m_BitmapData);

            // Release resources.
            ImageBytes = null;
            m_BitmapData = null;
        }

        public static Bitmap32 operator -(Bitmap32 first, Bitmap32 second)
            => ApplyOp(first, second, (one, two) => (one - two).ToByte());

        public static Bitmap32 operator +(Bitmap32 first, Bitmap32 second)
            => ApplyOp(first, second, (one, two) => (one + two).ToByte());

        public static Bitmap32 operator *(Bitmap32 bm, float f)
        {
            byte mul(byte c, float f) => (c * f).ToByte();

            bm.LockBitmap();

            for (var x = 0; x < bm.Width; x++)
            {
                for (var y = 0; y < bm.Height; y++)
                {
                    bm.GetPixel(x, y, out var r, out var g, out var b, out var a);
                    r = mul(r, f);
                    g = mul(g, f);
                    b = mul(b, f);
                    a = mul(a, f);
                    bm.SetPixel(x, y, r, g, b, a);
                }
            }           

            bm.UnlockBitmap();
            return bm;
        }

        public static Bitmap32 operator *(float f, Bitmap32 bm) => bm * f;

        private static Bitmap32 ApplyOp(Bitmap32 first, Bitmap32 second, Func<byte, byte, byte> op)
        {
            var width = Min(first.Width, second.Width);
            var height = Min(first.Height, second.Height);
            var b32 = new Bitmap32(new Bitmap(width, height));

            first.LockBitmap();
            second.LockBitmap();
            b32.LockBitmap();

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    first.GetPixel(x, y, out var fr, out var fg, out var fb, out var fa);
                    second.GetPixel(x, y, out var sr, out var sg, out var sb, out var sa);

                    var r = op(fr, sr);
                    var g = op(fg, sg);
                    var b = op(fb, sb);
                    var a = op(fa, sa);

                    b32.SetPixel(x, y, r, g, b, a);
                }
            }

            first.UnlockBitmap();
            second.UnlockBitmap();
            b32.UnlockBitmap();

            return b32;
        }
    }
}
