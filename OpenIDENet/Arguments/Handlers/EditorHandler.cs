using System;
using System.Linq;
using OpenIDENet.EditorEngineIntegration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using OpenIDENet.Core.Language;
using OpenIDENet.Bootstrapping;
namespace OpenIDENet.Arguments.Handlers
{
	class EditorHandler : ICommandHandler
	{
		private ILocateEditorEngine _editorFactory;
		
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Starts the editor of your choice depending on the plugins available in ||newline||" +
					"addition to launching the code engine backend providing features like ||newline||" +
					"type search and file explorer. It also initializes the system by running ||newline||" +
					"whatever is specified in the initialize(.rb) script.");
				usage.Add("PLUGIN_NAME", "The name of the plugin to launch");
				usage.Add("goto", "Open file on spesific line and column")
					.Add("FILE|LINE|COLUMN", "| separated filepath, line and column");
				usage.Add("setfocus", "Sets focus to the editor");
				usage.Add("insert", "Inserts a chunk of text to a spesific position in a file")
					.Add("CONTENT_FILE", "File containing the text you want to insert")
						.Add("TARGET_LOCATION", "The target file and position to insert the text to. Format: /my/file.cs|LINE|COLUMN");
				usage.Add("remove", "Removes a chunk of text from a file")
					.Add("FILE_AND_START_POSITION", "The file and starting position of text to be removed. Format: /my/file.cs|LINE|COLUMN")
						.Add("END_POSITION", "The position where the text chunk to remove ends. Format: LINE|COLUMN");
				usage.Add("replace", "Replaces a chunk of text within a file")
					.Add("CONTENT_FILE", "File containing the text you want to replace with")
						.Add("FILE_AND_START_POSITION", "The file and starting position for text to be replaced. Format: /my/file.cs|LINE|COLUMN")
							.Add("END_POSITION", "The position where the text chunk to be replaced ends. Format: LINE|COLUMN");
				usage.Add("refactor", "Gives the posibility to batch up several insert, remove and replace commands")
					.Add("CONTENT_FILE", "A file containing insert, remove and replace commands. One command pr line");
				usage.Add("get-dirty-files", "Queries the editor for all modified files and their content");
				return usage;
			}
		}

		public string Command { get { return "editor"; } }
		
		public EditorHandler(ILocateEditorEngine editorFactory)
		{
			_editorFactory = editorFactory;
		}
		
		public void Execute(string[] arguments)
		{
			var instance = _editorFactory.GetInstance(Environment.CurrentDirectory);
			// TODO remove that unbeleavable nasty setfocus solution. Only init if launching editor
			if (instance == null && arguments.Length == 1 && arguments[0] != "setfocus")
			{
				instance = startInstance();
				if (instance == null)
					return;
				instance.Start(arguments[0].Trim());
				runInitScript();
			}
			else if (arguments.Length == 1 && arguments[0] == "get-dirty-files")
			{
				if (instance == null)
					return;
				Console.WriteLine(instance.GetDirtyFiles());
			}
			else
			{
				if (instance == null)
					return;
				instance.Run(arguments);
			}
		}
		
		private Instance startInstance()
		{
			var exe = Path.Combine(
				Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EditorEngine"),
				"EditorEngine.exe");
			var proc = new Process();
			proc.StartInfo = new ProcessStartInfo(exe, "\"" + Environment.CurrentDirectory + "\"");
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
			var defaultLanguage = "";
			if (Bootstrapper.Settings.DefaultLanguage != null)
				defaultLanguage = " " + Bootstrapper.Settings.DefaultLanguage;
			var enabledLanguages = "";
			if (Bootstrapper.Settings.EnabledLanguages != null)
			{
				enabledLanguages = " \"";
				Bootstrapper.Settings.EnabledLanguages.ToList()
					.ForEach(x => enabledLanguages += x + ",");
				enabledLanguages = 
					enabledLanguages
						.Substring(0, enabledLanguages.Length - 1) + "\"";
			}
			var proc = new Process();
			proc.StartInfo = new ProcessStartInfo(
				initscript,
				"\"" + Environment.CurrentDirectory + "\"" + defaultLanguage + enabledLanguages);
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.WorkingDirectory = appdir;
			proc.Start();
		}
	}
}

