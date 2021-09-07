using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using JunTest.Model;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Linq;
using OxyPlot;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Xml.Serialization;
using System.Windows.Forms;
namespace JunTest.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<CommonUserInfo> UsersInfoList {get;set;}
        public ObservableCollection<DataPoint> DaysAndStepsPoints { get; set; }
        public ObservableCollection<DataPoint> MaxSteps { get; set; }
        public ObservableCollection<DataPoint> MinSteps { get; set; }
        public string NameSavedFile { get; set; } = "";
        public CommonUserInfo FocusUser { get; set; }
        public string SelectedExtended { get; set; } = "";
        List<Thread> threads;
        public MainWindowViewModel()
        {
            threads = new List<Thread>();
            DaysAndStepsPoints = new ObservableCollection<DataPoint>();
            UsersInfoList = new ObservableCollection<CommonUserInfo>();
            MinSteps = new ObservableCollection<DataPoint>();
            MaxSteps = new ObservableCollection<DataPoint>();
           DeserializeUsers();
            ClearThreads();

        }
         void DeserializeUsers()
        {

           var allUsersText = File.ReadAllText(@"Days\day1.json");
           foreach(var x in JsonConvert.DeserializeObject<ObservableCollection<CommonUserInfo>>(allUsersText))
            {
               
                Thread thr = new Thread(() =>
                {
                    while (true)
                    {
                        if (!File.Exists(@"Days\day" + x.CurrentDay + ".json")) { break; }
                        string allUsersTextSearch;
                        try
                        {
                            allUsersTextSearch = File.ReadAllText(@"Days\day" + x.CurrentDay + ".json");
                        }
                        catch { continue; }
                        foreach (var z in JsonConvert.DeserializeObject<ObservableCollection<UserInfo>>(allUsersTextSearch))
                        {
                            if (x.User == z.User)
                            {
                                z.Day = x.CurrentDay;
                                x.AllUserInfo.Add(z);
                                x.WorkDays++;
                                x.CurrentDay++;
                                break;
                            }
                        }
                    }
                    x.MaxSteps = x.AllUserInfo.Max(y => y.Steps);
                    x.MinSteps = x.AllUserInfo.Min(y => y.Steps);
                    x.AvgSteps = (int)x.AllUserInfo.Average(y => y.Steps);
                    var checkedProcent = (x.AvgSteps / 100) * 20;
                    var MinStat = x.AvgSteps - x.MinSteps;
                    var MaxStat = x.MaxSteps - x.AvgSteps;
                    if (checkedProcent < MinStat || MaxStat > checkedProcent)
                    {
                        x.StatusColor = Brushes.Red;
                    }
                    else
                    {
                        x.StatusColor = Brushes.White;
                    }
                    UsersInfoList.Add(x);
                });
                thr.IsBackground = true;
                thr.Start();
                threads.Add(thr);
            }
            
        }
       
        async void ClearThreads()//очистка потоков
        {
            await Task.Run(() =>
            {
                bool isAliveThreadExist = true;
                while (isAliveThreadExist)
                {
                    foreach (var x in threads)
                    {
                        if (!x.IsAlive) { isAliveThreadExist = false; break; }
                    }
                }
                foreach (var x in threads)
                {
                    x.Abort();
                }
            });
        }
        public ICommand DrawGraph
        {
            get
            {
                return new DelegateCommand(() => { DrawGrph(); });
            }
        }
        void DrawGrph() //рисование графика
        {
            if(FocusUser == null) { MessageBox.Show("Для отображения графика нужно выбрать пользователя из таблицы"); return; }
            DaysAndStepsPoints.Clear();
            MinSteps.Clear();
            MaxSteps.Clear();
            foreach(var x in FocusUser.AllUserInfo)
            {
                DaysAndStepsPoints.Add(new DataPoint(x.Day, x.Steps));
            }
            var maxstepsday = FocusUser.AllUserInfo.OrderByDescending(p => p.Steps).FirstOrDefault();
            var minstepsday = FocusUser.AllUserInfo.OrderBy(p => p.Steps).FirstOrDefault();
            MaxSteps.Add(new DataPoint(maxstepsday.Day, maxstepsday.Steps));
            MinSteps.Add(new DataPoint(minstepsday.Day, minstepsday.Steps));
        }
        public ICommand SaveInfo
        {
            get
            {
                return new DelegateCommand(() => { SavInf(); });
            }
        }
        void SavInf()// сохранение информации выбранного юзера
        {
            if (SelectedExtended == "" || FocusUser == null || NameSavedFile =="") { MessageBox.Show("Для сохранения файла необходимо выбрать формат, имя и пользователя из таблицы"); return; }
            var dlg = new FolderBrowserDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (dlg.SelectedPath == "" || !System.IO.File.Exists(dlg.SelectedPath) && !System.IO.Directory.Exists(dlg.SelectedPath)) { MessageBox.Show("Файл невозможно сохранить, т.к. не выбран путь или нет разрешения на сохранение в выбранном директории"); return; }
                    var folder = dlg.SelectedPath;
                    var savedUser = new SerializeUserInfo() { AvgSteps = FocusUser.AvgSteps, MaxSteps = FocusUser.MaxSteps, MinSteps = FocusUser.MinSteps, Rank = FocusUser.Rank, Status = FocusUser.Status, User = FocusUser.User };
                    if (SelectedExtended == "XML")
                    {
                        XmlSerializer formatter = new XmlSerializer(typeof(SerializeUserInfo));

                        using (FileStream fs = new FileStream(folder + NameSavedFile + ".xml", FileMode.OpenOrCreate))
                        {
                            formatter.Serialize(fs, savedUser);
                        }
                    }
                    if (SelectedExtended == "CSV")
                    {
                        StreamWriter myOutputStream = new StreamWriter(folder + NameSavedFile + ".csv");
                        myOutputStream.WriteLine(savedUser.ToString());
                        myOutputStream.Close();
                    }
                    if (SelectedExtended == "JSON")
                    {
                        using (FileStream fs = new FileStream(folder + NameSavedFile + ".json", FileMode.OpenOrCreate))
                        {
                            System.Text.Json.JsonSerializer.SerializeAsync<SerializeUserInfo>(fs, savedUser);
                        }

                    }
                }
                catch { MessageBox.Show("Не удалось сохранить файл"); return; }
            }

            }
        }
    }

