using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace CoreExtensions
{
    public static class ProcessExtensions
    {
        public static void Run(
            this Process proc,
            string command,
            string arguments,
            bool visible,
            string workingDir)
        {
            prepareProcess(proc, command, arguments, visible, workingDir);
            proc.Start();
			proc.WaitForExit();
        }

        public static void Query(
            this Process proc,
            string command,
            string arguments,
            bool visible,
            string workingDir,
			Action<string> onRecievedLine)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix &&
                Environment.OSVersion.Platform != PlatformID.MacOSX)
            {
                arguments = "/c " +
                    "^\"" + command + "^\" " +
                    arguments.Replace("\"", "^\"");
                command = "cmd.exe";
            }
			
			var exit = false;
            prepareProcess(proc, command, arguments, visible, workingDir);
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
			proc.OutputDataReceived += (s, data) => {
					if (data.Data == null)
						exit = true;
					else
						onRecievedLine(data.Data);
				};
            if (proc.Start())
            {
				proc.BeginOutputReadLine();
				while (!exit && !proc.HasExited)
					System.Threading.Thread.Sleep(10);
            }
        }

        private static void prepareProcess(
            Process proc,
            string command,
            string arguments,
            bool visible,
            string workingDir)
        {
            var info = new ProcessStartInfo(command, arguments);
            info.CreateNoWindow = !visible;
            if (!visible)
                info.WindowStyle = ProcessWindowStyle.Hidden;
            info.WorkingDirectory = workingDir;
            proc.StartInfo = info;
        }
    }
}
