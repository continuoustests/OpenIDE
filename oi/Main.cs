using System;
using OpenIDENet.Projects;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects.Writers;
using OpenIDENet.Messaging;
using OpenIDENet.FileSystem;
using OpenIDENet.Bootstrapping;
using OpenIDENet.Versioning;
using OpenIDENet.Files;

namespace oi
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
				return;
			
			Bootstrapper.Initialize();
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
	}
}

