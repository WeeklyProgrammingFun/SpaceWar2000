using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRunner.Model
{
    // Represent a player - has name, ability to communicate with bot
    public class Player : ObservableObject
    {
        public Player(string filename)
        {
            Filename = filename;
            proc = new ProcessCapture(filename);
        }

        ProcessCapture proc;

        private string filename = "";

        public string Filename
        {
            get { return filename; }
            set { Set(ref filename, value); }
        }

        private bool enabled = true;

        public bool Enabled
        {
            get { return enabled; }
            set { Set(ref enabled, value); }
        }

        public override string ToString()
        {
            return Filename;
        }

        public void Write(string text)
        {
            proc.Write(text);
        }

        public List<string> Read(int msDelay)
        {
            var curMs = Environment.TickCount;
            var ans = new List<String>();
            do
            {
                var temp = proc.Read(false);
                ans.AddRange(temp);
            } while (!ans.Any() && Environment.TickCount <= curMs + msDelay);

            return ans;
        }

        internal void Close()
        {
            proc.Write("QUIT E");
            var exited = proc.WaitForExit(1000);
            Debug.Assert(exited);
        }
    }
}
