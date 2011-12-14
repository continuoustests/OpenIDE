using System;
using System.Linq;
using OpenIDENet.Projects;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects.Writers;
using OpenIDENet.Messaging;
using OpenIDENet.FileSystem;
using OpenIDENet.Bootstrapping;
using OpenIDENet.Versioning;
using OpenIDENet.Files;
using OpenIDENet.Arguments;

namespace oi
{
	class MainClass
	{
		public static void Main(string[] args)
		{	
			Bootstrapper.Initialize();
			if (args.Length == 0)
			{
				printUsage();
				return;
			}
			var execute = Bootstrapper.GetDispatcher();
			execute.For(args[0], getCommandArguments(args));
		}
		
		private static string[] getCommandArguments(string[] args)
		{
			if (args.Length == 1)
				return new string[] {};
			string[] newArgs = new string[args.Length - 1];
			for (int i = 1; i < args.Length; i++)
				newArgs[i - 1] = args[i];
			return newArgs;
		}

		private static void printUsage()
		{
			Console.WriteLine();
			Console.WriteLine("OpenIDENet is a language agnostic system that provides simple IDE features around your favorite text exitor");
			Console.WriteLine("http://www.continuoustests.com, http://github.com/ContinuousTests/OpenIDENet");
			Console.WriteLine();
			Console.WriteLine();
			var level = 1;
			var handlers = Bootstrapper.GetCommandHandlers();
			handlers.ToList()
				.ForEach(x =>
					{
						var command = x.Usage;
						if (command == null)
							return;
						printCommand(command);
						command.Parameters.ToList()
							.ForEach(y =>  printParameter(y, ref level));
					});
		}
		
		private static void printCommand(CommandHandlerParameter command)
		{
			Console.WriteLine("");
			Console.WriteLine("\t{0} ({1})", command.Description, command.Language);
			Console.WriteLine("\t" + command.Name);
		}

		private static void printParameter(BaseCommandHandlerParameter parameter, ref int level)
		{
			level++;
			var name = parameter.Name;
			if (!parameter.Required)
				name = "[" + name + "]";
			Console.WriteLine("{0}{1} : {2}", "".PadLeft(level, '\t'), name, parameter.Description);

			foreach (var child in parameter.Parameters)
				printParameter(child, ref level);
			level--;
		}
	}
}

