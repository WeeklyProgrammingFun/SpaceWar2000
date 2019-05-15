using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRunner.SpaceWar2K
{
    public class FrameInfo
    {
        public State State { get; set; }

        public ObservableCollection<string> Messages { get;  } = new ObservableCollection<string>();

        public void Write(string name, string msg)
        {
            Messages.Add($"{name} <= {Clean(msg)}");
        }

        public void Read(string name, string msg)
        {
            Messages.Add($"{name} => {Clean(msg)}");
        }

        string Clean(string msg)
        {
            return msg.Replace("\n", "").Replace("\r", "");
        }

        // error msg
        public string Error { get; set; } = "";
    }
}
