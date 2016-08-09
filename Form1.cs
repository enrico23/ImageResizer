using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
           
        }

        private void btnSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fld1 = new FolderBrowserDialog();
            fld1.RootFolder = Environment.SpecialFolder.Desktop;
            if (fld1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
               
            txtSource.Text = fld1.SelectedPath;
        }

        private void btnDestination_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fld2 = new FolderBrowserDialog();
            fld2.RootFolder = Environment.SpecialFolder.Desktop;
            
            if (fld2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtDestination.Text = fld2.SelectedPath;
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            string source = "";
            string destination = "";
            int canvasWidth = 0;
            int canvasHeight = 0;

            if (string.IsNullOrEmpty(txtSource.Text))
            {
                MessageBox.Show("Source is empty.");
                return;
            }

            if (string.IsNullOrEmpty(txtDestination.Text))
            {
                MessageBox.Show("Destination is empty.");
                return;
            }
            if (string.IsNullOrEmpty(txtWidth.Text))
            {
                MessageBox.Show("Canvas width is empty.");
                return;
            }
            if (string.IsNullOrEmpty(txtHeight.Text))
            {
                MessageBox.Show("Canvas height is empty.");
                return;
            }

            source = txtSource.Text;
            destination = txtDestination.Text;
            canvasWidth = Convert.ToInt32(txtWidth.Text);
            canvasHeight = Convert.ToInt32(txtHeight.Text);

            var result = imageResizer.SaveResizeImage(source, destination, canvasWidth, canvasHeight);

            foreach (var item in result) {
                txtResult.Text += item.ToString() + System.Environment.NewLine ;
            }

        }

        private void txtWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtHeight_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

       
    }

    public static class imageResizer
    {
        
        public static List<string> SaveResizeImage(string source = "", string destination = "", int canvasWidth = 0, int canvasHeight = 0)
        {
            
            List<string> files = new List<string>();
            DirectoryInfo dirImages = new DirectoryInfo(source);
            ArrayList arrImages = new ArrayList();
            arrImages.AddRange(dirImages.GetFiles("*.gif"));
            arrImages.AddRange(dirImages.GetFiles("*.jpg"));
            arrImages.AddRange(dirImages.GetFiles("*.png"));
            foreach (FileInfo file in arrImages) {
               
                var result = resizeImagePreserveRatio(file.FullName, file.Name, destination, canvasWidth, canvasHeight);
                
                files.Add(result);
            }

            return files;
        }

        private static string resizeImagePreserveRatio(string fullName = "", string fileName = "", 
            string destination = "", int canvasWidth = 0, int canvasHeight = 0)
        {
            var image = Image.FromFile(fullName);
            int originalWidth = image.Width;
            int originalHeight = image.Height;
            string fPath = destination + "\\" + fileName;
            System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            System.Drawing.Imaging.ImageCodecInfo pngEncoder = GetEncoder(ImageFormat.Png);
            System.Drawing.Imaging.ImageCodecInfo gifEncoder = GetEncoder(ImageFormat.Gif);

            /* ------------------ begin CALCULATE NEW SIZE, RATIO --------------- */

            // Calculate the ratio or percentage of increase
            double ratioX = (double)canvasWidth / (double)originalWidth;
            double ratioY = (double)canvasHeight / (double)originalHeight;
            // use whichever multiplier is smaller
            double ratio = ratioX < ratioY ? ratioX : ratioY;

            // Set the new height and width
            int newHeight = Convert.ToInt32(originalHeight * ratio);
            int newWidth = Convert.ToInt32(originalWidth * ratio);

            // Calculate the X,Y position of the upper-left corner 
            int posX = Convert.ToInt32((canvasWidth - (originalWidth * ratio)) / 2);
            int posY = Convert.ToInt32((canvasHeight - (originalHeight * ratio)) / 2);
            /* ------------- end CALCULATE NEW SIZE, RATIO ---------------- */

            /* ------------------ begin Resize and repaint  --------------- */
            System.Drawing.Bitmap canvas = new Bitmap(canvasWidth, canvasHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(canvas);
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;

            EncoderParameters encoderParameters;
            encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            
            

            if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
            {
                graphic.Clear(Color.Transparent);
                graphic.DrawImage(image, posX, posY, newWidth, newHeight);
                canvas.MakeTransparent();
                canvas.Save(fPath, pngEncoder, encoderParameters);
            }
            else 
            {
                graphic.Clear(Color.White);
                graphic.DrawImage(image, posX, posY, newWidth, newHeight);
                canvas.Save(fPath, jpgEncoder, encoderParameters);
            }
            

            /* ------------- end REsize and repaint ---------------- */

            image.Dispose();
            graphic.Dispose();
            canvas.Dispose();

            return "Image Saved : " + fPath;

           
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

    }


}
