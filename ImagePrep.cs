using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace SSD2
{
    static class ImagePrep
    {
        public static Image<Bgr, Byte> ResizeImage(Image<Bgr, byte> sourceImage, int xRes, int yRes)
        {
            return sourceImage.Resize(xRes, yRes, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
        }

        public static Image<Bgr, Byte> GreyscaleImage(Image<Bgr, Byte> sourceImage)
        {
            //float[,] weights = new float[3, 3]
            //{
            //    { 0.3333f, 0.3333f, 0.3333f },  // "flat" weighting - may as well use the emgu version
            //    { 0.3000f, 0.5900f, 0.1100f },  // standard NTSC RGB luminence weights
            //    { 0.3086f, 0.6094f, 0.0820f }   // modified weights according to http://www.graficaobscura.com/matrix/index.html
            //};

            ColorMatrix colorMatrix = new ColorMatrix(new float[][] 
            {
                new float[] {0.3000f, 0.3000f, 0.3000f, 0, 0},
                new float[] {0.5900f, 0.5900f, 0.5900f, 0, 0},
                new float[] {0.1100f, 0.1100f, 0.1100f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });

            // apply the colourmatrix to a set of image attributes
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            // Convert the Iimage to a bmp, 
            Bitmap sourceBMP = sourceImage.ToBitmap();
            // use it to make a blank bmp to itiate a graphics object
            Bitmap tempBMP = new Bitmap(sourceBMP.Width, sourceBMP.Height);
            Graphics graphic = Graphics.FromImage(tempBMP);
            // apply the source image and the color matrix (via the attributes) to the graphics obj
            graphic.DrawImage(sourceBMP, new Rectangle(0, 0, sourceBMP.Width, sourceBMP.Height), 0, 0, sourceBMP.Width, sourceBMP.Height, GraphicsUnit.Pixel, attributes);
            return new Image<Bgr, byte>(tempBMP);
        }
    }
}
