using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Messaging;
using OpenIDE.Core.FileSystem;
using OpenIDE.Bootstrapping;
using OpenIDE.Arguments;
using OpenIDE.Core.Language;
using OpenIDE.CommandBuilding;
using OpenIDE.Core.CommandBuilding;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.Commands;
using OpenIDE.Core.Definitions;
namespace oi
{
	class MainClass
	{
		private const string PROFILE = "--profile=";
		private const string GLOBAL_PROFILE = "--global-profile=";

		public static void Main(string[] args)
		{
			args = parseProfile(args);
			Bootstrapper.Initialize();			
			if (args.Length > 0 && args[0] == "test") {
				tryDefinitionBuilder(args);
				return;
			}
			args = Bootstrapper.Settings.Parse(args);
			if (args.Length == 0)
			{
				printUsage(null);
				return;
			}
			var execute = Bootstrapper.GetDispatcher();
			var parser = new CommandStringParser();
			execute.For(
				parser.GetCommand(args),
				parser.GetArguments(args),
				(command) => printUsage(command));
		}

		private static void tryDefinitionBuilder(string[] args) {
			var token = Bootstrapper.Settings.RootPath;
			var builder = Bootstrapper.GetDefinitionBuilder(); 
			builder.Build();
			var arguments = new List<string>();
			arguments.AddRange(args);
			arguments.RemoveAt(0);
			var cmd = builder.Get(arguments.ToArray());
			if (cmd == null) {
				Console.WriteLine("Invlid command");
				return;
			}
			arguments.RemoveAt(0);
			if (cmd.Type == DefinitionCacheItemType.Script) {
				var script = new Script(token, Environment.CurrentDirectory, cmd.Location);
				var sb = new StringBuilder();
				for (int i = 0; i < arguments.Count; i++)
					sb.Append(" \"" + arguments[i] + "\"");
				script.Run(sb.ToString(), Bootstrapper.DispatchMessage);
			} else if (cmd.Type == DefinitionCacheItemType.Language) {
				var language = new LanguagePlugin(cmd.Location, Bootstrapper.DispatchMessage);
				language.Run(arguments.ToArray());
			} else {
				var command = Bootstrapper.GetDefaultHandlersWithoutRunHandler()
					.FirstOrDefault(x => x.Command == cmd.Name);
				if (command == null) {
					Console.WriteLine("Invalid command");
					return;
				}
				command.Execute(arguments.ToArray());
			}
		}

		private static string[] parseProfile(string[] args)
		{
			var newArgs = new List<string>();
			foreach (var arg in args) {
				if (arg.StartsWith(PROFILE)) {
					ProfileLocator.ActiveLocalProfile =
						arg.Substring(PROFILE.Length, arg.Length - PROFILE.Length);
				} else if (arg.StartsWith(GLOBAL_PROFILE)) {
					ProfileLocator.ActiveGlobalProfile =
						arg.Substring(GLOBAL_PROFILE.Length, arg.Length - GLOBAL_PROFILE.Length);
				} else {
					newArgs.Add(arg);
				}
			}
			return newArgs.ToArray();
		}

		private static void printUsage(string commandName)
		{
			if (commandName == null) {
				Console.WriteLine("OpenIDE v0.2");
				Console.WriteLine("OpenIDE is a scriptable environment that provides simple IDE features around your favorite text exitor.");
				Console.WriteLine("(http://www.openide.net, http://github.com/ContinuousTests/OpenIDE)");
				Console.WriteLine();
			}
			var level = 1;
			var handlers = Bootstrapper.GetCommandHandlers();
			var isHint = false;
			if (commandName != null) {
				handlers = handlers
					.Where(x => 
						 x.Command.Contains(commandName) ||
						(
							x.Usage.Parameters.Any(y => y.Required && matchName(y.Name, commandName))
						));
				if (handlers.Count() > 0)
					Console.WriteLine("Did you mean:");
				isHint = true;
			}
			if (handlers.Count() > 0 && commandName == null) {
				Console.WriteLine();
				Console.WriteLine("\t[{0}=NAME] : Force command to run under specified profile", PROFILE);
				Console.WriteLine("\t[{0}=NAME] : Force command to run under specified global profile", GLOBAL_PROFILE);
				Console.WriteLine("\t[--default-language=NAME] : Force command to run using specified default language");
				Console.WriteLine("\t[--enabled-languages=LANGUAGE_LIST] : Force command to run using specified languages");
				Console.WriteLine();
			}
			handlers.ToList()
				.ForEach(x =>
					{
						var command = x.Usage;
						if (command.Name != Bootstrapper.Settings.DefaultLanguage)
							UsagePrinter.PrintCommand(command);
						else
						{
							Console.WriteLine("");
							Console.WriteLine("\tDirect commands because of default language {0}", command.Name);
							level--;
						}

						command.Parameters.ToList()
							.ForEach(y =>  {
								if (commandName == null || y.Required && matchName(y.Name, commandName))
									UsagePrinter.PrintParameter(y, ref level);
							});

						if ((command.Name == Bootstrapper.Settings.DefaultLanguage))
							level++;
					});
			Console.WriteLine();
		}

		private static bool matchName(string actual, string parameter)
		{
			if (actual.Contains(parameter))
				return true;
			if (Math.Abs(actual.Length - parameter.Length) > 2)
				return false;
			var containedCharacters = 0;
			for (int i = 0; i < actual.Length; i++) {
				if (parameter.Contains(actual[i]))
					containedCharacters++;
			}
			if (containedCharacters > actual.Length - 2)
				return true;
			return false;
		}
	}
}

