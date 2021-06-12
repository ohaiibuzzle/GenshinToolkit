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

        static public bool? check_download_Aria()
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

        public static bool getVersionInfoGH()
        {
            try {
                using (var client = new WebClient())
                {
                    client.DownloadFile("https://raw.githubusercontent.com/ohaiibuzzle/GSToolkit/senpai/static/versioninfo.json", "versioninfo.json");
                    return true;
                } 
            }
            catch (WebException)
            {
                return false;
            }
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
}
