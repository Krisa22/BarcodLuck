using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;

namespace BarcodeTest
{
    public partial class Form2 : Form
    {
        public Form2()
        {

            this.MaximizeBox = false;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            Properties.Settings.Default.barcodselect = listBox1.SelectedItem.ToString();
            Properties.Settings.Default.Save();
            Dictionary<string, BarcodeFormat> barcodeFormats = new Dictionary<string, BarcodeFormat>();
            Form1 f = new Form1();
            foreach (BarcodeFormat format in Enum.GetValues(typeof(BarcodeFormat)))
            {
                string formatName = format.ToString();
                listBox1.Items.Add(formatName);
                barcodeFormats.Add(formatName, format);
            }
            string selectedFormatName = listBox1.SelectedItem.ToString();
            BarcodeFormat selectedFormat;
            if (barcodeFormats.TryGetValue(selectedFormatName, out selectedFormat))
            {
                f.barcodeReader.Options.PossibleFormats = new List<BarcodeFormat> { selectedFormat };
            }
            f.label3.Text = Properties.Settings.Default.barcodselect.ToString();
            this.Close();
            Application.Restart();

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            listBox1.SelectedItem = Properties.Settings.Default.barcodselect;
        }
    }
}
