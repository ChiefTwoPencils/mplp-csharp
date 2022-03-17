using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Drawing2D;

namespace Enhancements
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Bitmap OriginalBm = null;
        private Bitmap CurrentBm = null;
        private Point StartPoint;
        private Point EndPoint;

        private void Form1_Load(object sender, EventArgs e)
        {
            // Disable menu items because no image is loaded.
            SetMenusEditable(false);
        }

        // Enable or disable menu items that are
        // appropriate when an image is loaded.
        private void SetMenusEditable(bool enabled)
        {
            ToolStripMenuItem[] items =
            {
                mnuFileSaveAs,
                mnuFileReset,
                mnuGeometry,
                mnuPointOperations,
                mnuEnhancements,
                mnuFilters,
            };
            foreach (ToolStripMenuItem item in items)
                item.Enabled = enabled;
            resultPictureBox.Visible = enabled;
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            ofdFile.Title = "Open Image File";
            ofdFile.Multiselect = false;
            if (ofdFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Bitmap bm = LoadBitmapUnlocked(ofdFile.FileName);
                    OriginalBm = bm;
                    CurrentBm = (Bitmap)OriginalBm.Clone();
                    resultPictureBox.Image = CurrentBm;

                    // Enable menu items because an image is loaded.
                    SetMenusEditable(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(
                        "Error opening file {0}.\n{1}",
                        ofdFile.FileName, ex.Message));
                }
            }
        }

        private void mnuFileSaveAs_Click(object sender, EventArgs e)
        {
            sfdFile.Title = "Save As";
            if (sfdFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    CurrentBm.SaveImage(sfdFile.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(
                        "Error saving file {0}.\n{1}",
                        sfdFile.FileName, ex.Message));
                }
            }
        }

        // Restore the original unmodified image.
        private void mnuFileReset_Click(object sender, EventArgs e)
        {
            CurrentBm = (Bitmap)OriginalBm.Clone();
            resultPictureBox.Image = CurrentBm;
        }

        // Make a montage of files.
        private void mnuFileMontage_Click(object sender, EventArgs e)
        {
            // Let the user select the files.
            ofdFile.Title = "Select Montage Files";
            ofdFile.Multiselect = true;
            if (ofdFile.ShowDialog() == DialogResult.OK)
            {
                OriginalBm = MakeMontage(ofdFile.FileNames, Color.Black);
                CurrentBm = (Bitmap)OriginalBm.Clone();
                resultPictureBox.Image = CurrentBm;

                // Enable menu items because an image is loaded.
                SetMenusEditable(true);
            }
        }

        // Make a montage of files, four per row.
        private Bitmap MakeMontage(string[] filenames, Color bgColor)
        {
            var rows = (int) Math.Ceiling(filenames.Length / 4.0);
            var maxWidth = 0;
            var maxHeight = 0;
            var bms = new List<Bitmap>();
            foreach (var filename in filenames)
            {
                var bm = LoadBitmapUnlocked(filename);
                maxWidth = Math.Max(maxWidth, bm.Width);
                maxHeight = Math.Max(maxHeight, bm.Height);
                bms.Add(bm);
            }

            var widthMultiplier = bms.Count < 4 ? bms.Count : 4;
            var result = new Bitmap(maxWidth * widthMultiplier, maxHeight * rows);
            var graphics = Graphics.FromImage(result);
            graphics.Clear(bgColor);

            var x = -maxWidth;
            var y = -maxHeight;
            for (var i = 0; i < bms.Count; i++)
            {
                if (i % 4 == 0)
                {
                    x = 0;
                    y += maxHeight;
                }
                else
                {
                    x += maxWidth;
                }

                var bm = bms[i];
                graphics.DrawImage(bm, new Rectangle(x, y, bm.Width, bm.Height));
            }

            return result;
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Load a bitmap without locking it.
        private Bitmap LoadBitmapUnlocked(string file_name)
        {
            using (Bitmap bm = new Bitmap(file_name))
            {
                return new Bitmap(bm);
            }
        }

        // Rotate the image.
        private void mnuGeometryRotate_Click(object sender, EventArgs e)
        {
            var angle = GetFloat(float.NegativeInfinity, float.PositiveInfinity);

            if (angle == float.NaN) return;

            CurrentBm = CurrentBm.RotateAtCenter(angle, Color.Black, InterpolationMode.High);
            SetResultToCurrentImage();
        }

        // Scale the image uniformly.
        private void mnuGeometryScale_Click(object sender, EventArgs e)
        {
            var scale = GetFloat(0.01f, 1.0f);

            if (scale == float.NaN) return;

            CurrentBm = CurrentBm.Scale(scale, InterpolationMode.High);
            SetResultToCurrentImage();
        }

        private void mnuGeometryStretch_Click(object sender, EventArgs e)
        {
            var input = InputForm.GetString("String input!", "Enter a valid string value for your command!", "0, 0");

            var scales = input
                .Split(',', ' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(float.Parse)
                .ToList();

            if (scales.Any(s => s <= 0.0)) return;

            CurrentBm = CurrentBm.Scale(scales[0], scales[1], InterpolationMode.High);
            SetResultToCurrentImage();
        }

        private void mnuGeometryRotateFlip_Click(object sender, EventArgs e)
        {
            var form = new InputForm();
            form.Width = 300;
            form.Height = 300;
            form.valueTextBox.Text = "0";
            form.captionLabel.Text = string.Join("\n", new string[]
            {
                "Flip Horizontal",
                "Flip Vertical",
                "Rotate 90",
                "Rotate 180",
                "Rotate 270"
            }
            .Select((s, i) => $"{i + 1}) {s}"));
                
            if (form.ShowDialog() == DialogResult.OK)
            {
                if (int.TryParse(form.valueTextBox.Text, out var @int))
                {
                    var option = @int switch
                    {
                        1 => RotateFlipType.RotateNoneFlipX,
                        2 => RotateFlipType.RotateNoneFlipY,
                        3 => RotateFlipType.Rotate90FlipNone,
                        4 => RotateFlipType.Rotate180FlipNone,
                        5 => RotateFlipType.Rotate270FlipNone,
                        _ => RotateFlipType.RotateNoneFlipNone
                    };

                    CurrentBm.RotateFlip(option);
                    SetResultToCurrentImage();
                }                
            }
        }       

        private float GetFloat(float min, float max, string @default = "0")
            => InputForm.GetFloat("Float input", "Enter a float value!", @default, min, max, "The value is invalid!");

        private void SetResultToCurrentImage() => resultPictureBox.Image = CurrentBm;

        #region Cropping

        // Let the user select an area and crop to that area.
        private void mnuGeometryCrop_Click(object sender, EventArgs e)
        {
            resultPictureBox.MouseDown += crop_MouseDown;
            resultPictureBox.Cursor = Cursors.Cross;
        }

        // Let the user select an area with a desired
        // aspect ratio and crop to that area.
        private void mnuGeometryCropToAspect_Click(object sender, EventArgs e)
        {

        }

        private void crop_MouseDown(object? sender, MouseEventArgs e)
        {
            resultPictureBox.MouseDown -= crop_MouseDown;
            resultPictureBox.MouseMove += crop_MouseMove;
            resultPictureBox.MouseUp += crop_MouseUp;
            resultPictureBox.Paint += resultPictureBox_Paint;

            StartPoint = EndPoint = e.Location;
        }

        private void crop_MouseMove(object? sender, MouseEventArgs e)
        {
            EndPoint = e.Location;
            resultPictureBox.Refresh();
        }

        private void crop_MouseUp(object? sender, MouseEventArgs e)
        {
            resultPictureBox.Cursor = Cursors.Default;
            resultPictureBox.MouseMove -= crop_MouseMove;
            resultPictureBox.MouseUp -= crop_MouseUp;
            resultPictureBox.Paint -= resultPictureBox_Paint;

            CurrentBm = CurrentBm.Crop(StartPoint.ToRectangle(EndPoint), InterpolationMode.High);
            SetResultToCurrentImage();
        }

        private void resultPictureBox_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawDashedRectangle(Color.White, Color.Red, 2, 2, StartPoint, EndPoint);
            resultPictureBox.Refresh();
        }

        #endregion Cropping

        #region Point Processes

        // Set each color component to 255 - the original value.
        private void mnuPointInvert_Click(object sender, EventArgs e)
        {
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte _) =>
            {
                r = (byte) (255 - r);
                b = (byte) (255 - b);
                g = (byte) (255 - g);
            });

            resultPictureBox.Refresh();
        }

        // Set color components less than a specified value to 0.
        private void mnuPointColorCutoff_Click(object sender, EventArgs e)
        {            
            var @int = InputForm.GetInt("Integer input!", "Enter an integer!", "0", 0, 255, "The value is invalid!");
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte a) =>
            {
                byte Cut(byte x) => x < @int ? (byte) 0 : x;
                r = Cut(r);
                b = Cut(b);
                g = Cut(g); 
                a = Cut(a);
            });

            resultPictureBox.Refresh();
        }

        // Set each pixel's red color component to 0.
        private void mnuPointClearRed_Click(object sender, EventArgs e)
        {
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte a) => r = (byte)0);

            resultPictureBox.Refresh();
        }

        // Set each pixel's green color component to 0.
        private void mnuPointClearGreen_Click(object sender, EventArgs e)
        {
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte a) => g = (byte)0);

            resultPictureBox.Refresh();
        }

        // Set each pixel's blue color component to 0.
        private void mnuPointClearBlue_Click(object sender, EventArgs e)
        {
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte a) => b = (byte)0);

            resultPictureBox.Refresh();
        }

        // Average each pixel's color component.
        private void mnuPointAverage_Click(object sender, EventArgs e)
        {
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte a) 
                => r = b = g = (byte)((r + g + b) / 3));

            resultPictureBox.Refresh();
        }

        // Convert each pixel to grayscale.
        private void mnuPointGrayscale_Click(object sender, EventArgs e)
        {
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte a) 
                => r = b = g = (byte)(r * 0.3 + g * 0.5 + b * 0.2));

            resultPictureBox.Refresh();
        }

        // Convert each pixel to sepia tone.
        private void mnuPointSepiaTone_Click(object sender, EventArgs e)
        {
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte a) =>
            {
                byte limit(float f) => (byte)(f > 255 ? 255 : f);

                float nr, ng, nb;
                nr = r * 0.393f + g * 0.769f + b * 0.189f;
                nb = r * 0.349f + g * 0.686f + b * 0.168f;
                ng = r * 0.272f + g * 0.534f + b * 0.131f;

                r = limit(nr);
                g = limit(ng);
                b = limit(nb);
            });

            resultPictureBox.Refresh();
        }

        // Apply a color tone to the image.
        private void mnuPointColorTone_Click(object sender, EventArgs e)
        {
            if (cdColorTone.ShowDialog() == DialogResult.OK)
            {
                var color = cdColorTone.Color;
                (float nr, float ng, float nb) = (color.R, color.G, color.B);

                CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte a) =>
                {
                    var brightness = (r + g + b) / (3f * 255f);
                    byte Tone(float f) => (byte)(brightness * f);
                    r = Tone(nr);
                    g = Tone(ng);
                    b = Tone(nb);
                });

                resultPictureBox.Refresh();
            }
        }

        // Set non-maximal color components to 0.
        private void mnuPointSaturate_Click(object sender, EventArgs e)
        {
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte a) =>
            {
                var max = Math.Max(r, Math.Max(g, b));
                byte ZeroNotMax(byte x) => x == max ? max : (byte)0; 

                r = ZeroNotMax(r);
                g = ZeroNotMax(g);
                b = ZeroNotMax(b);
            });

            resultPictureBox.Refresh();
        }

        #endregion Point Processes

        #region Enhancements

        private void mnuEnhancementsColor_Click(object sender, EventArgs e)
        {
            var factor = GetFloat(0f, 2f, "1.0");
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte _) =>
            {
                RgbToHls(r, g, b, out double h, out double l, out double s);
                s = AdjustValue(l, factor);

                HlsToRgb(h, l, s, out int ir, out int ig, out int ib);
                r = (byte)ir;
                g = (byte)ig;
                b = (byte)ib;
            });

            resultPictureBox.Refresh();
        }

        // Use histogram stretching to modify contrast.
        private void mnuEnhancementsContrast_Click(object sender, EventArgs e)
        {
            var factor = GetFloat(0.00001f, float.MaxValue, "1.0");
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte _) =>
            {
                var half = 128;
                byte adjust(byte x) => (((x - half) * factor) + half).ToByte();

                r = adjust(r);  
                g = adjust(g);
                b = adjust(b);
            });

            resultPictureBox.Refresh();
        }

        private void mnuEnhancementsBrightness_Click(object sender, EventArgs e)
        {
            var factor = GetFloat(0f, 2f, "1.0");
            CurrentBm.ApplyPointOp((ref byte r, ref byte g, ref byte b, ref byte _) =>
            {
                RgbToHls(r, g, b, out double h, out double l, out double s);
                l = AdjustValue(l, factor);

                HlsToRgb(h, l, s, out int ir, out int ig, out int ib);
                r = (byte)ir;
                g = (byte)ig;
                b = (byte)ib;
            });

            resultPictureBox.Refresh();
        }

        // Adjust the value closer to 0 or 1.
        // Factor should be between 0 and 2.
        // 0 <= factor < 1 adjusts towards 0.
        // 1 < factor <= 2 adjusts towards 1.
        private double AdjustValue(double value, double factor)
        {
            if (0 <= factor && factor < 1)
            {
                // Adjust towards 0
                value *= factor;
            }
            else if (1 < factor && factor <= 2)
            {
                // Adjust towards 1
                var oneMinusValue = 1 - value;
                var otherFactor = 2 - factor;
                var oneMinusValueTimesOtherFactor = oneMinusValue * otherFactor;
                value = 1 - oneMinusValueTimesOtherFactor;
            }

            return value;
        }

        public static void RgbToHls(int r, int g, int b, out double h, out double l, out double s)
        {
            // Convert RGB to a 0.0 to 1.0 range.
            double double_r = r / 255.0;
            double double_g = g / 255.0;
            double double_b = b / 255.0;

            // Get the maximum and minimum RGB components.
            double max = double_r;
            if (max < double_g) max = double_g;
            if (max < double_b) max = double_b;

            double min = double_r;
            if (min > double_g) min = double_g;
            if (min > double_b) min = double_b;

            double diff = max - min;
            l = (max + min) / 2;
            if (Math.Abs(diff) < 0.00001)
            {
                s = 0;
                h = 0;  // H is really undefined.
            }
            else
            {
                if (l <= 0.5) s = diff / (max + min);
                else s = diff / (2 - max - min);

                double r_dist = (max - double_r) / diff;
                double g_dist = (max - double_g) / diff;
                double b_dist = (max - double_b) / diff;

                if (double_r == max) h = b_dist - g_dist;
                else if (double_g == max) h = 2 + r_dist - b_dist;
                else h = 4 + g_dist - r_dist;

                h = h * 60;
                if (h < 0) h += 360;
            }
        }

        // Convert an HLS value into an RGB value.
        public static void HlsToRgb(double h, double l, double s, out int r, out int g, out int b)
        {
            double p2;
            if (l <= 0.5) p2 = l * (1 + s);
            else p2 = l + s - l * s;

            double p1 = 2 * l - p2;
            double double_r, double_g, double_b;
            if (s == 0)
            {
                double_r = l;
                double_g = l;
                double_b = l;
            }
            else
            {
                double_r = QqhToRgb(p1, p2, h + 120);
                double_g = QqhToRgb(p1, p2, h);
                double_b = QqhToRgb(p1, p2, h - 120);
            }

            // Convert RGB to the 0 to 255 range.
            r = (int)(double_r * 255.0);
            g = (int)(double_g * 255.0);
            b = (int)(double_b * 255.0);
        }

        private static double QqhToRgb(double q1, double q2, double hue)
        {
            if (hue > 360) hue -= 360;
            else if (hue < 0) hue += 360;

            if (hue < 60) return q1 + (q2 - q1) * hue / 60;
            if (hue < 180) return q2;
            if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
            return q1;
        }

        #endregion Enhancements

        #region Filters

        private void mnuFiltersBoxBlur_Click(object sender, EventArgs e)
        {

        }

        private void mnuFiltersUnsharpMask_Click(object sender, EventArgs e)
        {

        }

        private void mnuFiltersRankFilter_Click(object sender, EventArgs e)
        {

        }

        private void mnuFiltersMedianFilter_Click(object sender, EventArgs e)
        {

        }

        private void mnuFiltersMinFilter_Click(object sender, EventArgs e)
        {

        }

        private void mnuFiltersMaxFilter_Click(object sender, EventArgs e)
        {

        }

        // Display a dialog where the user can select
        // and modify a default kernel.
        // If the user clicks OK, apply the kernel.
        private void mnuFiltersCustomKernel_Click(object sender, EventArgs e)
        {

        }

        #endregion Filters

        private void sfdFile_FileOk(object sender, CancelEventArgs e)
        {

        }
    }
}
