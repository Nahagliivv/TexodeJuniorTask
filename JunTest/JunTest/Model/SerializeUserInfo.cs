using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunTest.Model
{   [Serializable]
    public class SerializeUserInfo
    {
        public string User { get; set; }
        public int MaxSteps { get; set; }
        public int MinSteps { get; set; }
        public int AvgSteps { get; set; }
        public string Status { get; set; }
        public int Rank { get; set; }
        public override string ToString()
        {
            return $"{User} {MaxSteps} {MinSteps} {AvgSteps} {Status} {Rank}";
        }
    }
}
