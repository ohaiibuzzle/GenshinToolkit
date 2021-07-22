using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
            var startupNotif = new MsgBox("Updating version info", "Starting up");
            startupNotif.Show();
            if (Tools.GetVersionInfo() != true)
            {
                MessageBox.Show("Cannot check for version updates... :(", "Cannot Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Windows.Forms.Application.Exit();
            }
            startupNotif.Close();

            RestoreSettings();
            InitComboboxes();
            BuildString.Text = Assembly.GetExecutingAssembly().GetName().Name + " v. " +  Assembly.GetExecutingAssembly().GetName().Version.ToString();
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += UpdateServerStatus;
            worker.RunWorkerAsync();
        }

        private void Browsebutton_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    GameDirBox.Text = fbd.SelectedPath;
                    Properties.Settings.Default.GamePath = fbd.SelectedPath;
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

                        if (play_fullscreen_chk.IsChecked == true)
                        {
                            launchArgs.Append("-screen-fullscreen 1 ");
                        }
                        else
                        {
                            launchArgs.Append("-screen-fullscreen 0 ");
                        }

                        if (graphicsConfigChk.IsChecked == true)
                        {
                            launchArgs.Append("-show-screen-selector ");
                        }

                        if (play_custom_res_chk.IsChecked == true)
                        {
                            launchArgs.Append("-screen-width " + play_w_textbox.Text + " -screen-height " + play_h_textbox.Text);
                        }

                        Console.WriteLine(launchArgs.ToString().TrimEnd());

                        Process.Start(file, launchArgs.ToString().TrimEnd());

                        if (closeAppChk.IsChecked == true) Close();

                        return;
                    }
                }

                MessageBox.Show("Cannot find GenshinImpact.exe", "No launchbox :(", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void play_custom_res_chk_Checked(object sender, RoutedEventArgs e)
        {
            play_custom_res_grid.Visibility = Visibility.Visible;
            play_custom_res_grid.IsEnabled = true;
            Properties.Settings.Default.CustomResEnabled = true;
        }

        private void play_custom_res_chk_Unchecked(object sender, RoutedEventArgs e)
        {
            play_custom_res_grid.Visibility = Visibility.Hidden;
            play_custom_res_grid.IsEnabled = false;
            Properties.Settings.Default.CustomResEnabled = false;
        }

        private void Download_btn_click(object sender, RoutedEventArgs e)
        {    

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += DoDownload;
            worker.ProgressChanged += update_dl_statusbar;

            worker.RunWorkerAsync();
        }

        private void DoDownload(object sender, DoWorkEventArgs e)
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

            if (Tools.CheckDownloadAria2()!=true || Tools.check_download_7z() != true)
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
            var ret = StartAria2Download(path, downloadInfo.base_game_download_md5, downloadInfo.base_game_download);

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
                ret = StartAria2Download(path, downloadInfo.en_vo_pack_md5, downloadInfo.en_vo_pack);

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
                ret = StartAria2Download(path, downloadInfo.jp_vo_pack_md5, downloadInfo.jp_vo_pack);

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
                ret = StartAria2Download(path, downloadInfo.cn_vo_pack_md5, downloadInfo.cn_vo_pack);

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
                ret = StartAria2Download(path, downloadInfo.ko_vo_pack_md5, downloadInfo.ko_vo_pack);

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

        private int StartAria2Download(string path, string md5, string link)
        {
            path = "\"" + path + "\"";
            var aria2args = "-x16 -j16 -s16 -k 1M -d " + path + " -Vtrue --checksum=md5=" + md5 + " " + link;
            var process = Process.Start("aria2c.exe", aria2args);
            process.WaitForExit();
            return process.ExitCode;
        }
        private int StartAria2Download(string md5, string link)
        {
            var aria2args = "-x16 -j16 -s16 -k 1M" + " -Vtrue --checksum=md5=" + md5 + " " + link;
            var process = Process.Start("aria2c.exe", aria2args);
            process.WaitForExit();
            return process.ExitCode;
        }

        private void CurrentVersion_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VersionInfoJSON versionData = Tools.DeserializeVersionInfoJSON("versioninfo.json");

            ObservableCollection<string> update_version = new ObservableCollection<string>();
            update_version.Add(versionData.data.game.latest.version);
            if (versionData.data.pre_download_game != null)
            {
                update_version.Add(versionData.data.pre_download_game.latest.version);
            }
            UpdateVersion_box.ItemsSource = update_version;
            UpdateVersion_box.SelectedItem = UpdateVersion_box.Items[0];
        }


        private void InitComboboxes()
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
            if (Tools.CheckDownloadAria2() != true)
            {
                MessageBox.Show("Cannot get support tools :(", "Network Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                StartAria2Download("FB1CB75F59D98B5D1E1E31476CBE6F61", "https://aka.ms/vs/16/release/vc_redist.x64.exe");
                Process.Start("vc_redist.x64.exe");
            }
        }

        private void misc_reinstall_DirectX_Click(object sender, RoutedEventArgs e)
        {
            if (Tools.CheckDownloadAria2() != true || Tools.check_download_7z() != true)
            {
                MessageBox.Show("Cannot get support tools :(", "Network Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                StartAria2Download("CA2AC3835D7D7DA6CB8624FEFB177083", "https://autopatchhk.yuanshen.com/client_app/plugins/DXSETUP.zip");
                Process.Start("7za.exe", "x DXSETUP.zip").WaitForExit();
                Process.Start("DXSETUP\\DXSETUP.exe");
            }
        }

        private void misc_checkGameFileHashes_Click(object sender, RoutedEventArgs e)
        {
            FileCheckerWindow fc = new FileCheckerWindow(GameDirBox.Text);
            fc.Show();
        }

        private void misc_update_info_uri_Click(object sender, RoutedEventArgs e)
        {
            var changeUriDialog = new SimpleTextInput("Change Update URI", Properties.Settings.Default.UpdateInfoURI);
            changeUriDialog.ShowDialog();

            var UpdateNotif = new MsgBox("Updating version info", "Validating...");
            UpdateNotif.Show();
            if (!Tools.GetVersionInfo(changeUriDialog.value))
            {
                MessageBox.Show("Cannot fetch update data from URL provided", "Network Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Tools.GetVersionInfo();
                return;
            }
            try
            {
                Tools.DeserializeVersionInfoJSON("versioninfo.json");
            }
            catch (JsonException)
            {
                MessageBox.Show("Cannot parse update data from URL provided", "Parsing Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            UpdateNotif.Close();
            Properties.Settings.Default.UpdateInfoURI = changeUriDialog.value;
            InitComboboxes();
            Console.WriteLine(changeUriDialog.value);
        }

        private void misc_self_destruct_btn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            RestoreSettings();
        }

        private void play_refreshStatus_btn_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += UpdateServerStatus;

            worker.RunWorkerAsync();
        }

        private void UpdateServerStatus(object sender, DoWorkEventArgs e)
        {
            var status = Tools.serverInfo();
            this.Dispatcher.Invoke(() =>
            {
                play_as_status_chk.IsChecked = status.AS_Status;
                play_as_status_chk.Content = status.AS_Ping + " ms";

                play_eu_status_chk.IsChecked = status.EU_Status;
                play_eu_status_chk.Content = status.EU_Ping + " ms";

                play_na_status_chk.IsChecked = status.NA_Status;
                play_na_status_chk.Content = status.NA_Ping + " ms";

                play_tw_status_chk.IsChecked = status.TW_Status;
                play_tw_status_chk.Content = status.TW_Ping + " ms";

            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.CustomResW = play_w_textbox.Text;
            Properties.Settings.Default.CustomResH = play_h_textbox.Text;
            Properties.Settings.Default.BorderlessEnabled = (bool)play_borderless_chk.IsChecked;
            Properties.Settings.Default.FullscreenEnabled = (bool)play_fullscreen_chk.IsChecked;
            Properties.Settings.Default.SelfDestruct = (bool)closeAppChk.IsChecked;
            Properties.Settings.Default.OpenUnityConfig = (bool)graphicsConfigChk.IsChecked;
            Properties.Settings.Default.ActiveTab = MainUiTabs.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void RestoreSettings()
        {
            GameDirBox.Text = Properties.Settings.Default.GamePath;

            if (Properties.Settings.Default.CustomResH == "0" && Properties.Settings.Default.CustomResW == "0")
            {
                play_w_textbox.Text = SystemParameters.PrimaryScreenWidth.ToString();
                play_h_textbox.Text = SystemParameters.PrimaryScreenHeight.ToString();
            }
            else
            {
                play_w_textbox.Text = Properties.Settings.Default.CustomResW;
                play_h_textbox.Text = Properties.Settings.Default.CustomResH;
            }

            play_custom_res_chk.IsChecked = Properties.Settings.Default.CustomResEnabled;
            play_borderless_chk.IsChecked = Properties.Settings.Default.BorderlessEnabled;
            play_fullscreen_chk.IsChecked = Properties.Settings.Default.FullscreenEnabled;
            closeAppChk.IsChecked = Properties.Settings.Default.SelfDestruct;
            graphicsConfigChk.IsChecked = Properties.Settings.Default.OpenUnityConfig;

            MainUiTabs.SelectedIndex = Properties.Settings.Default.ActiveTab;
        }

        private void custom_res_textbox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !NumberFilter(e.Text);
        }

        private static readonly Regex numbersOnly = new Regex("[^0-9.-]+");
        private static bool NumberFilter(string text)
        {
            return !numbersOnly.IsMatch(text);
        }

        private void UpdateVersion_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VersionInfoJSON versionData = Tools.DeserializeVersionInfoJSON("versioninfo.json");
            if (versionData.data.pre_download_game != null) {
                if (UpdateVersion_box.SelectedItem.ToString() == versionData.data.pre_download_game.latest.version)
                {
                    dwl_rewrite_configini.IsChecked = false;

                    return;
                }
            }
            dwl_rewrite_configini.IsChecked = true;
        }

        private void BuildString_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/ohaiibuzzle/GenshinToolkit/");
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
