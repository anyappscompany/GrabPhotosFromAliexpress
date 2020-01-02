using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.Threading;
using System.IO;

namespace GrabPhotosFromAliexpress
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;
            if (backgroundWorker1.IsBusy != true)
            {
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
            }

            
        }

        private bool grabPhotos(string url)
        {
            List<string> imagesFromPage = new List<string>();

            string html = getPage(url);
            //webBrowser1.DocumentText = html;
            //string pattern = @"(?<=<img .*?src\s*=\s*"")[^""]+(?="".*?>)";
            string pattern = @"<div class=""ui-tab-body"">(.*)<div id=""feedback"" class=""ui-tab-pane"">";
            MatchCollection match = Regex.Matches(html, pattern, RegexOptions.Multiline);
            Regex exp = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            match = exp.Matches(html);
            if (match.Count <= 0)
                return false;
            html = match[0].Value;

            if (match.Count >0 ) //найдено тело карточки товара
            {
                string patternImgs = @"(?<=<img .*?src\s*=\s*"")[^""]+(?="".*?>)";
                MatchCollection matchImgs = Regex.Matches(html, pattern, RegexOptions.Multiline);
                Regex expImgs = new Regex(patternImgs, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                matchImgs = expImgs.Matches(html);

                for (int i = 0; i < matchImgs.Count; i++ )
                {                     
                    imagesFromPage.Add(matchImgs[i].Value);
                }
            }

            if(imagesFromPage.Count > 0)
            {
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "images/"))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "images/");
                }

                string foldName = "";
                string[] split = url.Split(new Char[] { '/' });
                foldName = split[split.Count() -1].Replace(".html", "");

                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "images/" + foldName))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "images/" + foldName);
                }

                foreach(string imaga in imagesFromPage)
                {
                    //MessageBox.Show(imaga);
                    string [] splitImaga = imaga.Split(new Char [] {'/'});
                    string kartinka = splitImaga[splitImaga.Count() - 1];
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(imaga, AppDomain.CurrentDomain.BaseDirectory + "images/" + foldName + "/" + kartinka);
                }
                return true;
            }
           

            return false;
        }

        private string getPage(string page)
        {
            WebRequest reqGET = WebRequest.Create(page);
            WebResponse resp = reqGET.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string html = sr.ReadToEnd();
            return html;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                backgroundWorker1.CancelAsync();
            }
        }
        public delegate void MethodInvoker();
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            
                BackgroundWorker worker = sender as BackgroundWorker;

                int totalUrl = textBox1.Lines.Length;

                if (totalUrl > 0)
                {
                    String[] s = textBox1.Text.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    int i = 1;
                    foreach (string url in s)
                    {
                        if (worker.CancellationPending == true)
                        {
                            e.Cancel = true;
                            break;
                        }
                        else
                        {
                            this.Invoke((MethodInvoker)delegate()
    {
        if (richTextBox1.Lines.Length > 50)
            richTextBox1.Clear();
    });
                            try
                            {
                            if (grabPhotos(url))
                            {
                                this.Invoke((MethodInvoker)delegate()
    {
        richTextBox1.AppendText(Environment.NewLine + "+ " + url);
    });
                            }
                            else
                            {
                                this.Invoke((MethodInvoker)delegate()
    {
        richTextBox1.AppendText(Environment.NewLine + "- " + url );
    });
                            }
                            }
            catch(Exception ex)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    richTextBox1.AppendText(Environment.NewLine + ex.Message + " --- " + url);
                });
            }
                            worker.ReportProgress(i * (100 / totalUrl));
                        }
                        i++;

                    }
                }
            
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            resultLabel.Text = (e.ProgressPercentage.ToString() + "%");
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;

            if (e.Cancelled == true)
            {
                resultLabel.Text = "Canceled!";
            }
            else if (e.Error != null)
            {
                resultLabel.Text = "Error: " + e.Error.Message;
            }
            else
            {
                resultLabel.Text = "Done!";
            }
        }

        Form2 form2;
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form2 = new Form2(); // Вот здесь новый экземпляр создаёшь 
            form2.Show(); 
        }
    }
}
