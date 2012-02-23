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
        }

        public static IEnumerable<string> Query(
            this Process proc,
            string command,
            string arguments,
            bool visible,
            string workingDir)
        {
            var outputFile = Path.GetTempFileName();
            var batch = outputFile + ".bat";
            if (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                prepareProcess(proc, command, arguments, visible, workingDir);
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
            }
            else
            {
                File.WriteAllText(batch, command + " " + arguments + " > " + outputFile);
                prepareProcess(proc, batch, "", visible, workingDir);
            }

            if (proc.Start())
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix ||
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    while (true)
                    {
                        var line = proc.StandardOutput.ReadLine();
                        if (line == null)
                            break;
                        yield return line;
                    }
                }
                else
                {
                    proc.WaitForExit();
                    if (File.Exists(outputFile))
                    {
                        foreach (var line in File.ReadAllLines(outputFile))
                            yield return line;
                        File.Delete(outputFile);
                    }
                    File.Delete(batch);
                }
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
