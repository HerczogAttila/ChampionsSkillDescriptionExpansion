using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ChampionsSkillDescriptionExpansion
{
    public partial class WindowProgress : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Message { get; set; }
        public Visibility VisProgress { get; set; }
        public Visibility VisCompleted { get; set; }

        private Collection<ChampionList> Champions;
        private ChampionList ChampionList;
        private LanguageStrings LanguageString;
        private SQLiteConnection sql;
        private Thread thread;
        private CancellationTokenSource cts;
        private Config Config;
        private string PathDownload, PathRelease;

        private static string LogTime => DateTime.Now.ToLongTimeString();

        public WindowProgress(Config config)
        {
            InitializeComponent();

            Champions = new Collection<ChampionList>();

            Message = "";
            VisProgress = Visibility.Visible;
            VisCompleted = Visibility.Collapsed;

            Config = config;

            if(config != null)
            {
                PathRelease = @"data\backup\\" + Config.DirectoryRelease + "\\";
                PathDownload = "data\\" + Config.Version + "_" + Config.Lang + "\\";
            }

            DataContext = this;
        }

        private async void UpdateSkillDescription()
        {
            if (!Directory.Exists(PathRelease))
            {
                Directory.CreateDirectory(PathRelease);

                AddMessage("Biztonsági mentés készítése a következő fájlokról:");

                foreach (var v in Directory.GetFiles(Config.PathInstall))
                {
                    File.Copy(v, PathRelease + Path.GetFileName(v), false);

                    AddMessage("-" + v);
                }
            }

            File.Copy(Config.PathInstall + "\\gameStats_" + Config.Lang + ".sqlite", @"data\temp\gameStats_" + Config.Lang + ".sqlite", true);

            if (!Directory.Exists(PathDownload))
                Directory.CreateDirectory(PathDownload);

            cts = new CancellationTokenSource();

            try
            {
                await LoadLanguageString();
                await LoadChampionList();
                await LoadChampionsData(cts.Token);
            }
            catch (OperationCanceledException) { return; }
            finally
            {
                cts = null;
            }

            sql = new SQLiteConnection(@"Data Source=data\temp\gameStats_" + Config.Lang + ".sqlite;Version=3;");
            sql.Open();

            string desc;
            int i;
            foreach (var v in Champions)
                foreach (var v2 in v.Data.Values)
                {
                    i = 2;
                    foreach (var v3 in v2.Spells)
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(sql))
                        {
                            desc = v3.MakeDescription(LanguageString);
                            cmd.CommandText = "update championAbilities set description = :desc where rank=:rank and championId=:id";
                            cmd.Parameters.Add("desc", DbType.String).Value = desc;
                            cmd.Parameters.Add("rank", DbType.String).Value = i;
                            cmd.Parameters.Add("id", DbType.String).Value = v2.Key;
                            cmd.ExecuteNonQuery();
                        }
                        i++;
                    }

                    /*using (SQLiteCommand cmd = new SQLiteCommand(m_dbConnection))
                    {
                        cmd.CommandText = "update championAbilities set description = :desc where rank=:rank and championId=:id";
                        cmd.Parameters.Add("desc", DbType.String).Value = v2.Passive.Description;
                        cmd.Parameters.Add("rank", DbType.String).Value = 1;
                        cmd.Parameters.Add("id", DbType.String).Value = v2.Key;
                        cmd.ExecuteNonQuery();
                    }*/

                    AddMessage("Hős képességek leírásának bővítése: " + v2.Id);
                }

            var path = Config.PathInstall + "\\gameStats_" + Config.Lang + ".sqlite";
            File.Copy(@"data\temp\gameStats_" + Config.Lang + ".sqlite", path, true);

            sql.Close();

            File.Delete(@"data\temp\gameStats_" + Config.Lang + ".sqlite");

            AddMessage("Fájl fellülírva: " + path);
            AddMessage("Hős képességek leírásának bővítése '" + Config.Lang + "' nyelven befejezve.");

            Dispatcher.Invoke(() =>
            {
                VisProgress = Visibility.Collapsed;
                VisCompleted = Visibility.Visible;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VisProgress"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VisCompleted"));
            });
        }
        private static string FixArray(string data)
        {
            int i, i2 = 0;
            i = data.IndexOf("coeff\":", StringComparison.Ordinal);
            while (i > -1)
            {
                if (data[i + 7] != '[')
                {
                    i2 = data.IndexOf(",", i, StringComparison.Ordinal);
                    if (i2 > -1)
                    {
                        data = data.Insert(i2, "]");
                        data = data.Insert(i + 7, "[");
                    }
                }
                else
                    i2 = i + 7;

                i = data.IndexOf("coeff\":", i2, StringComparison.Ordinal);
            }
            return data;
        }

        private async Task LoadChampionList()
        {
            var path = PathDownload + "champions.json";
            
            string data;
            if (File.Exists(path))
                data = File.ReadAllText(path);
            else
            {
                var link = "http://ddragon.leagueoflegends.com/cdn/" + Config.Version + "/data/" + Config.Lang + "/champion.json";
                AddMessage("Fájl letöltés: " + link);
                data = await link.DownloadAndSave(path);
            }

            ChampionList = JsonConvert.DeserializeObject<ChampionList>(data);
        }
        private async Task LoadLanguageString()
        {
            var path = PathDownload + "lang.json";
            
            string data;
            if (File.Exists(path))
                data = File.ReadAllText(path);
            else
            {
                var link = "http://ddragon.leagueoflegends.com/cdn/" + Config.Version + "/data/" + Config.Lang + "/language.json";
                AddMessage("Fájl letöltés: " + link);
                data = await link.DownloadAndSave(path);
            }

            LanguageString = JsonConvert.DeserializeObject<LanguageStrings>(data);
        }
        private async Task LoadChampionsData(CancellationToken token)
        { 
            var pathSkill = PathDownload + "skilldescription.json";
            if (File.Exists(pathSkill))
                Champions = JsonConvert.DeserializeObject<Collection<ChampionList>>(File.ReadAllText(pathSkill));
            else
            {
                string path, link, data;
                foreach (var v in ChampionList.Data.Values)
                {
                    token.ThrowIfCancellationRequested();
                    path = PathDownload + v.Id + ".json";

                    if (File.Exists(path))
                        data = File.ReadAllText(path);
                    else
                    {
                        link = "http://ddragon.leagueoflegends.com/cdn/" + Config.Version + "/data/" + Config.Lang + "/champion/" + v.Id + ".json";
                        AddMessage("Fájl letöltés: " + link);
                        data = await link.Download();

                        data = FixArray(data);

                        File.WriteAllText(path, data);
                    }
                    try
                    {
                        Champions.Add(JsonConvert.DeserializeObject<ChampionList>(data));
                    }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                    
                }

                File.WriteAllText(pathSkill, JsonConvert.SerializeObject(Champions));
            }
        }

        private void AddMessage(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                txtMsg.AppendText("[" + LogTime + "] " + msg + "\r\n");
                txtMsg.ScrollToEnd();
            }, DispatcherPriority.Background);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            thread = new Thread(new ThreadStart(UpdateSkillDescription));
            thread.Start();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }

            if (sql != null)
                sql.Close();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }

            if (sql != null)
                sql.Close();

            File.Delete(@"data\temp\gameStats_" + Config.Lang + ".sqlite");

            if (cts != null)
                cts.Cancel();

            Close();
        }
    }
}
