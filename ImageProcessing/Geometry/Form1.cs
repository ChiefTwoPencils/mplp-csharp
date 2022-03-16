using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Drawing2D;

namespace Geometry
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

        }

        // Set color components less than a specified value to 0.
        private void mnuPointColorCutoff_Click(object sender, EventArgs e)
        {

        }

        // Set each pixel's red color component to 0.
        private void mnuPointClearRed_Click(object sender, EventArgs e)
        {

        }

        // Set each pixel's green color component to 0.
        private void mnuPointClearGreen_Click(object sender, EventArgs e)
        {

        }

        // Set each pixel's blue color component to 0.
        private void mnuPointClearBlue_Click(object sender, EventArgs e)
        {

        }

        // Average each pixel's color component.
        private void mnuPointAverage_Click(object sender, EventArgs e)
        {

        }

        // Convert each pixel to grayscale.
        private void mnuPointGrayscale_Click(object sender, EventArgs e)
        {

        }

        // Convert each pixel to sepia tone.
        private void mnuPointSepiaTone_Click(object sender, EventArgs e)
        {

        }

        // Apply a color tone to the image.
        private void mnuPointColorTone_Click(object sender, EventArgs e)
        {

        }

        // Set non-maximal color components to 0.
        private void mnuPointSaturate_Click(object sender, EventArgs e)
        {

        }

        #endregion Point Processes

        #region Enhancements

        private void mnuEnhancementsColor_Click(object sender, EventArgs e)
        {

        }

        // Use histogram stretching to modify contrast.
        private void mnuEnhancementsContrast_Click(object sender, EventArgs e)
        {

        }

        private void mnuEnhancementsBrightness_Click(object sender, EventArgs e)
        {

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

    }
}
