using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace FFT_Simulator
{
    internal class SignalInput
    {
        public string signalName;
        public float Freq;
        public float Theta;
        public float Amplitude;
        
        public bool Activate = true;

        public int timeResulation;

        public float[] ContiniousTimeSamples;
        public float[] DiscreteTimeSamples;

        public static int ObjectCount = 0;

        public SignalInput(int timeResulation)
        {
            this.timeResulation = timeResulation;

            ObjectCount++;
        }

        public void setTimeResulation(int val)
        {
            timeResulation = val;
        }

        public void generateSineWave(float sampleFreq, int sampleCount)
        {
            //y(t) = Asin(2πft + ϕ) = Asin(ωt + ϕ)

            float sampleStep = Freq / (sampleFreq * (float)timeResulation);
            float w = (float)(2.0 * Math.PI * sampleStep);

            ContiniousTimeSamples = new float[sampleCount * timeResulation];

            for (int t = 0; t < (sampleCount * timeResulation); t++)            
                ContiniousTimeSamples[t] = (float)(Amplitude * Math.Sin((w * (float)t) + Theta * (Math.PI / 180f)));
            
        }

        public void getSamples(float sampleFreq, int sampleCount)
        {
            //y(t) = Asin(2πft + ϕ) = Asin(ωt + ϕ)
            // t => n
            float sampleStep = Freq / sampleFreq;

            float w = (float)(2.0 * Math.PI * sampleStep);

            DiscreteTimeSamples = new float[sampleCount];

            for (int n = 0; n < sampleCount; n++)
                DiscreteTimeSamples[n] = (float)(Amplitude * Math.Sin(w * (float)n + Theta * (Math.PI / 180f)));

        }

        public void createSineWave(float sampleFreq, int sampleCount)
        {
            generateSineWave(sampleFreq, sampleCount);
            getSamples(sampleFreq, sampleCount);
        }
    }
}
