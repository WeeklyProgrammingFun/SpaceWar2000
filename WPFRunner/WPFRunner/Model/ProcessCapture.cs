using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRunner.Model
{
    class ProcessCapture
    {
        // anything other than black shows debug messages 
        public ConsoleColor color = ConsoleColor.Black;

        /// <summary>
        /// Given filename to execute, do so
        /// </summary>
        /// <param name="filename"></param>
        public ProcessCapture(string filename)
        {

            process.EnableRaisingEvents = true;
            process.OutputDataReceived += process_OutputDataReceived;
            process.ErrorDataReceived += process_ErrorDataReceived;
            process.Exited += process_Exited;

            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = "";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            //below line is optional if we want a blocking call
            //process.WaitForExit();
        }

        /// <summary>
        /// Write a message to the item
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            Debug.WriteLine("PROC WRITE: " + message);
            WriteDebugMessage(message, true);
            process.StandardInput.WriteLine(message);
        }

        // read (and clear) any pending messages
        public List<string> Read(bool waitTillReceived)
        {
            var lines = new List<string>();
            do
            {
                while (messages.TryDequeue(out var result))
                {
                    Debug.WriteLine("PROC READ: " + result);
                    WriteDebugMessage(result, false);
                    lines.Add(result);
                }

                if (!waitTillReceived)
                    break;
                if (waitTillReceived && lines.Any())
                    break;
            } while (true);
            return lines;
        }

        public bool WaitForExit(int maxMs)
        {
            return process.WaitForExit(maxMs);
        }

        #region Implementation

        void WriteDebugMessage(string message, bool isInput)
        {
            if (color != ConsoleColor.Black)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                if (isInput)
                    message = "In: " + message;
                else
                    message = "Out:" + message;
                Console.WriteLine(message);
                Console.ForegroundColor = oldColor;
            }
        }

        void process_Exited(object sender, EventArgs e)
        {
            //Console.WriteLine(string.Format("process exited with code {0}\n", process.ExitCode.ToString()));
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // Console.WriteLine("ERROR: " + e.Data + "\n");
        }

        ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            messages.Enqueue(e.Data);
            //Console.WriteLine(e.Data + "\n");
        }

        Process process = new Process();

        #endregion
    }
}
