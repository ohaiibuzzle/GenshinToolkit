using System;
using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Windows;

namespace GenshinToolkit
{
    /// <summary>
    /// Interaction logic for HoyoLabCheckin.xaml
    /// </summary>
    public partial class HoyoLabCheckin : Window
    {
        const string ENDPOINT = "https://hk4e-api-os.mihoyo.com/event/sol/sign?lang=en-us";

        public HoyoLabCheckin()
        {
            InitializeComponent();
            if (Properties.Settings.Default.hyLabCookieString == "mi18nLang=en-us; _MHYUUID=dQw4w9WgXcQ-nev3r-g00na-giv3-y0u-up-...")
            {
                cookies_tb.Text = Properties.Settings.Default.hyLabCookieString;
            } else
            {
                cookies_tb.Text = "Previously saved, hidden for obvious reasons.";
                chk_remember_cookies.IsChecked = true;
            }
            eventid_tb.Text = Properties.Settings.Default.hyLabEventID;
            useragent_tb.Text = Properties.Settings.Default.hyLabUserAgent;
            auto_checkin.IsChecked = Properties.Settings.Default.CheckInOnStartup;
        }

        private void Ok_btn_Click(object sender, RoutedEventArgs e)
        {
            StatusLabel.Content = "Working on it!";
            var cookies_str = Properties.Settings.Default.hyLabCookieString;
            
            if (Properties.Settings.Default.hyLabCookieString == "mi18nLang=en-us; _MHYUUID=dQw4w9WgXcQ-nev3r-g00na-giv3-y0u-up-...")
            {
                if (cookies_tb.Text == Properties.Settings.Default.hyLabCookieString)
                {
                    new MsgBox("Are you sure that cookie is valid?", "Whaaaaa").ShowDialog();
                    return;
                }

                cookies_str = cookies_tb.Text;
            }

            if (useragent_tb.Text == "Something/SomeVersion" && Properties.Settings.Default.hyLabUserAgent == "Something/SomeVersion")
            {
                new MsgBox("Please change the User Agent to avoid shenanegans", "User Agent notice").ShowDialog();
                return;
            }

            var ua_str = Properties.Settings.Default.hyLabUserAgent;


            var webClient = new System.Net.WebClient();
            if (webClient != null)
            {
                webClient.Headers[System.Net.HttpRequestHeader.ContentType] = "application/json";
                webClient.Headers.Add("Accept", "application/json, text/plain, */*");
                webClient.Headers.Add("User-Agent", ua_str);
                webClient.Headers.Add("Origin", "https, //webstatic-sea.mihoyo.com");
                webClient.Headers.Add("Referer", "https, //webstatic-sea.mihoyo.com/");
                webClient.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                webClient.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                webClient.Headers.Add(System.Net.HttpRequestHeader.Cookie, cookies_str);

                var response = webClient.UploadString(ENDPOINT, new JavaScriptSerializer().Serialize(new { act_id = eventid_tb.Text }));
                var response_json = new JavaScriptSerializer().Deserialize<ReturnJsonData>(response);

                StatusLabel.Content = "Done!";

                if (response_json.Retcode == 0)
                {
                    new MsgBox("Checked in successfully to HoyoLab", "Success!").ShowDialog();
                    Properties.Settings.Default.Today = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
                }
                else
                {
                    new MsgBox("Paimon once said: " + response_json.Message, "Something happened :(").ShowDialog();
                }

                if (chk_remember_cookies.IsChecked == true)
                {
                    Properties.Settings.Default.hyLabCookieString = cookies_str;
                    Properties.Settings.Default.hyLabEventID = eventid_tb.Text;
                    Properties.Settings.Default.hyLabUserAgent = useragent_tb.Text;
                    Properties.Settings.Default.Save();
                }

                if (auto_checkin.IsChecked == true)
                {
                    Properties.Settings.Default.CheckInOnStartup = true;
                    Properties.Settings.Default.Save();
                }
            }
        }

        public static void AutoCheckin(object sender, DoWorkEventArgs e)
        {
            TimeZoneInfo cst_zone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            var ShangHai_time = TimeZoneInfo.ConvertTime(DateTime.Now, cst_zone);

            if (ShangHai_time.DayOfYear == Properties.Settings.Default.Today.DayOfYear)
            {
                return;
            }

            var cookies_str = Properties.Settings.Default.hyLabCookieString;

            var webClient = new System.Net.WebClient();
            if (webClient != null)
            {
                webClient.Headers[System.Net.HttpRequestHeader.ContentType] = "application/json";
                webClient.Headers.Add("Accept", "application/json, text/plain, */*");
                webClient.Headers.Add("User-Agent", Properties.Settings.Default.hyLabUserAgent);
                webClient.Headers.Add("Origin", "https, //webstatic-sea.mihoyo.com");
                webClient.Headers.Add("Referer", "https, //webstatic-sea.mihoyo.com/");
                webClient.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                webClient.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                webClient.Headers.Add(System.Net.HttpRequestHeader.Cookie, cookies_str);

                var response = webClient.UploadString(ENDPOINT, new JavaScriptSerializer().Serialize(new { act_id = Properties.Settings.Default.hyLabEventID }));

                Properties.Settings.Default.Today = ShangHai_time;
                Properties.Settings.Default.Save();
            }
        }

        private void cookies_tb_GotFocus(object sender, RoutedEventArgs e)
        {
            cookies_tb.Clear();
        }

        private void Cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void chk_remember_cookies_Checked(object sender, RoutedEventArgs e)
        {
            if (chk_remember_cookies.IsChecked == true)
            {
                auto_checkin.IsEnabled = true;
            }
            else
            {
                auto_checkin.IsChecked = false;
                auto_checkin.IsEnabled = false;
            }
        }
    }

    public partial class ReturnJsonData
    {
        public int Retcode { get; set; }
        public string Message { get; set; }
    }

}
