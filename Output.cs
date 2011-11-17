using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Audiere.Net;

namespace SSD2
{
    class Output
    {

        OutputStream[] waves;
        AudioDevice device;
        int rows;

        int lowFreq = 250;
        int highFreq = 2500;

        public Output(int _rows)
        {
            device = new AudioDevice();
            this.rows = _rows;
            this.waves = new OutputStream[this.rows];
            int i;
            for (i = 0; i < this.rows; i++)
            {
                waves[i] = device.CreateTone(musical_constrained(i));
                waves[i].Volume = 0f;
                waves[i].Play();
            }
        }

        private double musical_constrained(int i)
        {
            float r = (float)(rows * Math.Pow(Math.Log((highFreq / lowFreq), 2), -1));
            float freq = (float)(lowFreq * Math.Pow(2, (i / r)));
            Console.WriteLine(freq.ToString("0.000"));
            return freq;
        }

        public void useCol(double[] column, double cycleProgress)
        {
            float volume;
            float pan;
            for (int i = 0; i < waves.Length; i++)
            {
                if (cycleProgress > -1)
                {
                    // column is top->bottom by default, so use i as negative index
                    volume = WeightedVolume(column[waves.Length - (i + 1)]);
                    //volume = (float)(column[rows - (i+1)] / (256 * rows * 0.5));
                    pan = (2 * (float)cycleProgress) - 1;
                }
                else
                {
                    volume = 0f;
                    pan = 0f;
                }
                waves[i].Volume = (volume);
                waves[i].Pan = pan;
            }
        }

        private float WeightedVolume(double pixel)
        {
            //if (contrastInverted) pixel = 255 - pixel;
            double doubleVal;

            int curve = 2;
            // Curve should be any integer bigger than 1
            doubleVal = 0.87 / (1 + Math.Pow(curve, (-1 * ((pixel / 12.8) - 10))));
            // Don't even ask about that 0.87 business...
            // =1/(1+POWER(B$2,(($A3/12.7)-10)*-1))

            doubleVal = (2 * doubleVal) / waves.Length;
            float returnVal = (float)doubleVal;
            //Console.WriteLine("{0}", returnVal);
            return returnVal;
        }

        public void Stop()
        {
            if (waves != null)
            {
                for (int i = 0; i < waves.Length; i++)
                {
                    waves[i].Stop();
                }
                device.Dispose();
            }
        }

        public void Pause()
        {
            if (waves != null)
            {
                for (int i = 0; i < waves.Length; i++)
                {
                    waves[i].Volume = 0;
                }
            }
        }
    }
}
