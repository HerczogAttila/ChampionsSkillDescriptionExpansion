using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ChampionsSkillDescriptionExpansion
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Languages { get; private set; }
        private Collection<string> LangCodes;
        public Visibility VisMenu { get; set; }
        public Visibility VisUpToDate { get; set; }
        public Config Config { get; set; }
        public int LangIndex { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Languages = new ObservableCollection<string>();
            LangCodes = new Collection<string>();
            VisMenu = Visibility.Collapsed;
            VisUpToDate = Visibility.Collapsed;
            DataContext = this;
        }

        private async Task<string> GetFullPath(string path)
        {
            path += @"RADS\projects\lol_air_client\releases\";
            if (Directory.Exists(path))
            {
                var dirs = Directory.GetDirectories(path);
                foreach (var v in dirs)
                {
                    path = v + @"\deploy\assets\data\gameStats";
                    if (Directory.Exists(path))
                    {
                        var index = v.LastIndexOf("\\", StringComparison.Ordinal) + 1;
                        var dirRelease = v.Substring(index);
                        if((!string.IsNullOrEmpty(Config.DirectoryRelease) && !dirRelease.Equals(Config.DirectoryRelease)) || !Config.IsUpToDate)
                        {
                            Config.IsUpToDate = false;
                            await Config.DownloadVersions();
                            await Config.DownloadLanguages();
                        }

                        Config.DirectoryRelease = dirRelease;
                        break;
                    }
                    else
                        path = string.Empty;
                }
            }

            return path;
        }
        private async Task FindInstallDirectory()
        {
            var path = Config.PathInstall;

            if (string.IsNullOrEmpty(path))
                path = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Riot Games\League of Legends", "Path", string.Empty);
            if (string.IsNullOrEmpty(path))
                path = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Riot Games\League of Legends", "Path", string.Empty);

            if (string.IsNullOrEmpty(path))
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.ShowNewFolderButton = false;
                    dialog.Description = "Kérem adja meg a League of Legends telepítési helyét!";

                    do
                    {
                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            Config.PathInstallRoot = dialog.SelectedPath;
                            path = await GetFullPath(dialog.SelectedPath);

                            if (!Directory.Exists(path))
                            {
                                System.Windows.MessageBox.Show("A megadott könyvtárban nem találhatók meg a szükséges adatok. Kérem válasszon másikat!", "Hiba!");
                                path = string.Empty;
                            }
                        }
                        else
                            Close();
                    } while (string.IsNullOrEmpty(path));
                }
            }
            else
            {
                Config.PathInstallRoot = path;
                path = await GetFullPath(path);
            }

            Config.PathInstall = path;
        }

        private void ReadLanguages()
        {
            var files = Directory.GetFiles(Config.PathInstall);
            string lang;
            Languages.Clear();
            LangCodes.Clear();
            foreach (var v in files)
            {
                lang = Path.GetFileNameWithoutExtension(v).Replace("gameStats_", "");
                if (Config.Languages.Contains(lang))
                {
                    Languages.Add(Config.LanguageStrings.GetLang(lang) + " (" + lang + ")");
                    LangCodes.Add(lang);
                }
            }
        }

        private void BtnExpand_Click(object sender, RoutedEventArgs e)
        {
            Config.Lang = LangCodes[LangIndex];
            Config.Save();
            var win = new WindowProgress(Config);
            win.ShowDialog();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var dataPath = "data";
            var tempPath = "data\\temp";
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            Config = Config.Load();

            if (string.IsNullOrEmpty(Config.Version))
                await Config.DownloadVersions();

            if (Config.Languages.Count == 0)
                await Config.DownloadLanguages();

            if (Config.LanguageStrings.Data.Count == 0)
                await Config.DownloadLanguageStrings();

            await FindInstallDirectory();
            ReadLanguages();

            Config.Save();

            if (!Config.IsUpToDate)
                VisUpToDate = Visibility.Visible;

            VisMenu = Visibility.Visible;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Config"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VisMenu"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VisUpToDate"));
        }
        private void BtnRestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(@"data\backup\" + Config.DirectoryRelease))
            {
                var pathBackup = @"data\backup\" + Config.DirectoryRelease + "\\gameStats_" + Config.Lang + ".sqlite";
                var pathDest = Config.PathInstall + "\\gameStats_" + Config.Lang + ".sqlite";
                File.Copy(pathBackup, pathDest, true);
                System.Windows.MessageBox.Show("'" + pathDest + "' fájl helyreállítása megtörtént.", "Információ!");
            }
            else
                System.Windows.MessageBox.Show("Ehhez a verzióhoz nem készült biztonsági mentés!", "Hiba!");
        }
        private async void BtnSearchUpdate_Click(object sender, RoutedEventArgs e)
        {
            var oldversion = Config.Version;
            await Config.DownloadVersions();
            await Config.DownloadLanguages();
            await Config.DownloadLanguageStrings();
            ReadLanguages();

            if (Config.IsUpToDate)
                VisUpToDate = Visibility.Visible;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Config"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VisUpToDate"));

            if (oldversion.Equals(Config.Version))
                System.Windows.MessageBox.Show("Nem található új frissítés.", "Információ!");
            else
                System.Windows.MessageBox.Show("Frissítések telepítve.", "Információ!");
        }
    }
}

//korábbi adatok és backup-ok törlése