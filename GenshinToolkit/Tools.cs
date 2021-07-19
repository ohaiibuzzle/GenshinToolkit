using IniParser;
using IniParser.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GenshinToolkit
{
    public static class Tools
    {
        static public void WriteConfigIni(string version, string path)
        {
            var parser = new FileIniDataParser();
            IniData data = new IniData();
            data["General"]["channel"] = "1";
            data["General"]["cps"] = "mihoyo";
            data["General"]["game_version"] = version;
            data["General"]["sub_channel"] = "0";
            data["General"]["sdk_version"] = "";
            parser.WriteFile(path, data);
        }

        static public string ReadVersionData(string path)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(path);
            return data["General"]["game_version"];
        }
        static public bool? getAria2()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile("https://github.com/aria2/aria2/releases/download/release-1.35.0/aria2-1.35.0-win-64bit-build1.zip", "aria.zip");

                Console.WriteLine(CalcMD5("aria.zip"));
                if (CalcMD5("aria.zip") == "118d109c350993e6a8b43cbc3700e0a7")
                {
                    using (var archive = ZipFile.OpenRead("aria.zip"))
                    {
                        ZipArchiveEntry entry = archive.GetEntry("aria2-1.35.0-win-64bit-build1/aria2c.exe");
                        entry.ExtractToFile(entry.Name);
                        return true;
                    }
                }
                return false;

            }
        }

        static public bool? get7z()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile("https://7-zip.org/a/7za920.zip", "7z.zip");

                Console.WriteLine(CalcMD5("7z.zip"));
                if (CalcMD5("7z.zip") == "2fac454a90ae96021f4ffc607d4c00f8")
                {
                    using (var archive = ZipFile.OpenRead("7z.zip"))
                    {
                        ZipArchiveEntry entry = archive.GetEntry("7za.exe");
                        entry.ExtractToFile(entry.Name);
                        return true;
                    }
                }
                return false;

            }
        }

        static public bool? CheckDownloadAria2()
        {
            if (!File.Exists("aria2c.exe"))
            {
                if (Tools.getAria2() == true)
                {
                    File.Delete("aria.zip");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        static public bool? check_download_7z()
        {
            if (!File.Exists("7za.exe"))
            {
                if (Tools.get7z() == true)
                {
                    File.Delete("7z.zip");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        static public string CalcMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static bool CompareMD5Async(string filename, string correct_MD5)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return correct_MD5 == BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static List<string> getDeprecatedList()
        {
            VersionInfoJSON versionData = Tools.DeserializeVersionInfoJSON("versioninfo.json");
            List<string> deprecated = new List<string>();
            foreach (var file in versionData.data.deprecated_packages)
            {
                deprecated.Add(file.name);
            }
            return deprecated;
        }

        public static VersionDownloadInfo getDownloadList(string curr_ver, string upd_ver)
        {
            Console.WriteLine("Updating from " + curr_ver + " to " + upd_ver);
            VersionInfoJSON versionData = Tools.DeserializeVersionInfoJSON("versioninfo.json");
            VersionDownloadInfo this_version = new VersionDownloadInfo();
            bool isDone = false;
            foreach (var diff in versionData.data.game.diffs)
            {
                if (diff.version == curr_ver && upd_ver == versionData.data.game.latest.version)
                {
                    this_version.version = diff.version;
                    this_version.base_game_download = diff.path;
                    this_version.base_game_download_md5 = diff.md5;
                    foreach (var pack in diff.voice_packs)
                    {
                        switch (pack.language)
                        {
                            case "en-us":
                                this_version.en_vo_pack = pack.path;
                                this_version.en_vo_pack_md5 = pack.md5.ToLowerInvariant();
                                break;
                            case "ja-jp":
                                this_version.jp_vo_pack = pack.path;
                                this_version.jp_vo_pack_md5 = pack.md5.ToLowerInvariant();
                                break;
                            case "zh-cn":
                                this_version.cn_vo_pack = pack.path;
                                this_version.cn_vo_pack_md5 = pack.md5.ToLowerInvariant();
                                break;
                            case "ko-kr":
                                this_version.ko_vo_pack = pack.path;
                                this_version.ko_vo_pack_md5 = pack.md5.ToLowerInvariant();
                                break;
                        }
                    }
                    isDone = true;
                }
            }
            if (upd_ver == versionData.data.pre_download_game?.latest.version)
            {
                foreach (var diff in versionData.data.pre_download_game.diffs) {
                    if (diff.version == curr_ver)
                    {
                        this_version.version = diff.version;
                        this_version.base_game_download = diff.path;
                        this_version.base_game_download_md5 = diff.md5;
                        foreach (var pack in diff.voice_packs)
                        {
                            switch (pack.language)
                            {
                                case "en-us":
                                    this_version.en_vo_pack = pack.path;
                                    this_version.en_vo_pack_md5 = pack.md5.ToLowerInvariant();
                                    break;
                                case "ja-jp":
                                    this_version.jp_vo_pack = pack.path;
                                    this_version.jp_vo_pack_md5 = pack.md5.ToLowerInvariant();
                                    break;
                                case "zh-cn":
                                    this_version.cn_vo_pack = pack.path;
                                    this_version.cn_vo_pack_md5 = pack.md5.ToLowerInvariant();
                                    break;
                                case "ko-kr":
                                    this_version.ko_vo_pack = pack.path;
                                    this_version.ko_vo_pack_md5 = pack.md5.ToLowerInvariant();
                                    break;
                            }
                        }
                        isDone = true;
                    }
                }
                if (!isDone)
                {
                    this_version.version = versionData.data.pre_download_game.latest.version;
                    this_version.base_game_download = versionData.data.pre_download_game.latest.path;
                    this_version.base_game_download_md5 = versionData.data.pre_download_game.latest.md5;
                    foreach (var pack in versionData.data.pre_download_game.latest.voice_packs)
                    {
                        switch (pack.language)
                        {
                            case "en-us":
                                this_version.en_vo_pack = pack.path;
                                this_version.en_vo_pack_md5 = pack.md5;
                                break;
                            case "ja-jp":
                                this_version.jp_vo_pack = pack.path;
                                this_version.jp_vo_pack_md5 = pack.md5;
                                break;
                            case "zh-cn":
                                this_version.cn_vo_pack = pack.path;
                                this_version.cn_vo_pack_md5 = pack.md5;
                                break;
                            case "ko-kr":
                                this_version.ko_vo_pack = pack.path;
                                this_version.ko_vo_pack_md5 = pack.md5;
                                break;
                        }
                    }
                    isDone = true;
                }
            }
            if (!isDone)
            {
                {
                    this_version.version = versionData.data.game.latest.version;
                    this_version.base_game_download = versionData.data.game.latest.path;
                    this_version.base_game_download_md5 = versionData.data.game.latest.md5;
                    foreach (var pack in versionData.data.game.latest.voice_packs)
                    {
                        switch (pack.language)
                        {
                            case "en-us":
                                this_version.en_vo_pack = pack.path;
                                this_version.en_vo_pack_md5 = pack.md5;
                                break;
                            case "ja-jp":
                                this_version.jp_vo_pack = pack.path;
                                this_version.jp_vo_pack_md5 = pack.md5;
                                break;
                            case "zh-cn":
                                this_version.cn_vo_pack = pack.path;
                                this_version.cn_vo_pack_md5 = pack.md5;
                                break;
                            case "ko-kr":
                                this_version.ko_vo_pack = pack.path;
                                this_version.ko_vo_pack_md5 = pack.md5;
                                break;
                        }
                    }
                }
            }
            return this_version;
        }

        public static VersionInfoJSON DeserializeVersionInfoJSON(string path)
        {
            var serializer = new JsonSerializer();

            using (var sw = new StreamReader(path))
            {
                string json = sw.ReadToEnd();
                return JsonConvert.DeserializeObject<VersionInfoJSON>(json);
            }
        }

        public static void ExtractZIP(string path, string zip)
        {
            Process.Start("7za.exe", "-o" + path + " x " + zip + " -y ");
        }

        public static string GetFileName(string url)
        {
            return Path.GetFileName(new Uri(url).AbsolutePath);
        }

        public static bool GetVersionInfo()
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(Properties.Settings.Default.UpdateInfoURI, "versioninfo.json");
                    return true;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }

        public static bool GetVersionInfo(string uri)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(uri, "versioninfo.json");
                    return true;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }

        public static Servers serverInfo()
        {
            var servers = new Servers();
            {
                Ping pinger = new Ping();
                byte[] packet = new byte[64];
                var res = (HttpWebResponse)WebRequest.Create(servers.AS_Uri + "/query_cur_region").GetResponse();
                if ((int)res.StatusCode == 200)
                {
                    servers.AS_Status = true;
                    try
                    {
                        var reply = pinger.Send(servers.AS_Ping_Addr, 10000, packet);
                        servers.AS_Ping = reply.RoundtripTime;
                    }
                    catch (PingException)
                    {
                        servers.AS_Ping = -1;
                    }
                }

                res = (HttpWebResponse)WebRequest.Create(servers.TW_Uri + "/query_cur_region").GetResponse();
                if ((int)res.StatusCode == 200)
                {
                    servers.TW_Status = true;
                    try
                    {
                        var reply = pinger.Send(servers.TW_Ping_Addr, 10000, packet);
                        servers.TW_Ping = reply.RoundtripTime;
                    }
                    catch (PingException)
                    {
                        servers.TW_Ping = -1;
                    }
                }

                res = (HttpWebResponse)WebRequest.Create(servers.EU_Uri + "/query_cur_region").GetResponse();
                if ((int)res.StatusCode == 200)
                {
                    servers.EU_Status = true;
                    try
                    {
                        var reply = pinger.Send(servers.EU_Ping_Addr, 10000, packet);
                        servers.EU_Ping = reply.RoundtripTime;
                    }
                    catch (PingException)
                    {
                        servers.EU_Ping = -1;
                    }
                }

                res = (HttpWebResponse)WebRequest.Create(servers.NA_Uri + "/query_cur_region").GetResponse();
                if ((int)res.StatusCode == 200)
                {
                    servers.NA_Status = true;
                    try
                    {
                        var reply = pinger.Send(servers.NA_Ping_Addr, 10000, packet);
                        servers.NA_Ping = reply.RoundtripTime;
                    }
                    catch (PingException)
                    {
                        servers.NA_Ping = -1;
                    }
                }
            }

            return servers;
        }
    }
    public class VersionDownloadInfo
    {
        public string version;
        public string base_game_download;
        public string base_game_download_md5;
        
        public string en_vo_pack;
        public string jp_vo_pack;
        public string cn_vo_pack;
        public string ko_vo_pack;

        public string en_vo_pack_md5;
        public string jp_vo_pack_md5;
        public string cn_vo_pack_md5;
        public string ko_vo_pack_md5;
    }

    public class Servers
    {
        public string AS_Uri = "https://osasiadispatch.yuanshen.com";
        public string TW_Uri = "https://oschtdispatch.yuanshen.com";
        public string EU_Uri = "https://oseurodispatch.yuanshen.com";
        public string NA_Uri = "https://osusadispatch.yuanshen.com";

        public string AS_Ping_Addr = "oss-ap-northeast-1.aliyuncs.com";
        public string NA_Ping_Addr = "oss-us-east-1.aliyuncs.com";
        public string EU_Ping_Addr = "oss-eu-central-1.aliyuncs.com";
        public string TW_Ping_Addr = "oss-ap-northeast-1.aliyuncs.com";

        public bool AS_Status = false;
        public bool TW_Status = false;
        public bool EU_Status = false ;
        public bool NA_Status = false;

        public long AS_Ping = -1;
        public long TW_Ping = -1;
        public long EU_Ping = -1;
        public long NA_Ping = -1;
    }
}
