using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace JunTest.Model
{
    public class CommonUserInfo 
    {
        public int Rank { get; set; }
        public string User { get; set; }
        public string Status { get; set; }
        public int Steps { get; set; }

        public List<UserInfo> AllUserInfo;
        public int WorkDays;
        public int AvgSteps { get; set; }
        public int MinSteps { get; set; }
        public int MaxSteps { get; set; }
        public int CurrentDay;
        public Brush StatusColor { get; set; }
        public CommonUserInfo()
        {
            AllUserInfo = new List<UserInfo>();
            CurrentDay = 1;
            StatusColor = Brushes.Red;
        }
    }
}
