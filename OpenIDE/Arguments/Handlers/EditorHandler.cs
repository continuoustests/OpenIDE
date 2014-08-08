using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Scripts;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.Config;
using OpenIDE.Core.Environments;
using CoreExtensions;
namespace OpenIDE.Arguments.Handlers
{
	class EditorHandler : ICommandHandler
	{
		private string _rootPath;
		private OpenIDE.Core.EditorEngineIntegration.ILocateEditorEngine _editorFactory;
		private EnvironmentService _environment;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Initializes the environment by staring the editor, code model and editor engine");
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
				usage.Add("get-dirty-files", "Queries the editor for all modified files and their content")
					.Add("[FILE]", "If passed it will only respond with the file specified");
				usage.Add("command", "Custom editor commands");
				usage.Add("get-caret", "Gets the caret from editor");
				usage.Add("user-select", "Presents the user with a list of options to select from")
					.Add("ID", "Identifier string to look for in the responding user-selected event")
						.Add("LIST", "A comma separated list of options for the user");
				usage.Add("user-input", "Presents the user with an input dialog")
					.Add("ID", "Identifier string to look for in the responding user-inputted event")
						.Add("[DEFAULT]", "The default value for the user");
				return usage;
			}
		}

		public string Command { get { return "editor"; } }
		
		public EditorHandler(string rootPath, OpenIDE.Core.EditorEngineIntegration.ILocateEditorEngine editorFactory, EnvironmentService environment)
		{
			_rootPath = rootPath;
			_editorFactory = editorFactory;
			_environment = environment;
		}
		
		public void Execute(string[] arguments)
		{
			Logger.Write ("Getting editor instance");
			var instance = _editorFactory.GetInstance(_rootPath);
			// TODO remove that unbeleavable nasty setfocus solution. Only init if launching editor
			var isSetfocus = arguments.Length > 0 && arguments[0] == "setfocus";
			if (instance == null && arguments.Length >= 0 && !isSetfocus)
			{
				var args = new List<string>();
				Logger.Write("Reading configuration from " + _rootPath);
				var configReader = new ConfigReader(_rootPath);
				if (arguments.Length == 0) {
					var name = configReader.Get("default.editor");
					if (name == null) {
						Console.WriteLine("To launch without specifying editor you must specify the default.editor config option");
						return;
					}
					args.Add(name);
				} else {
					args.AddRange(arguments);
				}
				var editorName = args[0];
				args.AddRange( 
					configReader	
						.GetStartingWith("editor." + editorName)
						.Select(x => "--" + x.Key + "=" + x.Value));

				if (!_environment.HasEditorEngine(_rootPath)) {
					if (!_environment.StartEditorEngine(args, _rootPath)) {
						Logger.Write("Could not launch editor " + args[0]);
						return;
					}
				}
				if (!_environment.HasEditorEngine(_rootPath)) {
					Logger.Write("Could not launch editor " + args[0]);
					return;
				}
				if (!_environment.IsRunning(_rootPath))
					_environment.Start(_rootPath);
			}
			else if (arguments.Length >= 1 && arguments[0] == "get-dirty-files")
			{
				if (instance == null)
					return;
				string file = null;
				if (arguments.Length > 1)
					file = arguments[1];
				Console.WriteLine(instance.GetDirtyFiles(file));
			}
			else if (arguments.Length == 1 && arguments[0] == "get-caret")
			{
				Console.WriteLine(instance.GetCaret());
			}
			else if (arguments.Length == 3 && arguments[0] == "user-select")
			{
				instance.UserSelect(arguments[1], arguments[2]);
			}
			else if (arguments.Length >= 2 && arguments[0] == "user-input")
			{
				var defaultvalue = "";
				if (arguments.Length > 2)
					defaultvalue = arguments[2];
				instance.UserInput(arguments[1], defaultvalue);
			}
			else
			{
				if (instance == null)
					return;
				var editor = _editorFactory.GetInstance(_rootPath);
				if (editor != null)
					Console.WriteLine("There is already an editor session running at this location. Run 'oi environment details' for more information.");
				instance.Run(arguments);
			}
		}
	}
}

