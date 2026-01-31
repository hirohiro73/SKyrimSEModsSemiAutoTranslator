using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SKyrimSEModsSemiAutoTranslator
{
    internal class RunProcessHelper
    {
        static public void RunProcess(string exePath, string arguments)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = exePath;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                p.WaitForExit();
            }
        }
    }
}
