using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

//Created By Volkan Benzer
//https://www.linkedin.com/in/volkan-benzer-26tr34ist/


namespace GeneratingSignal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        double TestWave_Freq = 5000;
        double TestWave_Theta = (float)(Math.PI / 2.0);
        double TestWave_Fsample = 20000;
        double TestWave_Amplitude = 3.0f;

        int TestWave_SampleCount;

        ushort LOG2_FFT_SIZE = 6;
        ushort FFT_SIZE;
        float FREQUENCY_RANGE;
 
        double[] sineWave;
        double[] sampleBufferInDouble;
        short[]  sampleBufferInShort;

        fix_FFT _fix_FFT = new fix_FFT();

        private void Form1_Load(object sender, EventArgs e)
        {
            //to preventing of start the X axes from -1 
            chart1.ChartAreas[0].AxisX.Minimum = 0;            
            chart3.ChartAreas[0].AxisX.Minimum = 0;

            //chart3.ChartAreas[0].AxisX.Minimum = 0;
            //or    (dezavantajı, hep sıfırdan başlatıyor. örn işaret 5. saniyeden başlaması gerekiyorsa hatalı oluyor)
            //chart1.ChartAreas[0].AxisX.IsMarginVisible = false;
            //chart2.ChartAreas[0].AxisX.IsMarginVisible = false;

            chart3.ChartAreas[0].AxisX.LabelStyle.Angle = 45;

            trackBar2.Minimum = 0;
            trackBar2.Maximum = 16;

            trackBar1.Value = 0;
            trackBar2.Value = 0;

            numericUpDown1.Value = 5;

            textBox1.Text = "10000";
            textBox2.Text = "20000";
            textBox3.Text = "3.0";

            trackBar1.Minimum = 1;
            trackBar1.Maximum = Convert.ToInt16(textBox1.Text);
        }

        void generateSineWave(double freq, double amplitude, double timeResulation, double theta, double periodCount)
        {
            //y(t) = Asin(2πft + ϕ) = Asin(ωt + ϕ)
            // t => n

            freq = freq / timeResulation;        //freq in timeResulation Time                         
            double _1cyclePeriode = (1.0 / freq);
            double w = (2.0 * Math.PI * freq);

            int signalLenght = (int)(_1cyclePeriode * periodCount);

            sineWave = new double[signalLenght];

            for (int n = 0; n < signalLenght; n++)
            {
                sineWave[n] = amplitude * Math.Sin(w * (double)n + theta);

                chart1.Series["Continuous"].Points.AddXY(n, sineWave[n]);
            }            

            //label1.Text = _1cyclePeriode.ToString();
        }

        void getSamples(double[] signalBuffer, double fs, int sampleCount)
        {
            
            double samplePeriod = fs;//(int)((1.0 / fs) * timeResulation);   //Fs is converted to period, period is also converted in timeResulation

            sampleBufferInDouble = new double[sampleCount];

            for (int n = 0; n < sampleCount; n++)
            {
                sampleBufferInDouble[n] = signalBuffer[n + 1];  //we dont want to get first sample in t = 0

                chart1.Series["Sampled"].Points.AddXY(n + 1, sampleBufferInDouble[n]);
            }
        }

        void convertSamplesToShort(double[] sampleBuffer, double amplitude)
        {
            sampleBufferInShort = new short[sampleBuffer.Length];

            for (int n = 0; n < sampleBuffer.Length; n++)            
                sampleBufferInShort[n] = (short)((sampleBuffer[n] * 32767f) / amplitude);            
        }

        void test_FixFFT()
        {            
            
            CreateTestSignal(TestWave_Freq, TestWave_Theta, TestWave_Fsample, TestWave_Amplitude, TestWave_SampleCount);

            short[] real = new short[FFT_SIZE];
            short[] imaginary = new short[FFT_SIZE];
            int[] output = new int[FFT_SIZE >> 1];
            int max_Val = 0, max_Index = 0;


            float[] freqBand = new float[FFT_SIZE >> 1];

            for (int i = 0; i < FFT_SIZE; i++)
            {
                real[i] = sampleBufferInShort[i];
                imaginary[i] = 0;
            }

            _fix_FFT.fix_fft(real, imaginary, (short)LOG2_FFT_SIZE, 0);

            for (int i = 0; i < (FFT_SIZE >> 1); i++)  // Make frequency range table 	
                freqBand[i] = FREQUENCY_RANGE * i;

            chart3.ChartAreas[0].AxisX.Interval = FREQUENCY_RANGE;

            for (int i = 0; i < (FFT_SIZE >> 1); i++)
            {
                output[i] = (int)Math.Sqrt(Math.Abs(_fix_FFT.real[i]) + Math.Abs(_fix_FFT.imaginary[i]));

                if (max_Val < output[i])
                {
                    max_Val = output[i];
                    max_Index = i;   
                }
            }

            chart3.Series["Spectrum"].Points.DataBindXY(freqBand, output);
            
            chart3.ChartAreas[0].AxisX.LabelStyle.Angle = 45;

            label2.Text = "Freq Band: " + freqBand[max_Index].ToString() + " - " + (freqBand[max_Index] + FREQUENCY_RANGE).ToString();
        }

        void CreateTestSignal(double TestWave_Freq, double TestWave_Theta, double TestWave_Fsample, double TestWave_Amplitude, int TestWave_SampleCount)
        {
            double minPeriodCount = ((double)TestWave_SampleCount / ((double)TestWave_Fsample / (double)TestWave_Freq)) + 1;
            // You must create sufficiently long signal for sufficiently sampling. because of + 1 is that sampling doesn't start from t = 0.
            
            generateSineWave(TestWave_Freq, TestWave_Amplitude, TestWave_Fsample, TestWave_Theta, minPeriodCount);

            getSamples(sineWave, TestWave_Fsample, TestWave_SampleCount);

            convertSamplesToShort(sampleBufferInDouble, TestWave_Amplitude);
        }   

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            TestWave_Freq = trackBar1.Value;
            label7.Text = "Freq: " + TestWave_Freq.ToString();

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();

            test_FixFFT();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            TestWave_Theta = (180/ Math.PI) * (float)((Math.PI / (float)trackBar2.Maximum) * (float)(trackBar2.Value));
            label6.Text = "Theta: " + TestWave_Theta.ToString("##.##");

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            test_FixFFT();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TestWave_Fsample = Convert.ToDouble(textBox2.Text);
                FREQUENCY_RANGE = ((float)TestWave_Fsample / 2) / (FFT_SIZE / 2);
            }
            catch
            {
                FREQUENCY_RANGE = 0;
            }

            label10.Text = "FREQ_RANGE: "  + FREQUENCY_RANGE.ToString(); 
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            LOG2_FFT_SIZE = (ushort)numericUpDown1.Value;
            FFT_SIZE = (ushort)Math.Pow(2, LOG2_FFT_SIZE);
            label9.Text = "FFT Size: " + FFT_SIZE.ToString();

            TestWave_SampleCount = FFT_SIZE;

            textBox2_TextChanged(sender, e);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            TestWave_Amplitude = Convert.ToDouble(textBox3.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            trackBar1.Maximum = Convert.ToInt16(textBox1.Text);
        }
    }
}
