using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;

namespace SSD2
{
    public partial class Main : Form
    {

        // variables (replaces a settings class)
        double sweeptime = 1.0;
        double gaptime = 0.1;
        int xRes = 80;
        int yRes = 80;

        // Used internally
        private double colTicks; // Ticks per column (sweeptime/xRes)
        private int col = -1; // Column. From 0 to xRes. -1 is flag for "stopped"
        private double startTime; // Start time (in ticks) of the cycle
        private System.Timers.Timer timer; // Timer object
        private bool stopflag = false;

        // Image stores
        protected internal Image<Bgr, Byte> sourceImage;
        protected internal Image<Bgr, Byte> finalImage;

        // Classes
        WebCam cam;
        Output output;

        // Diagnostics
        int colsProcessed = 0;
        //public Util.RRQueue colsPerStep;
        //public Util.RRQueue secsPerCycle;
        public int cycles = 0;

        // Constructor
        public Main()
        {
            // set up requisite classes
            cam = new WebCam();
            cam.Init();
            output = new Output(yRes);

            // Set up the variables
            colTicks = CalcColTicks();

            // Set the timer to fire like crazy (1ms)
            timer = new System.Timers.Timer(1);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(nextCol);

            InitializeComponent();
        }

        private int CalcColTicks()
        {
            return (int)Math.Round(sweeptime * 10000000 / xRes);
        }

        private void nextCol(object sender, ElapsedEventArgs e)
        {
            if (!stopflag)
            {
                stopflag = true;
                int colDiff = 0;
                double elapsed = DateTime.Now.Ticks - startTime;
                int _col = (int)Math.Floor(elapsed / colTicks);
                if (_col >= xRes)
                {
                    newCycle();
                    col = 0;
                    colDiff = _col - xRes;
                }
                else
                {
                    colDiff = _col - col;
                    col = _col;
                }
                //statusText.Text = cycles.ToString() + ":" + col.ToString();
                if (colDiff > 0)
                {
                    processCol(col);
                }
                stopflag = false;
            }
        }

        private void newCycle()
        {
            if (gaptime > 0)
            {
                timer.Enabled = false;
                output.Pause();
                Thread.Sleep((int)(gaptime * 1000));
            }
            double efficiency = (100 * colsProcessed / xRes);
            Console.WriteLine("{0} / {1} = {2}%", colsProcessed.ToString(), xRes, efficiency.ToString("0"));
            UpdateStatus("Running (" + efficiency.ToString() + "% efficiency)");
            sourceImage = cam.grab();
            Image<Bgr,Byte> lowRes = ImagePrep.ResizeImage(sourceImage, xRes, yRes);
            finalImage = ImagePrep.GreyscaleImage(lowRes);
            this.imageBox.Image = finalImage;
            startTime = DateTime.Now.Ticks;
            cycles++;
            colsProcessed = 0;
            if (gaptime > 0)
            {
                timer.Enabled = true;
            }
        }

        public double cycleProgress()
        {
            double progress;
            if (col == 0) progress = 0;
            if (col > 0)
            {
                progress = (double)col / (double)xRes;
            }
            else progress = -1;
            return progress;
        }

        private void processCol(int col)
        {
            if (finalImage != null)
            {
                double[] colData = new double[yRes];
                for (int i = 0; i < yRes; i++)
                {
                    try
                    {
                        // cunning trick - because the image has been "monochromed" it doesn't matter if we choose R, G, or B
                        colData[i] = finalImage.Data[i, col, 0];
                        // third index is RGB, 0 = R
                    }
                    catch
                    {
                        return;
                    }
                }
                output.useCol(colData, cycleProgress());
                colsProcessed++;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            timer.Enabled = true;
            this.newCycle();
            UpdateStatus("Running");
        }

        private void imageBox_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Image clicked");
            if (timer.Enabled)
            {
                timer.Enabled = false;
                output.Pause();
                UpdateStatus("Paused");
            }
            else
            {
                timer.Enabled = true;
            }
        }

        delegate void UpdateStatusCallback(string text);

        private void UpdateStatus(string status)
        {
            if (this.statusStrip1.InvokeRequired)
            {
                UpdateStatusCallback d = new UpdateStatusCallback(UpdateStatus);
                this.Invoke(d, new object[] { status });
            }
            else
            {
                toolStripStatusLabel1.Text = status;
            }
        }
    }
}
