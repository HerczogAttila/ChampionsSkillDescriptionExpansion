using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace ChampionsSkillDescriptionExpansion
{
    public class Config
    {
        private static string LinkLanguages = "https://ddragon.leagueoflegends.com/cdn/languages.json";
        private static string LinkVersions = "https://ddragon.leagueoflegends.com/api/versions.json";
        private static string Path = "data\\config.json";

        public Collection<string> Languages { get; private set; }
        public LanguageStrings LanguageStrings { get; set; }
        public string Version { get; set; }
        public string PathInstallRoot { get; set; }
        public string FullLang { get; set; }
        public string Lang { get; set; }
        public string DirectoryRelease { get; set; }
        public bool IsUpToDate { get; set; }

        [JsonIgnore]
        public string PathInstall { get; set; }

        public Config()
        {
            Languages = new Collection<string>();
            LanguageStrings = new LanguageStrings();
            Version = "";
            PathInstallRoot = "";
            Lang = "";
            PathInstall = "";
            DirectoryRelease = "";
            IsUpToDate = false;
        }

        public async Task DownloadVersions()
        {
            var data = await LinkVersions.Download();
            var versions = JsonConvert.DeserializeObject<Collection<string>>(data);
            if (versions != null)
                if (versions.Count > 0)
                {
                    if (!Version.Equals(versions[0]))
                        IsUpToDate = true;

                    Version = versions[0];
                }
                    
        }
        public async Task DownloadLanguages()
        {
            var data = await LinkLanguages.Download();
            Languages = JsonConvert.DeserializeObject<Collection<string>>(data);
        }
        public async Task DownloadLanguageStrings()
        {
            var link = "http://ddragon.leagueoflegends.com/cdn/" + Version + "/data/en_US/language.json";
            var data = await link.Download();
            LanguageStrings = JsonConvert.DeserializeObject<LanguageStrings>(data);
        }

        public void Save()
        {
            var data = JsonConvert.SerializeObject(this);
            File.WriteAllText(Path, data);
        }
        public static Config Load()
        {
            if (File.Exists(Path))
            {
                var data = File.ReadAllText(Path);
                return JsonConvert.DeserializeObject<Config>(data);
            }

            return new Config();
        }
    }
}
