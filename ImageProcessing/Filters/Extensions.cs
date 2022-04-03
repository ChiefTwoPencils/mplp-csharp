using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using static System.Math;

namespace Filters
{
    public static class Extensions
    {
        public delegate void PointOp(ref byte r, ref byte b, ref byte g, ref byte a);

        // Save the file with the appropriate format.
        public static void SaveImage(this Image image, string filename)
        {
            string extension = Path.GetExtension(filename);
            switch (extension.ToLower())
            {
                case ".bmp":
                    image.Save(filename, ImageFormat.Bmp);
                    break;
                case ".exif":
                    image.Save(filename, ImageFormat.Exif);
                    break;
                case ".gif":
                    image.Save(filename, ImageFormat.Gif);
                    break;
                case ".jpg":
                case ".jpeg":
                    image.Save(filename, ImageFormat.Jpeg);
                    break;
                case ".png":
                    image.Save(filename, ImageFormat.Png);
                    break;
                case ".tif":
                case ".tiff":
                    image.Save(filename, ImageFormat.Tiff);
                    break;
                default:
                    throw new NotSupportedException(
                        "Unknown file extension " + extension);
            }
        }

        public static Bitmap ApplyKernel(this Bitmap bm, float[,] kernel, float weight, float offset)
        {
            byte calcNew(float total) => (total / weight + offset).ToByte();

            var kRows = kernel.GetLength(0);
            var kCols = kernel.GetLength(1);

            if (kRows % 2 == 0 || kCols % 2 == 0) throw new Exception("Kernels have odd rows and columns");

            var width = bm.Width;
            var height = bm.Height;
            var @new = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(@new);
            var og32 = new Bitmap32(bm);
            var new32 = new Bitmap32(@new);

            og32.LockBitmap();
            new32.LockBitmap();
            graphics.Clear(Color.Black);

            var xradius = kRows / 2;
            var yradius = kCols / 2;
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var rTotal = 0f;
                    var gTotal = 0f;
                    var bTotal = 0f;
                    for (var dx = -xradius; dx <= xradius; dx++)
                    {
                        for (var dy = -yradius; dy <= yradius; dy++)
                        {
                            var sourceX = x + dx;
                            if (sourceX < 0) sourceX = 0;
                            if (sourceX >= width) sourceX = width - 1;

                            var sourceY = y + dy;
                            if (sourceY < 0) sourceY = 0;
                            if (sourceY >= height) sourceY = height - 1;

                            og32.GetPixel(sourceX, sourceY, out var r, out var g, out var b, out var _);

                            var scale = kernel[dy + yradius, dx + xradius];
                            rTotal += r * scale;
                            gTotal += g * scale;
                            bTotal += b * scale;
                        }
                    }                    

                    var nr = calcNew(rTotal);
                    var ng = calcNew(gTotal);
                    var nb = calcNew(bTotal);

                    new32.SetPixel(x, y, nr, ng, nb, 255);
                }
            }

            og32.UnlockBitmap();
            new32.UnlockBitmap();

            return @new;
        }

        public static Bitmap BoxBlur(this Bitmap bm, int radius)
        {
            var ones = OnesArray(radius);
            var weight = ones.GetLength(0) * ones.GetLength(1);

            return bm.ApplyKernel(ones, weight, 0);
        }

        // Perform unsharp masking.
        // sharpened = original + (original - blurred) × amount.
        public static Bitmap UnsharpMask(this Bitmap bm, int radius, float amount)
        {
            var blurred = bm.BoxBlur(radius);
            var og32 = new Bitmap32(bm);
            var blurred32 = new Bitmap32(blurred);
            var result32 = og32 + (og32 - blurred32) * amount;

            return result32.BitMap;
        }

        public static Bitmap RankFilter(this Bitmap bm, int xradius, int yradius, int rank)
        {
            var result = new Bitmap(bm.Width, bm.Height);
            var graphics = Graphics.FromImage(result);
            var o32 = new Bitmap32(bm);
            var r32 = new Bitmap32(result);

            graphics.Clear(Color.Black);
            o32.LockBitmap();
            r32.LockBitmap();

            for (var x = 0; x < o32.Width; x++)
            {
                for (var y = 0; y < o32.Height; y++)
                {
                    var pixels = new List<PixelData>();
                    for (var dx = -xradius; dx <= xradius; dx++)
                    {
                        for (var dy = -yradius; dy <= yradius; dy++)
                        {
                            var sourceX = x + dx;
                            if (sourceX < 0) sourceX = 0;
                            else if (sourceX >= bm.Width) sourceX = bm.Width -1;

                            var sourceY = y + dy;
                            if (sourceY < 0) sourceY = 0;
                            else if (sourceY >= bm.Height) sourceY = bm.Height -1;

                            o32.GetPixel(dx, dy, out var r, out var g, out var b, out var a);
                            pixels.Add(new PixelData(r, g, b, a));
                        }
                    }

                    pixels = pixels
                        .OrderBy(pixel => pixel.Brightness)
                        .ToList();

                    if (rank < 0) rank = 0;
                    if (rank > pixels.Count) rank = pixels.Count - 1;

                    var p = pixels.ElementAt(rank);
                    r32.SetPixel(x, y, p.R, p.G, p.B, p.A);
                }
            }

            o32.UnlockBitmap();
            r32.UnlockBitmap();

            return result;
        }

        public static void ApplyPointOp(this Bitmap bm, PointOp op)
        {
            var b32 = new Bitmap32(bm);
            b32.LockBitmap();

            for (var x = 0; x < bm.Width; x++)
            {
                for (var y = 0; y < bm.Height; y++)
                {
                    byte r, b, g, a;
                    b32.GetPixel(x, y, out r, out b, out g, out a);
                    op(ref r, ref b, ref g, ref a);
                    b32.SetPixel(x, y, r, b, g, a);
                }
            }

            b32.UnlockBitmap();
        }

        public static Bitmap RotateAtCenter(this Bitmap bm, float angle, Color color, InterpolationMode mode)
        {
            var copy = new Bitmap(bm.Width, bm.Height);
            var graphics = Graphics.FromImage(copy);

            graphics.Clear(color);
            graphics.InterpolationMode = mode;

            var halfWidth = copy.Width / 2;
            var halfHeight = copy.Height / 2;

            graphics.TranslateTransform(-halfWidth, -halfHeight, MatrixOrder.Append);
            graphics.RotateTransform(angle, MatrixOrder.Append);
            graphics.TranslateTransform(halfWidth, halfHeight, MatrixOrder.Append);
            graphics.DrawImage(bm, new Point(0, 0));

            return copy;
        }

        public static Bitmap Scale(this Bitmap bm, float scale, InterpolationMode mode)
            => bm.Scale(scale, scale, mode);

        public static Bitmap Scale(this Bitmap bm, float xScale, float yScale, InterpolationMode mode)
        {
            var newWidth = (int)Math.Round(bm.Width * xScale);
            var newHeight = (int)Math.Round(bm.Height * yScale);
            var copy = new Bitmap(newWidth, newHeight);
            var graphics = Graphics.FromImage(copy);

            var destPoints = new Point[] { new Point(0, 0), new Point(newWidth, 0), new Point(0, newHeight) };
            graphics.DrawImage(bm, destPoints);

            return copy;
        }

        public static Bitmap Crop(this Image image, Rectangle selection, InterpolationMode mode)
        {
            var copy = new Bitmap(selection.Width, selection.Height);            
            var points = new Point[]
            {
                new Point(0, 0),        
                new Point(selection.Width - 1, 0),
                new Point(0, selection.Height - 1)
            };

            using var graphics = Graphics.FromImage(copy);

            graphics.InterpolationMode = mode;
            graphics.DrawImage(image, points, selection, GraphicsUnit.Pixel);
            return copy;
        }

        public static Rectangle ToRectangle(this Point left, Point right)
            => new(Min(left.X, right.X), Min(left.Y, right.Y), Abs(left.X - right.X), Abs(left.Y - right.Y));   

        public static void DrawDashedRectangle(this Graphics graphics, Color first, Color second, float thickness, float dash, Point left, Point right)
        {
            var rectangle = left.ToRectangle(right);
            
            using var pen = new Pen(first, thickness);

            graphics.DrawRectangle(pen, rectangle);
            pen.DashPattern = new float[]{ dash, dash };
            pen.Color = second;
            graphics.DrawRectangle(pen, rectangle);
        }

        public static byte ToByte(this float f) => (byte) (f < 0 ? 0 : f > 255 ? 255 : Round(f));

        public static byte ToByte(this int i) => (byte) (int) ToByte((float) i);

        private static float[,] OnesArray(int radius)
        {
            var width = 2 * radius + 1;
            var array = new float[width, width];

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    array[i, j] = 1;
                }
            }

            return array;
        }
    }
}
