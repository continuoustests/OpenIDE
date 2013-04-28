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

			var builder = Bootstrapper.GetDefinitionBuilder(); 
			builder.Build();
			args = Bootstrapper.Settings.Parse(args);

			if (args.Length == 0) {
				printUsage(null);
				return;
			}

			var arguments = new List<string>();
			arguments.AddRange(args);
			var cmd = builder.Get(arguments.ToArray());
			if (cmd == null) {
				printUsage(arguments[0]);
				return;
			}
			arguments.RemoveAt(0);
			if (cmd.Type == DefinitionCacheItemType.Script) {
				var script = new Script(Bootstrapper.Settings.RootPath, Environment.CurrentDirectory, cmd.Location);
				var sb = new StringBuilder();
				for (int i = 0; i < arguments.Count; i++)
					sb.Append(" \"" + arguments[i] + "\"");
				script.Run(sb.ToString(), Bootstrapper.DispatchMessage);
			} else if (cmd.Type == DefinitionCacheItemType.Language) {
				var language = new LanguagePlugin(cmd.Location, Bootstrapper.DispatchMessage);
				language.Run(arguments.ToArray());
			} else {
				var command = Bootstrapper.GetDefaultHandlersWithoutRunHandler()
					.FirstOrDefault(x => x.Command == args[0]);
				if (command == null) {
					printUsage(args[0]);
					return;
				}
				command.Execute(arguments.ToArray());
			}
			return;

						var execute = Bootstrapper.GetDispatcher();
			var parser = new CommandStringParser();
			execute.For(
				parser.GetCommand(args),
				parser.GetArguments(args),
				(command) => printUsage(command));
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
			var definitions = Bootstrapper.GetDefinitionBuilder().Definitions;
			if (commandName == null) {
				Console.WriteLine("OpenIDE v0.2");
				Console.WriteLine("OpenIDE is a scriptable environment that provides simple IDE features around your favorite text exitor.");
				Console.WriteLine("(http://www.openide.net, http://github.com/ContinuousTests/OpenIDE)");
				Console.WriteLine();
			}
			var isHint = false;
			if (commandName != null) {
				definitions = definitions 
					.Where(x => 
						 x.Name.Contains(commandName) ||
						(
							x.Parameters.Any(y => y.Required && matchName(y.Name, commandName))
						));
				if (definitions.Count() > 0)
					Console.WriteLine("Did you mean:");
				isHint = true;
			}
			if (definitions.Count() > 0 && commandName == null) {
				Console.WriteLine();
				Console.WriteLine("\t[{0}=NAME] : Force command to run under specified profile", PROFILE);
				Console.WriteLine("\t[{0}=NAME] : Force command to run under specified global profile", GLOBAL_PROFILE);
				Console.WriteLine("\t[--default-language=NAME] : Force command to run using specified default language");
				Console.WriteLine("\t[--enabled-languages=LANGUAGE_LIST] : Force command to run using specified languages");
				Console.WriteLine();
			}
			definitions.ToList()
				.ForEach(x => UsagePrinter.PrintDefinition(x));
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

