using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinToolkit {
    public class VoicePack
    {
        public string language;
        public string name;
        public string path;
        public string size;
        public string md5;
    }

    public class Latest
    {
        public string name;
        public string version;
        public string path;
        public string size;
        public string md5;
        public string entry;
        public List<VoicePack> voice_packs;
        public string decompressed_path;
    }

    public class Diff
    {
        public string name;
        public string version;
        public string path;
        public string size;
        public string md5;
        public bool is_recommended_update;
        public List<VoicePack> voice_packs;
    }

    public class Game
    {
        public Latest latest;
        public List<Diff> diffs;
    }

    public class DeprecatedPackage
    {
        public string name;
        public string md5;
    }

    public class Data
    {
        public Game game;
        public List<DeprecatedPackage> deprecated_packages;
    }

    public class VersionInfoJSON
    {
        public Data data;
    }

    public class FileHashInfo
    {
        public string remoteName;
        public string md5;
        public int fileSize;
    }
}