using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using ZXing;
using SautinSoft;
using System.Windows.Forms;
using System.Collections.Generic;
using ZXing.Common;
using Microsoft.VisualBasic;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using System.Drawing;
using System.ComponentModel;

namespace BarcodeTest
{
    public partial class Form1 : Form
    {
        public BackgroundWorker _worker;
        private ProgressBar _progressBar;
        string[] allfiles;
        Form3 f3 = new Form3();
        public Form1()
            {
            Form3 f3 = new Form3();
            _progressBar = f3.progressBar1;
            //Controls.Add(_progressBar);
            //Size = new Size(220, 40);
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += Worker_DoWork;
            _worker.ProgressChanged += Worker_ProgressChanged;
            this.MaximizeBox = false;
            Form2 f2 = new Form2();
            f2.listBox1.SelectedItem = Properties.Settings.Default.barcodselect;


            InitializeComponent();
        }
        public BarcodeReader barcodeReader = new BarcodeReader()
        {
            AutoRotate = true,
            TryInverted = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                PureBarcode = true,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.CODE_128 }
            }
        };
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Refresh();
           if(Properties.Settings.Default.barcodselect.ToString() != null)
            {
                label3.Text = Properties.Settings.Default.barcodselect.ToString();
            }
           if(Properties.Settings.Default.phaf.ToString() != null)
            {
                textBox1.Text = Properties.Settings.Default.phaf.ToString();
            }
           if (Properties.Settings.Default.phaf2.ToString() != null)
            {
                textBox2.Text = Properties.Settings.Default.phaf2.ToString();
            }
               

        }
        public void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {        // Обновление ProgressBar 
            
            f3.progressBar1.Value = e.ProgressPercentage;
        }
        
        public void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            
            while (true)
            {
                if (_worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                try
                {
                   
                    allfiles = Directory.GetFiles(textBox1.Text, "*.pdf");
                   
                    foreach (string filename in allfiles)
                    {
                        
                        if (filename != null)
                        {
                            if (allfiles.Length != 0)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    f3.Show();
                                }));
                            }
                            DateTime lastwritefile = File.GetCreationTime(filename);
                            TimeSpan timedeference = DateTime.Now - lastwritefile;
                            if (timedeference.TotalSeconds >= 60)
                            {
                                System.Threading.Thread.Sleep(5000);
                                var pdfFocus = new PdfFocus();
                                pdfFocus.OpenPdf(filename);
                                var sourceDocument = PdfReader.Open(filename, PdfDocumentOpenMode.Import);
                                PdfDocument targetDocument = null; string barcode = null;
                                for (int i = 0; i < sourceDocument.PageCount; i++)
                                {

                                    var page = sourceDocument.Pages[i];
                                    // Ensure pdfFocus is not null before converting the PDF page to an image                       
                                    if (pdfFocus != null)
                                    {
                                        var bitmap = pdfFocus.ToDrawingImage(i + 1);
                                        if (bitmap != null)
                                        {
                                            var image = new Bitmap(bitmap);
                                            var newBarcode = ReadBarcodeFromBitmap(image, barcodeReader);
                                            // If a new barcode is found, save the current document and start a new one                           
                                            if (newBarcode != null && newBarcode != barcode)
                                            {
                                                if (targetDocument != null)
                                                {
                                                    //var targetFilePath = Path.Combine(Path.GetDirectoryName(textBox2.Text), $"{barcode}.pdf");
                                                    this.Invoke(new Action(() =>
                                                    {
                                                        targetDocument.Save(textBox2.Text + barcode + ".pdf");
                                                    }));

                                                    targetDocument.Close();
                                                }
                                                targetDocument = new PdfDocument();
                                                targetDocument.Version = sourceDocument.Version;
                                                targetDocument.Info.Creator = sourceDocument.Info.Creator;
                                                barcode = newBarcode;
                                            }
                                            if (targetDocument != null) { targetDocument.AddPage(page); }
                                            image.Dispose();
                                        }
                                        _worker.ReportProgress((int)((double)i / sourceDocument.PageCount * 100));
                                    }
                                }
                                // Save the last document                   
                                if (targetDocument != null && barcode != null)
                                {
                                    this.Invoke(new Action(() =>
                                    {
                                    //var targetFilePath = Path.Combine(Path.GetDirectoryName(textBox2.Text), $"{barcode}.pdf");
                                    targetDocument.Save(textBox2.Text + barcode + ".pdf");
                                    }));
                                    targetDocument.Close();
                                    targetDocument.Dispose();
                                }
                                sourceDocument.Close();
                                sourceDocument.Dispose();
                                pdfFocus.ClosePdf();
                                _worker.ReportProgress(100);
                                System.Threading.Thread.Sleep(500);
                                File.Delete(filename);
                                _worker.ReportProgress(0);
                                this.Invoke(new Action(() =>
                                {
                                    f3.Hide();
                                }));
                                allfiles = Directory.GetFiles(textBox1.Text, "*.pdf");

                            }
                            else
                            {
                                System.Threading.Thread.Sleep(1000);
                            }
                        }
                    }
                }
                finally
                { 
                    System.Threading.Thread.Sleep(1000); 
                }
               }
                
            }

        private string ReadBarcodeFromBitmap(Bitmap bitmap, BarcodeReader barcodeReader)
        {
            var result = barcodeReader.Decode(bitmap);

            return result?.Text.Replace("/", "");
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.Show();
        }
       

        private void button3_ClickAsync(object sender, EventArgs e)
        {
            if (button3.Text == "Запуск")
            {
                this.WindowState = FormWindowState.Minimized;
                Properties.Settings.Default.phaf = textBox1.Text;
                Properties.Settings.Default.phaf2 = textBox2.Text;
                Properties.Settings.Default.Save();
                _worker.RunWorkerAsync();
                button3.Text = "Стоп";
            }
            else
            {
                if (_worker.IsBusy)
                {
                    _worker.CancelAsync();
                }
               
                button3.Text = "Запуск";
                return;
            }
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Open.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = Open.SelectedPath + @"\"; // !!!
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Open.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = Open.SelectedPath; 
            }
        }
    }

}
