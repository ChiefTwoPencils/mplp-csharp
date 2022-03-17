using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using static System.Math;

namespace PointOps
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
    }
}
