using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunTest.ViewModel
{
    internal interface IDialogService
    {
        string OpenFolder();
        void ShowMessage(string message);
    }
}
