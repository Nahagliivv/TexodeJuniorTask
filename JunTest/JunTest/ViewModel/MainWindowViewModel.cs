using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using JunTest.Model;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Linq;
using OxyPlot;
using System.Windows.Input;
using System.Xml.Serialization;
using Splat;

namespace JunTest.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<CommonUserInfo> UsersInfoList {get;set;} //лист с информацией о пользователях
        public ObservableCollection<DataPoint> DaysAndStepsPoints { get; set; } //точки всего графика
        public ObservableCollection<DataPoint> MaxSteps { get; set; }//точка с максимальным значением шагов за период
        public ObservableCollection<DataPoint> MinSteps { get; set; }//точка с минимальным значением шагов за период
        public string NameSavedFile { get; set; } = "";

        public CommonUserInfo FocusUser { get; set; }
        public string SelectedExtended { get; set; } = "";
        List<Thread> threads;

        private IDialogService dialogService;
        public MainWindowViewModel(IDialogService dialogService)
        {
            threads = new List<Thread>();
            DaysAndStepsPoints = new ObservableCollection<DataPoint>();
            UsersInfoList = new ObservableCollection<CommonUserInfo>();
            MinSteps = new ObservableCollection<DataPoint>();
            MaxSteps = new ObservableCollection<DataPoint>();
            DeserializeUsers();
            ClearThreads();

            this.dialogService = dialogService;
        }

        public MainWindowViewModel() : this(Locator.Current.GetService<IDialogService>())
        {

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
                threads.Clear();
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
            if(FocusUser == null) { dialogService.ShowMessage("Для отображения графика нужно выбрать пользователя из таблицы"); return; }
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
            if (SelectedExtended == "" || FocusUser == null || NameSavedFile =="") { dialogService.ShowMessage("Для сохранения файла необходимо выбрать формат, имя и пользователя из таблицы"); return; }
             

            if (dialogService.OpenFolder() is string folder)
            {
                try
                {
                    if (folder == "" || !File.Exists(folder) && !Directory.Exists(folder)) 
                    {
                        dialogService.ShowMessage("Файл невозможно сохранить, т.к. не выбран путь или нет разрешения на сохранение в выбранном директории");
                        return; 
                    }
                    var savedUser = new SerializeUserInfo() { AvgSteps = FocusUser.AvgSteps, MaxSteps = FocusUser.MaxSteps, MinSteps = FocusUser.MinSteps, Rank = FocusUser.Rank, Status = FocusUser.Status, User = FocusUser.User };
                    if (SelectedExtended == "XML")
                    {
                        XmlSerializer formatter = new XmlSerializer(typeof(SerializeUserInfo));

                        using (FileStream fs = new FileStream(Path.Combine(folder, NameSavedFile + ".xml"), FileMode.OpenOrCreate))
                        {
                            formatter.Serialize(fs, savedUser);
                        }
                    }
                    if (SelectedExtended == "CSV")
                    {
                        StreamWriter myOutputStream = new StreamWriter(Path.Combine(folder, NameSavedFile + ".csv"));
                        myOutputStream.WriteLine(savedUser.ToString());
                        myOutputStream.Close();
                    }
                    if (SelectedExtended == "JSON")
                    {
                        using (FileStream fs = new FileStream(Path.Combine(folder,NameSavedFile + ".json"), FileMode.OpenOrCreate))
                        {
                            System.Text.Json.JsonSerializer.SerializeAsync<SerializeUserInfo>(fs, savedUser);
                        }

                    }
                }
                catch { dialogService.ShowMessage("Не удалось сохранить файл"); return; }
            }

            }
        }
    }

