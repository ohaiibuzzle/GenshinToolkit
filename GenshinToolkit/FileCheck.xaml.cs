using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GenshinToolkit
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FileCheckerWindow : Window
    {
        string gamePath = "";
        string[] badFiles = new string[] { };
        int goodFiles = 0;
        BackgroundWorker worker;
        public FileCheckerWindow(string gamePath)
        {
            this.gamePath = gamePath;
            InitializeComponent();

            statusTextBox.Text = "Checking files from " + gamePath + "\n";

            this.worker = new BackgroundWorker();
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += DoFileCheckup;
            this.worker.ProgressChanged += updateTextBox;
            this.worker.RunWorkerCompleted += runCompleted;

            this.worker.RunWorkerAsync();

        }

        private void runCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            statusTextBox.Clear();
            if (badFiles.Length != 0)
            {
                
                statusTextBox.AppendText("Bad files were found:\n");
                foreach(var badfile in badFiles)
                {
                    statusTextBox.AppendText(badfile + '\n');
                }
            }
            else
            {
                statusTextBox.AppendText("Everything looks OK!\n");
            }
            statusTextBox.AppendText("Good: " + goodFiles + ", Bad: " + badFiles.Length + ", Total: " + goodFiles + badFiles.Length);
        }

        private void DoFileCheckup(object sender, DoWorkEventArgs e)
        {
            string[] hash_files = { "pkg_version", "Audio_English(US)_pkg_version", "Audio_Japanese_pkg_version", "Audio_Korean_pkg_version", "Audio_Chinese_pkg_version" };
            foreach (var hash_file in hash_files)
            {
                var path = gamePath + "\\" + hash_file;
                if (File.Exists(path))
                {
                    var lines = File.ReadAllLines(path);
                    foreach (var line in lines)
                    {
                        FileHashInfo thisFile = JsonConvert.DeserializeObject<FileHashInfo>(line);
                        var filepath = gamePath + "\\" + thisFile.remoteName;
                        if (File.Exists(filepath))
                        {
                            if (Tools.CompareMD5Async(filepath, thisFile.md5))
                            {
                                (sender as BackgroundWorker).ReportProgress(1, new string[]{ filepath, " checked OK!" });
                            }
                            else
                            {
                                (sender as BackgroundWorker).ReportProgress(0, new string[]{filepath, " not OK!"});
                            }
                        }
                        if ((sender as BackgroundWorker).CancellationPending)
                        {
                            return;
                        }
                    }
                }
            }
            return;
        }

        private void updateTextBox(object sender, ProgressChangedEventArgs e)
        {
            string[] status = (string[])e.UserState;
            if (e.ProgressPercentage == 0)
            {
                badFiles.Append<string>(status[0]);
            }
            else
            {
                goodFiles++;
            }

            statusTextBox.AppendText(status[0] + status[1] + "\n");
            statusTextBox.ScrollToEnd();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (worker.IsBusy)
            {
                worker.CancelAsync();
                return;
            }
        }
    }
}
