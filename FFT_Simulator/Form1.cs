using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace FFT_Simulator
{    
    public partial class Form1 : Form
    {
        public static Form1 instance;

        public Form1()
        {
            InitializeComponent();

            instance = this;
        }

        fix_FFT _fix_FFT = new fix_FFT();

        SignalInput _signalInput;
        List<SignalInput> signalSeries;

        UserInput UserInput1;

        float Max_Frequency;
        float F_SAMPLE;

        ushort LOG2_FFT_SIZE;
        public ushort FFT_SIZE;
        float FREQUENCY_RANGE;

        int[] FFT_Output;
        float[] freqBand;

        readonly float defultSignal_Freq = 500;
        readonly float defultSignal_Theta = 0;
        readonly float defultSignal_Amplitude = 1.0f;

        readonly int continuousTimeResulation = 10;

        ushort ADC_Resolation;
        float ADC_Ref;

        double[] combinedSignal;
        int[] signalInInt;
        int[] ADC_Buffer;

        int signalControlIndex;  

        bool eventHandler_Enable;
        
        bool userInputEnable = false;
        float signalInput0_FreqTemp;

        bool windowing_Status = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new Size(1050, 700);
            this.Location = new System.Drawing.Point(0, 0);

            _signalInput = new SignalInput(continuousTimeResulation);
            signalSeries = new List<SignalInput>();

            eventHandler_Enable = false;
            //*******************************Controller Settings**********************************
            signalChart.ChartAreas[0].AxisX.Minimum = 0;
            combinedChart.ChartAreas[0].AxisX.Minimum = 0;
            spectrumChart.ChartAreas[0].AxisX.Minimum = 0;

            spectrumChart.ChartAreas[0].AxisX.LabelStyle.Angle = 45;

            trackBar2.Minimum = 0;
            trackBar2.Maximum = 36;           
            
            trackBar1.Minimum = 1;
            
            
            spectrumChart.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
            //***********************************************************************************
            Max_Frequency = 10000;
            F_SAMPLE = 20000;
            LOG2_FFT_SIZE = 6;

            trackBar1.Maximum = Convert.ToInt16(Max_Frequency);

            ADC_Ref = 3.3f;
            ADC_Resolation = (ushort)((Math.Pow(2, 16) / 2) - 1);

            _signalInput.Freq = defultSignal_Freq;
            _signalInput.Theta = defultSignal_Theta;
            _signalInput.Amplitude = defultSignal_Amplitude;
            _signalInput.Activate = Enabled;
            _signalInput.signalName = "Signal 0";
            //****************************************************************************
            textBox1.Text = Max_Frequency.ToString();
            textBox2.Text = F_SAMPLE.ToString();

            numericUpDown1.Value = LOG2_FFT_SIZE;
            FFT_SIZE = (ushort)Math.Pow(2, LOG2_FFT_SIZE);
            label9.Text = "FFT Size: " + FFT_SIZE.ToString();

            createFreqRangeTable(F_SAMPLE, FFT_SIZE);
            label10.Text = "FREQ_RANGE: " + FREQUENCY_RANGE.ToString();

            numericUpDown2.Value = (decimal)ADC_Ref;
            numericUpDown3.Value = (decimal)(Math.Log((ADC_Resolation + 1)*2, 2));

            trackBar1.Value = (int)_signalInput.Freq;
            trackBar2.Value = ThetaTotrackBar(_signalInput.Theta);
            label7.Text = "Freq(Hz): " + _signalInput.Freq.ToString();
            label6.Text = "Theta: " + string.Format("{0:0}", _signalInput.Theta);

            numericUpDown4.Value = (decimal)_signalInput.Amplitude;

            checkBox3.Checked = _signalInput.Activate;
            textBox5.Text = _signalInput.signalName;

            //*********************************************************************

            createNewSignal("Signal 0", defultSignal_Freq, defultSignal_Amplitude, defultSignal_Theta, Enabled);
            signalSeries.Add(_signalInput);

            combinedSignal = combinationOfSignals(signalSeries);
            ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
            ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);
            test_FixFFT();

            //*********************************************************************
            ChartsUpdate(FFT_SIZE, 0);

            listBox1.Items.Add(_signalInput.signalName);
            listBox1.SelectedIndex = 0;

            eventHandler_Enable = true;

            


        }
        private double[] combinationOfSignals(List<SignalInput> signals)
        {            
            int bufferLenght = signals[0].DiscreteTimeSamples.Length;
            double[] sumOfSignals = new double[bufferLenght];

            if (userInputEnable == false)
            {

                for (int j = 0; j < signals.Count; j++)
                {
                                        
                    if (signals[j].Activate == true)
                    {
                        for (int i = 0; i < bufferLenght; i++)
                        {
                            sumOfSignals[i] += signals[j].DiscreteTimeSamples[i];
                        }
                    }
                }
            }

            return sumOfSignals;
        }
         

        int[] ADC_Conversation(double[] combinedsignal, float adc_ref, ushort adc_resolation)
        {
            int[] adc_buffer = new int[combinedsignal.Length];

            bool ampOutOfRange_Flag = false;

            for (int i = 0; i < combinedsignal.Length; i++)
            {
                if (combinedsignal[i] > 0)
                {
                    if (combinedsignal[i] > adc_ref)
                    {
                        adc_buffer[i] = (int)(adc_resolation);
                        ampOutOfRange_Flag = true; 
                    }
                    else
                        adc_buffer[i] = (int)((combinedsignal[i] * adc_resolation) / adc_ref);
                }
                else
                {
                    if (combinedsignal[i] < (-1 * adc_ref))
                    {
                        adc_buffer[i] = -1 * adc_resolation;
                        ampOutOfRange_Flag = true;
                    }
                    else
                        adc_buffer[i] = (int)((combinedsignal[i] * adc_resolation) / adc_ref);
                }


                if (Math.Abs(combinedsignal[i]) < adc_ref)
                    adc_buffer[i] = (int)((combinedsignal[i] * adc_resolation) / adc_ref);
                else
                    adc_buffer[i] = adc_resolation;
            }

            if (ampOutOfRange_Flag == true)
                label17.Text = "The combined signal Vpeak is higher than ADC Reference Voltage!";
            else
                label17.Text = "";

            return adc_buffer;
        }

        void test_FixFFT()
        {
            short[] real = new short[FFT_SIZE];
            short[] imaginary = new short[FFT_SIZE];
            FFT_Output = new int[FFT_SIZE >> 1];

            int max_Val = 0, max_Index = 0;

            for (int i = 0; i < FFT_SIZE; i++)
            {
                //real[i] = (short)((combinedSignal[i] * 32767f) / 3.0f);
                real[i] = (short)(ADC_Buffer[i]);
                imaginary[i] = 0;
            }

            _fix_FFT.fix_fft(real, imaginary, (short)LOG2_FFT_SIZE, 0);

            for (int i = 0; i < (FFT_SIZE >> 1); i++)
            {
                FFT_Output[i] = (int)Math.Sqrt((_fix_FFT.real[i] * _fix_FFT.real[i]) + (_fix_FFT.imaginary[i] * _fix_FFT.imaginary[i]));

                if (max_Val < FFT_Output[i])
                {
                    max_Val = FFT_Output[i];
                    max_Index = i;   
                }
            }
            
            label2.Text = "Max Output Frequency Range:" + Environment.NewLine + freqBand[max_Index].ToString("#.#") + " - " + (freqBand[max_Index] + FREQUENCY_RANGE).ToString("#.#");
                    
        }

        private void ChartsUpdate(int sampleCount, int signalIndex)
        {
            signalChart.Series["Continuous"].Points.Clear();
            signalChart.Series["Sampling"].Points.Clear();
            combinedChart.Series["Combination"].Points.Clear();

            spectrumChart.Series["Spectrum"].Points.Clear();
                        
            for (int t = 0; t < (sampleCount * continuousTimeResulation); t++)            
                signalChart.Series["Continuous"].Points.AddXY(t, signalSeries[signalControlIndex].ContiniousTimeSamples[t]); 

            for (int n = 0; n < sampleCount; n++)            
                signalChart.Series["Sampling"].Points.AddXY(n * continuousTimeResulation, signalSeries[signalControlIndex].DiscreteTimeSamples[n]);
            
            for (int t = 0; t < sampleCount; t++)            
                combinedChart.Series["Combination"].Points.AddXY(t * continuousTimeResulation, ADC_Buffer[t]);
                                    
            spectrumChart.Series[0].Points.DataBindXY(freqBand, FFT_Output);

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (eventHandler_Enable == true)
            {
                signalSeries[signalControlIndex].Freq = trackBar1.Value;
                label7.Text = "Freq(Hz): " + signalSeries[signalControlIndex].Freq.ToString();

                signalSeries[signalControlIndex].createSineWave(F_SAMPLE, FFT_SIZE);

                combinedSignal = combinationOfSignals(signalSeries);
                ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                test_FixFFT();

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {            
            if (eventHandler_Enable == true)
            {
                signalSeries[signalControlIndex].Theta = trackBarToTheta();
                signalSeries[signalControlIndex].createSineWave(F_SAMPLE, FFT_SIZE);

                label6.Text = "Theta: " + string.Format("{0:0}", signalSeries[signalControlIndex].Theta);

                combinedSignal = combinationOfSignals(signalSeries);
                ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }

            
        }
        private float trackBarToTheta()
        {
            return 180f * ((float)(trackBar2.Value) / (float)trackBar2.Maximum);
        }

        private int ThetaTotrackBar(float theta)
        {
            return (int)((theta * (float)trackBar2.Maximum) / 180f);
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                F_SAMPLE = (float)Convert.ToDouble(textBox2.Text);

                createFreqRangeTable(F_SAMPLE, FFT_SIZE);

                if (eventHandler_Enable == true)
                {
                    signalsUpdate();

                    combinedSignal = combinationOfSignals(signalSeries);
                    ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                    ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                    test_FixFFT();

                    ChartsUpdate(FFT_SIZE, signalControlIndex);
                }

            }
            catch
            {
                //FREQUENCY_RANGE = 0;
            }

            label10.Text = "FREQ_RANGE: "  + FREQUENCY_RANGE.ToString();

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (eventHandler_Enable == true)
            {
                LOG2_FFT_SIZE = (ushort)numericUpDown1.Value;
                FFT_SIZE = (ushort)Math.Pow(2, LOG2_FFT_SIZE);
                label9.Text = "FFT Size: " + FFT_SIZE.ToString();

                createFreqRangeTable(F_SAMPLE, FFT_SIZE);
                label10.Text = "FREQ_RANGE: " + FREQUENCY_RANGE.ToString();

                signalsUpdate();

                combinedSignal = combinationOfSignals(signalSeries);
                ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                test_FixFFT();

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (eventHandler_Enable == true)
            {
                signalSeries[signalControlIndex].Amplitude = (float)Convert.ToDouble(numericUpDown4.Value);

                signalSeries[signalControlIndex].createSineWave(F_SAMPLE, FFT_SIZE);

                combinedSignal = combinationOfSignals(signalSeries);
                ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                test_FixFFT();

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int newSignalIndex = signalSeries.Count;

            createNewSignal("NewSignal_" + SignalInput.ObjectCount, trackBar1.Value, (float)Convert.ToDouble(numericUpDown4.Value), trackBarToTheta(), Enabled);
                        
            signalSeries.Add(_signalInput);

            combinedSignal = combinationOfSignals(signalSeries);
            ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
            ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

            test_FixFFT();

            eventHandler_Enable = false;
            listBox1.Items.Add(signalSeries[newSignalIndex].signalName);

            textBox5.Text = signalSeries[newSignalIndex].signalName;
            
            eventHandler_Enable = true;
            listBox1.SelectedIndex = signalSeries.Count - 1;
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (eventHandler_Enable == true)
            {
                signalControlIndex = getSignalIndex(listBox1.SelectedItem.ToString());

                groupBox1.Text = "Input Signal: " + signalSeries[signalControlIndex].signalName;

                setInputControls();

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }
        }

        void createNewSignal(string signalName, float freq, float amp, float theta, bool actv)
        {
            _signalInput = new SignalInput(continuousTimeResulation);

            _signalInput.signalName = signalName;
            _signalInput.Freq = freq;
            _signalInput.Amplitude = amp;
            _signalInput.Theta = theta;
            _signalInput.Activate = actv;

            _signalInput.createSineWave(F_SAMPLE, FFT_SIZE);
        }

        void setInputControls()
        {
            eventHandler_Enable = false;

            trackBar1.Value = (int)signalSeries[signalControlIndex].Freq;// (int)_signalInput.Freq;
            trackBar2.Value = ThetaTotrackBar(signalSeries[signalControlIndex].Theta);
            label7.Text = "Freq(Hz): " + signalSeries[signalControlIndex].Freq.ToString();
            label6.Text = "Theta: " + string.Format("{0:0}", signalSeries[signalControlIndex].Theta);

            numericUpDown4.Value = (decimal)signalSeries[signalControlIndex].Amplitude;

            checkBox3.Checked = signalSeries[signalControlIndex].Activate;

            textBox5.Text = signalSeries[signalControlIndex].signalName;
            eventHandler_Enable = true;
        }

        void selectSignalActvities(int signalIndex)
        {
            if(userInputEnable == false)
                signalSeries[signalIndex].createSineWave(F_SAMPLE, FFT_SIZE);

            combinedSignal = combinationOfSignals(signalSeries);
            ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
            ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

            test_FixFFT();

            ChartsUpdate(FFT_SIZE, signalIndex);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {    
            if(eventHandler_Enable == true)
            {
                signalSeries[signalControlIndex].Activate = checkBox3.Checked;
                signalSeries[signalControlIndex].createSineWave(F_SAMPLE, FFT_SIZE);

                combinedSignal = combinationOfSignals(signalSeries);
                ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                test_FixFFT();

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }                
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int deleteIndex = signalControlIndex;

            if (signalSeries.Count > 1)
            {
                signalSeries.RemoveAt(deleteIndex);

                eventHandler_Enable = false;
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);

                 if (deleteIndex < 2)
                     listBox1.SelectedIndex = 0;
                 else
                     listBox1.SelectedIndex = deleteIndex - 1;

                signalControlIndex = getSignalIndex(listBox1.SelectedItem.ToString());
                setInputControls();
                eventHandler_Enable = true;

                combinedSignal = combinationOfSignals(signalSeries);
                ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                test_FixFFT();

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Max_Frequency = (float)Convert.ToDouble(textBox1.Text);
            trackBar1.Maximum = (int)Max_Frequency;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;

            if (eventHandler_Enable == true)
            {
                if (textBox5.Text != "")
                    signalSeries[signalControlIndex].signalName = textBox5.Text;
                else
                    signalSeries[signalControlIndex].signalName = "Unnamed";

                eventHandler_Enable = false;
                listBox1.Items.RemoveAt(index);
                
                listBox1.Items.Insert(index, signalSeries[signalControlIndex].signalName);

                eventHandler_Enable = true;

                listBox1.SelectedIndex = index;
            }
        }

        private void textBox5_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox4.Clear();
            textBox6.Clear();
            
            for (int i = 0; i < FFT_SIZE; i++)
                textBox4.AppendText(i.ToString("[0000]") + " " + ADC_Buffer[i].ToString() + Environment.NewLine);

            for (int i = 0; i < (FFT_SIZE >> 1) - 1; i++)
                textBox6.AppendText(i.ToString("[0000]") + " [" + freqBand[i].ToString("#0.#") + "-" + freqBand[i + 1].ToString("#0.#") + "] : " + FFT_Output[i].ToString() + Environment.NewLine);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            groupBox1.Enabled = !checkBox2.Checked;
            textBox1.Enabled = !checkBox2.Checked;
            textBox2.Enabled = !checkBox2.Checked;
            numericUpDown1.Enabled = !checkBox2.Checked;

            listBox1.Enabled = !checkBox2.Checked;
            button1.Enabled = !checkBox2.Checked;
            button2.Enabled = !checkBox2.Checked;

            button5.Enabled = checkBox2.Checked;

            if(checkBox2.Checked == false)
            {

                signalSeries[0].Freq = signalInput0_FreqTemp;
                signalSeries[0].createSineWave(F_SAMPLE, FFT_SIZE);

                for (int j = 1; j < signalSeries.Count; j++)
                {                    
                    signalSeries[j].Activate = true;
                    signalSeries[j].createSineWave(F_SAMPLE, FFT_SIZE);
                }
                
                combinedSignal = combinationOfSignals(signalSeries);
                ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                test_FixFFT();

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            windowing_Status = checkBox1.Checked;
            
            if (eventHandler_Enable == true)
            {
                combinedSignal = combinationOfSignals(signalSeries);
                ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                test_FixFFT();

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (userInputEnable == false)
            {
                UserInput1 = new UserInput();

                UserInput1.FFT_Size = FFT_SIZE;
                UserInput1.SamplesInShort = new short[FFT_SIZE];
                UserInput1.DiscreteTimeSamples = new float[FFT_SIZE];

                UserInput1.ADC_Ref = ADC_Ref;
                UserInput1.ADC_Resolation = ADC_Resolation;

                for (int i = 0; i < FFT_SIZE; i++)
                {
                    UserInput1.SamplesInShort[i] = (short)ADC_Buffer[i];
                    signalSeries[0].DiscreteTimeSamples[i] = (float)combinedSignal[i];
                }
                Array.Clear(signalSeries[0].ContiniousTimeSamples, 0, signalSeries[0].ContiniousTimeSamples.Length);

                userInputEnable = true;

                signalInput0_FreqTemp = signalSeries[0].Freq;

                listBox1.SelectedItem = signalSeries[0].signalName;

                UserInput1.Show();
            }
        }

        public void Update_UserInput()
        {
            combinedSignal = new double[FFT_SIZE];

            for(int  i = 0; i < signalSeries.Count; i++)
            {
                Array.Clear(signalSeries[i].ContiniousTimeSamples, 0, signalSeries[0].ContiniousTimeSamples.Length);               
                signalSeries[i].Activate = false;
            }

            signalSeries[0].Activate = true;

            for (int i = 0; i < FFT_SIZE; i++)
            {
                signalSeries[0].DiscreteTimeSamples[i] = UserInput1.DiscreteTimeSamples[i];
                combinedSignal[i] = UserInput1.DiscreteTimeSamples[i];
            }

            ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
            ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

            test_FixFFT();

            ChartsUpdate(FFT_SIZE, 0);
        }

        public void UserInputFormClosed()
        {
            userInputEnable = false;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            signalsUpdate();
        }

        public void createFreqRangeTable(float f_sample, ushort fft_size)
        {
            freqBand = new float[FFT_SIZE >> 1];

            FREQUENCY_RANGE = ((float)f_sample / 2) / (fft_size / 2);

            for (int i = 0; i < freqBand.Length; i++)  // Make frequency range table 	
                freqBand[i] = FREQUENCY_RANGE * i;
        }

        public void signalsUpdate()
        {
            for (int j = 0; j < signalSeries.Count; j++)
            {
                createNewSignal(signalSeries[j].signalName, signalSeries[j].Freq, signalSeries[j].Amplitude, signalSeries[j].Theta, signalSeries[j].Activate);

                signalSeries.RemoveAt(j);
                signalSeries.Insert(j, _signalInput);
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
 
            ADC_Ref = (float)numericUpDown2.Value;
            label15.Text = "(-" + ADC_Ref.ToString() + ", +" + ADC_Ref.ToString() + ")";

            if (eventHandler_Enable == true)
            {
                combinedSignal = combinationOfSignals(signalSeries);
                ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                test_FixFFT();

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            ADC_Resolation = (ushort)(Math.Pow(2, (ushort)numericUpDown3.Value - 1) - 1);
            label16.Text = "(-" + ADC_Resolation.ToString() + ", +" + ADC_Resolation.ToString() + ")";

            if (eventHandler_Enable == true)
            {
                combinedSignal = combinationOfSignals(signalSeries);
                ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                test_FixFFT();

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (eventHandler_Enable == true)
            {
                signalSeries[signalControlIndex].Amplitude = (float)Convert.ToDouble(numericUpDown4.Value);
                signalSeries[signalControlIndex].createSineWave(F_SAMPLE, FFT_SIZE);

                combinedSignal = combinationOfSignals(signalSeries);
                ADC_Buffer = ADC_Conversation(combinedSignal, ADC_Ref, ADC_Resolation);
                ADC_Buffer = _fix_FFT.HannWindowing(ADC_Buffer, FFT_SIZE, windowing_Status);

                test_FixFFT();

                ChartsUpdate(FFT_SIZE, signalControlIndex);
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            textBox6.Text = freqBand[0].ToString("#0.#") + "   " + freqBand[1].ToString("#0.#");
        }

        private int getSignalIndex(string signalName)
        {
            int i;

            for (i = 0; i < signalSeries.Count; i++)
            {
                if (signalSeries[i].signalName == signalName)
                    return i;
            }
                       
            return -1;
        }
    }
}
