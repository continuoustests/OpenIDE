using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
namespace OpenIDENet.Arguments.Handlers
{
	class VeiHandler : ICommandHandler
	{
		public string Command { get { return "vei"; } }
		
		public void Execute (string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation)
		{
			var veiFile = getFile();
			if (veiFile == null)
				return;
			
			Console.WriteLine("Loading environment ({0})", veiFile);
			var process = new Process();
			process.StartInfo = new ProcessStartInfo(veiFile, Environment.CurrentDirectory);
			process.StartInfo.CreateNoWindow = false;
			process.StartInfo.UseShellExecute = true;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			process.Start();
			process.WaitForExit();
			Console.WriteLine("Go crazy!");
		}
		
		private string getFile()
		{
			var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var file = Path.Combine(assemblyDir, "initialize_openide");
			if (File.Exists(file))
				return file;
			file = Path.Combine(assemblyDir, "initialize_openide.sh");
			if (File.Exists(file))
				return file;
			file = Path.Combine(assemblyDir, "initialize_openide.bat");
			if (File.Exists(file))
				return file;
			file = Path.Combine(assemblyDir, "initialize_openide.rb");
			if (File.Exists(file))
				return file;
			return null;
		}
	}
}