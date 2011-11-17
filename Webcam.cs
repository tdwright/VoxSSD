using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows.Forms;

namespace SSD2
{
    class WebCam
    {
        public Boolean connected = false;
        private Capture webcam;

        public void Init()
        {
            if (!scanCams())
            {
                MessageBox.Show("No camera found (or too dark)");
                Application.Exit();
            }
        }

        private bool namedCam(int camIndex)
        {
            webcam = new Capture(camIndex);
            return (AvrLum(webcam) >= 10);
        }

        private bool scanCams()
        {
            int i = 0;
            bool found = false;
            while (i < 10)
            {
                webcam = new Capture(i);
                if (AvrLum(webcam) >= 10)
                {
                    Console.WriteLine(i.ToString());
                    found = true;
                    break;
                }
                i++;
            }
            return found;
        }

        private static double AvrLum(Capture webcam)
        {
            Image<Bgr, Byte> testFrame = webcam.QueryFrame();
            Image<Bgr, Byte> cloneFrame = testFrame.Clone();

            int sum = new int();
            foreach (int lum in cloneFrame.Data)
            {
                sum += lum;
            }
            Double avr = sum / (cloneFrame.Height * cloneFrame.Width * 3);
            return avr;
        }

        public Image<Bgr, Byte> grab()
        {
            Image<Bgr, Byte> frame = webcam.QueryFrame();
            return frame.Clone(); // return clone to populate .Data[,,]
        }

        ~WebCam()
        {
            if (webcam != null) webcam.Dispose();
        }
    }
}
