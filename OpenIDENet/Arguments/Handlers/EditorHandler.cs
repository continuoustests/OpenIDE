using System;
using System.Linq;
using OpenIDENet.EditorEngineIntegration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
namespace OpenIDENet.Arguments.Handlers
{
	class EditorHandler : ICommandHandler
	{
		private ILocateEditorEngine _editorFactory;
		
		public string Command { get { return "editor"; } }
		
		public EditorHandler(ILocateEditorEngine editorFactory)
		{
			_editorFactory = editorFactory;
		}
		
		public void Execute(string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation)
		{
			if (arguments.Length != 1)
			{
				Console.WriteLine("Invalid number of arguments. Useage: editor {editor name}");
				return;
			}
			var instance = _editorFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				instance = startInstance();
			instance.Start(arguments[0].Trim());
			runInitScript();
		}
		
		private Instance startInstance()
		{
			var exe = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EditorEngine"), "EditorEngine.exe");
			var proc = new Process();
			proc.StartInfo = new ProcessStartInfo(exe, Environment.CurrentDirectory);
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
			proc.Start();
            var timeout = DateTime.Now.AddSeconds(5);
            while (DateTime.Now < timeout)
            {
                if (_editorFactory.GetInstance(Environment.CurrentDirectory) != null)
                    break;
                Thread.Sleep(50);
            }
            return _editorFactory.GetInstance(Environment.CurrentDirectory);
		}
		
		private void runInitScript()
		{
			var appdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var initscript = Directory.GetFiles(appdir, "initialize.*").FirstOrDefault();
			if (initscript == null)
				return;
			var proc = new Process();
			proc.StartInfo = new ProcessStartInfo(initscript, "\"" + Environment.CurrentDirectory + "\"");
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.WorkingDirectory = appdir;
			proc.Start();
		}
	}
}

