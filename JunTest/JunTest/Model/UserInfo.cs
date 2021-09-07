using System; 

namespace JunTest.Model
{
    [Serializable]
    public class UserInfo
    {
        public int Rank { get; set; }
        public string User { get; set; }
        public string Status { get; set; }
        public int Steps { get; set; }
        [NonSerialized]
        public int Day;
        public UserInfo()
        {

        }
    }
}
