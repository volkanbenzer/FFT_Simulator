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


namespace GeneratingSignal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        double TestWave_Freq = 5000;
        double TestWave_Theta = 0;
        double TestWave_Fsample = 20000;
        double TestWave_Amplitude = 3.0;

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

            numericUpDown1.Value = 6;

            textBox1.Text = "10000";
            textBox2.Text = TestWave_Fsample.ToString();
            textBox3.Text = TestWave_Amplitude.ToString("#.#");

            trackBar1.Minimum = 1;
            trackBar1.Maximum = Convert.ToInt16(textBox1.Text);

            trackBar2.Value = 0;
            trackBar1.Value = 1;
        }

        void generateSineWave(double freq, double amplitude, double timeResulation, double theta, double periodCount)
        {
            //y(t) = Asin(2πft + ϕ) = Asin(ωt + ϕ)
            // t => n
            double freqInTimeResulation = freq * timeResulation;        //freq in timeResulation Time    
                    
            double _1cyclePeriode = (1.0 / freqInTimeResulation);
            double w = (2.0 * Math.PI * freqInTimeResulation);

            int signalLenght = (int)Math.Ceiling(_1cyclePeriode * periodCount) + 1;

            sineWave = new double[signalLenght];

            //if (signalLenght == 64)
                //MessageBox.Show(_1cyclePeriode.ToString() + " " + periodCount.ToString() + " " + ((int)_1cyclePeriode * periodCount).ToString());

            for (int t = 0; t < signalLenght; t++)
            {
                sineWave[t] = amplitude * Math.Sin(w * (double)t + theta);

                chart1.Series["Continuous"].Points.AddXY(t, sineWave[t]); 
            }            

            //label1.Text = _1cyclePeriode.ToString();
        }

        void getSamples(double[] signalBuffer, double fs, double timeResulation, int sampleCount)
        {
            
            double samplePeriod = fs * timeResulation;
            int n = 0, t_delta = 0;

            sampleBufferInDouble = new double[sampleCount];

            //listBox1.Items.Clear();
            for (int t = 0; t < sampleCount; t++)
            {
                if (t_delta >= samplePeriod)
                {
                    sampleBufferInDouble[n] = signalBuffer[t];
                    chart1.Series["Sampled"].Points.AddXY(t, sampleBufferInDouble[n]);                                       
                    n++;
                }

                t_delta = t - t_delta;
                //listBox1.Items.Add(sampleBufferInDouble[n]);
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

            listBox1.Items.Clear();
            for (int i = 0; i < (FFT_SIZE >> 1); i++)
            {
                //output[i] = (int)Math.Sqrt((_fix_FFT.real[i] * _fix_FFT.real[i]) + (_fix_FFT.imaginary[i] * _fix_FFT.imaginary[i]));
                output[i] = Math.Abs(_fix_FFT.real[i]) + Math.Abs(_fix_FFT.imaginary[i]);

                listBox1.Items.Add(output[i].ToString() + " " + _fix_FFT.real[i].ToString() + " " + _fix_FFT.imaginary[i].ToString());

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
            double minPeriodCount = ((double)TestWave_SampleCount / ((double)TestWave_Fsample / (double)TestWave_Freq));
            // You must create sufficiently long signal for sufficiently sampling. because of + 1 is that sampling doesn't start from t = 0.
            
            generateSineWave(TestWave_Freq, TestWave_Amplitude, (1.0 / TestWave_Fsample), TestWave_Theta, minPeriodCount);

            getSamples(sineWave, TestWave_Fsample, (1.0 / TestWave_Fsample), TestWave_SampleCount);

            convertSamplesToShort(sampleBufferInDouble, TestWave_Amplitude);
        }   

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            TestWave_Freq = trackBar1.Value;
            label7.Text = "Freq(Hz): " + TestWave_Freq.ToString();

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
