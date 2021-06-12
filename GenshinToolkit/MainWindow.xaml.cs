using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;

using MessageBox = System.Windows.Forms.MessageBox;

namespace GenshinToolkit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (Tools.getVersionInfoGH() != true)
            {
                MessageBox.Show("Cannot check for version updates... :(", "Cannot Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Windows.Forms.Application.Exit();
            }
            init_comboboxes();
        }

        private void Browsebutton_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    GameDirBox.Text = fbd.SelectedPath;
                }

                ObservableCollection<string> versions = new ObservableCollection<string>();
                versions.Add("None");
                try
                {
                    versions.Add(Tools.ReadVersionData(GameDirBox.Text + "\\config.ini"));
                }
                catch (IniParser.Exceptions.ParsingException)
                {
                    ;
                }
                finally
                {
                    CurrentVersion_box.ItemsSource = versions;
                    CurrentVersion_box.SelectedItem = CurrentVersion_box.Items[CurrentVersion_box.Items.Count - 1];
                }
            }
        }

        private void LaunchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(GameDirBox.Text))
            {
                MessageBox.Show("Make sure you set the correct directory...", "Can't find anything :(", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string[] files = Directory.GetFiles(GameDirBox.Text);
                foreach (var file in files)
                {
                    if (file.EndsWith("GenshinImpact.exe"))
                    {
                        Console.WriteLine("Go!");

                        StringBuilder launchArgs = new StringBuilder();

                        if (play_borderless_chk.IsChecked == true)
                        {
                            launchArgs.Append("-popupwindow ");
                        }

                        if (graphicsConfigChk.IsChecked == true)
                        {
                            launchArgs.Append("-show-screen-selector ");
                        }

                        Process.Start(file, launchArgs.ToString().TrimEnd());

                        if (closeAppChk.IsChecked == true) Close();

                        break;
                    }
                }

                MessageBox.Show("Cannot find GenshinImpact.exe", "No launchbox :(", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Download_btn_click(object sender, RoutedEventArgs e)
        {    

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += do_download;
            worker.ProgressChanged += update_dl_statusbar;

            worker.RunWorkerAsync();
        }

        private void do_download(object sender, DoWorkEventArgs e)
        {
            var downloadInfo = this.Dispatcher.Invoke(() =>
            {
                var curr_ver = CurrentVersion_box.SelectedItem.ToString();
                var upd_ver = UpdateVersion_box.SelectedItem.ToString();
                Download_ProgLabel.Content = "Preparing Download...";
                return Tools.getDownloadList(curr_ver, upd_ver);
            });
            (sender as BackgroundWorker).ReportProgress(5);

            this.Dispatcher.Invoke(() =>
            {
                Download_ProgLabel.Content = "Downloading support tools";
            });

            if (Tools.check_download_Aria()!=true || Tools.check_download_7z() != true)
            {
                MessageBox.Show("Cannot get support tools :(", "Network Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            (sender as BackgroundWorker).ReportProgress(5);

            var path = this.Dispatcher.Invoke(() =>
            {
                return GameDirBox.Text;
            });

            Tools.WriteConfigIni(downloadInfo.version, path + "/config.ini");

            this.Dispatcher.Invoke(() =>
            {
                Download_ProgLabel.Content = "Downloading Base Game...";
            });
            var ret = startAria2download(path, downloadInfo.base_game_download_md5, downloadInfo.base_game_download);

            if (ret == 0 && this.Dispatcher.Invoke(() =>
            {
                return Dwl_Autoextract_chk.IsChecked;
            }) == true)
            {
                Tools.ExtractZIP(path, path + "\\" + Tools.GetFileName(downloadInfo.base_game_download));
            }

            (sender as BackgroundWorker).ReportProgress(25);

            if (this.Dispatcher.Invoke(() =>
            {
                return EnVO_upd.IsChecked;
            }) == true)
            {
                this.Dispatcher.Invoke(() =>
                {
                    Download_ProgLabel.Content = "Downloading English Voice Pack";

                });
                ret = startAria2download(path, downloadInfo.en_vo_pack_md5, downloadInfo.en_vo_pack);

                if (ret == 0 && this.Dispatcher.Invoke(() =>
                {
                    return Dwl_Autoextract_chk.IsChecked;
                }) == true)
                {
                    Tools.ExtractZIP(path, path + "\\" + Tools.GetFileName(downloadInfo.en_vo_pack));
                }
            }
            (sender as BackgroundWorker).ReportProgress(50);

            if (this.Dispatcher.Invoke(() =>
            {
                return JpVO_upd.IsChecked;
            }) == true)
            {
                this.Dispatcher.Invoke(() =>
                {
                    Download_ProgLabel.Content = "Downloading Japanese Voice Pack";

                });
                ret = startAria2download(path, downloadInfo.jp_vo_pack_md5, downloadInfo.jp_vo_pack);

                if (ret == 0 && this.Dispatcher.Invoke(() =>
                {
                    return Dwl_Autoextract_chk.IsChecked;
                }) == true)
                {
                    Tools.ExtractZIP(path, path + "\\" + Tools.GetFileName(downloadInfo.jp_vo_pack));
                }
            }
            (sender as BackgroundWorker).ReportProgress(70);

            if (this.Dispatcher.Invoke(() =>
            {
                return CnVO_upd.IsChecked;
            }) == true)
            {
                this.Dispatcher.Invoke(() =>
                {
                    Download_ProgLabel.Content = "Downloading Chinese Voice Pack";

                });
                ret = startAria2download(path, downloadInfo.cn_vo_pack_md5, downloadInfo.cn_vo_pack);

                if (ret == 0 && this.Dispatcher.Invoke(() =>
                {
                    return Dwl_Autoextract_chk.IsChecked;
                }) == true)
                {
                    Tools.ExtractZIP(path, path + "\\" + Tools.GetFileName(downloadInfo.cn_vo_pack));
                }
            }
            (sender as BackgroundWorker).ReportProgress(90);

            if (this.Dispatcher.Invoke(() =>
            {
                return KorVo_upd.IsChecked;
            }) == true)
            {
                this.Dispatcher.Invoke(() =>
                {
                    Download_ProgLabel.Content = "Downloading Korean Voice Pack";

                });
                ret = startAria2download(path, downloadInfo.ko_vo_pack_md5, downloadInfo.ko_vo_pack);

                if (ret == 0 && this.Dispatcher.Invoke(() =>
                {
                    return Dwl_Autoextract_chk.IsChecked;
                }) == true)
                {
                    Tools.ExtractZIP(path, path + "\\" + Tools.GetFileName(downloadInfo.ko_vo_pack));
                }
            }
            (sender as BackgroundWorker).ReportProgress(100);

            if (this.Dispatcher.Invoke(() => {
                return dwl_rewrite_configini.IsChecked;
            }) == true)
            {
                Tools.WriteConfigIni(downloadInfo.version, path + "/config.ini");
            }

            this.Dispatcher.Invoke(() =>
            {
                Download_progbar.SetPercent(100);
                Download_ProgLabel.Content = "Done!";
            });

        }

        private void update_dl_statusbar(object sender, ProgressChangedEventArgs e)
        {
            Download_progbar.SetPercent(e.ProgressPercentage);
        }

        private int startAria2download(string path, string md5, string link)
        {
            var aria2args = "-x16 -j16 -k 1M -d " + path + " -Vtrue --checksum=md5=" + md5 + " " + link;
            var process = Process.Start("aria2c.exe", aria2args);
            process.WaitForExit();
            return process.ExitCode;
        }
        private int startAria2download(string md5, string link)
        {
            var aria2args = "-x16 -j16 -k 1M" + " -Vtrue --checksum=md5=" + md5 + " " + link;
            var process = Process.Start("aria2c.exe", aria2args);
            process.WaitForExit();
            return process.ExitCode;
        }

        private void CurrentVersion_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VersionInfoJSON versionData = Tools.DeserializeVersionInfoJSON("versioninfo.json");

            ObservableCollection<string> update_version = new ObservableCollection<string>();
            update_version.Add(versionData.data.game.latest.version);
            UpdateVersion_box.ItemsSource = update_version;
            UpdateVersion_box.SelectedItem = UpdateVersion_box.Items[UpdateVersion_box.Items.Count - 1];
        }


        private void init_comboboxes()
        {
            ObservableCollection<string> current_version = new ObservableCollection<string>();
            current_version.Add("None");
            try
            {
                current_version.Add(Tools.ReadVersionData(GameDirBox.Text + "\\config.ini"));
            }
            catch (IniParser.Exceptions.ParsingException)
            {
                ;
            }
            finally
            {
                CurrentVersion_box.ItemsSource = current_version;
                CurrentVersion_box.SelectedItem = CurrentVersion_box.Items[CurrentVersion_box.Items.Count - 1];
            }

            VersionInfoJSON versionData = Tools.DeserializeVersionInfoJSON("versioninfo.json");
            ObservableCollection<string> fix_vers = new ObservableCollection<string>();
            fix_vers.Add(versionData.data.game.latest.version);
            foreach(var diff in versionData.data.game.diffs)
            {
                fix_vers.Add(diff.version);
            }
            fixVersion_cb.ItemsSource = fix_vers;
            fixVersion_cb.SelectedItem = fixVersion_cb.Items[0];
        }

        private void fix_button_Click(object sender, RoutedEventArgs e)
        {
            var deprList = Tools.getDeprecatedList();
            if (!Directory.Exists(GameDirBox.Text))
            {
                MessageBox.Show("Make sure you set the correct directory...", "Can't find anything :(", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string[] files = Directory.GetFiles(GameDirBox.Text);
                foreach (var file in files)
                {
                    if (fix_rem_tmp_chk.IsChecked == true)
                    {
                        if (file.EndsWith(".zip_tmp"))
                        {
                            Console.WriteLine("Found File: " + file);
                            File.Delete(file);
                        }
                        if (File.Exists(file + ".aria2"))
                        {
                            Console.WriteLine("Found File: " + file);
                            File.Delete(file);
                            File.Delete(file + ".aria2");
                        }
                    }
                    if (fix_remove_deprecated.IsChecked == true)
                    {

                        if (deprList.Contains(Path.GetFileName(file)))
                        {
                            Console.WriteLine("Found File: " + file);
                            File.Delete(file);
                            ;
                        }
                    }
                }

                if (fix_resetSettings_chk.IsChecked == true)
                {
                    Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\miHoYo");
                    Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\miHoYoSDK");
                }

                if (fix_delete_gamedata.IsChecked == true)
                {
                    var localAppData_mhy = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "miHoYo");
                    Console.WriteLine(localAppData_mhy);
                    Directory.Delete(localAppData_mhy, true);

                    string localLowAppData_mhy = @"%UserProfile%\AppData\LocalLow\miHoYo";
                    localLowAppData_mhy = Environment.ExpandEnvironmentVariables(localLowAppData_mhy);
                    Console.WriteLine(localLowAppData_mhy);
                    Directory.Delete(localLowAppData_mhy, true);

                }

                if (fix_rewrite_config_chk.IsChecked == true)
                {
                    Tools.WriteConfigIni(fixVersion_cb.Text, GameDirBox.Text + "/config.ini");
                }
                MessageBox.Show("Success. Try starting your game now", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void misc_reset_network_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("ipconfig", "/release").WaitForExit();
            Process.Start("ipconfig", "/renew").WaitForExit();
            Process.Start("ipconfig", "/flushdns").WaitForExit();
            MessageBox.Show("Successfully reset Windows networking", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void misc_dump_dxdiag_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("dxdiag", "/t sysinfo.txt").WaitForExit();
            MessageBox.Show("System info has been dumped to sysinfo.txt", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void misc_msvc_install_Click(object sender, RoutedEventArgs e)
        {
            if (Tools.check_download_Aria() != true)
            {
                MessageBox.Show("Cannot get support tools :(", "Network Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                startAria2download("FB1CB75F59D98B5D1E1E31476CBE6F61", "https://aka.ms/vs/16/release/vc_redist.x64.exe");
                Process.Start("vc_redist.x64.exe");
            }
        }

        private void misc_reinstall_DirectX_Click(object sender, RoutedEventArgs e)
        {
            if (Tools.check_download_Aria() != true || Tools.check_download_7z() != true)
            {
                MessageBox.Show("Cannot get support tools :(", "Network Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                startAria2download("CA2AC3835D7D7DA6CB8624FEFB177083", "https://autopatchhk.yuanshen.com/client_app/plugins/DXSETUP.zip");
                Process.Start("7za.exe", "x DXSETUP.zip").WaitForExit();
                Process.Start("DXSETUP\\DXSETUP.exe");
            }
        }
    }
    public static class ProgressBarExtensions
    {
        private static TimeSpan duration = TimeSpan.FromSeconds(1);

        public static void SetPercent(this System.Windows.Controls.ProgressBar progressBar, double percentage)
        {
            DoubleAnimation animation = new DoubleAnimation(percentage, duration);
            progressBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, animation);
        }
    }
}
