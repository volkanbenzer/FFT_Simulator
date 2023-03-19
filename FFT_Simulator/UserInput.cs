using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFT_Simulator
{
    public partial class UserInput : Form
    {
        public static UserInput instance;

        public ushort FFT_Size;
        public short[] SamplesInShort;
        public float[] DiscreteTimeSamples;

        public float ADC_Ref;
        public ushort ADC_Resolation;
        public UserInput()
        {
            InitializeComponent();  
            
            instance = this;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string[] textBoxSplit = Regex.Split(textBox1.Text, "\r\n");


            for (int i = 0; i < FFT_Size; i++)
            {
                SamplesInShort[i] = Convert.ToInt16(textBoxSplit[i]);
                DiscreteTimeSamples[i] = (float)(((double)SamplesInShort[i] * ADC_Ref) / (double)ADC_Resolation);
            }

            Form1.instance.Update_UserInput();
        }

        private void UserInput_Load(object sender, EventArgs e)
        {
            label1.Text = "ADC Ref.: " + string.Format("{0:0.0}", ADC_Ref);
            label2.Text = "ADC Res.: " + (decimal)(Math.Log((ADC_Resolation + 1) * 2, 2));
            label3.Text = "FFT Size: " + FFT_Size.ToString();

            for (int i = 0; i < FFT_Size; i++)
            {
                textBox1.Text += SamplesInShort[i];

                if ((i + 1) < FFT_Size)
                    textBox1.Text += Environment.NewLine;
            }

        }

        private void UserInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.instance.UserInputFormClosed(); 
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string[] textBoxSplit = Regex.Split(textBox1.Text, "\r\n");

            label4.Text = "Sample Count: " +  textBoxSplit.Length + " / " + FFT_Size.ToString();

            if (textBoxSplit.Length < FFT_Size)
                label4.BackColor = Color.Red;
            else
                label4.BackColor = Color.Green;
        }
    }
}
