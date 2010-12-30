using System;
using OpenIDENet.Projects;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects.Writers;
using OpenIDENet.Messaging;
using OpenIDENet.FileSystem;
using OpenIDENet.Bootstrapping;
using OpenIDENet.Versioning;

namespace oi
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length != 2)
				return;
			if (!args[0].Equals("add"))
				return;
			
			Console.WriteLine("About to add {0}", args[1]);
			Bootstrapper.Initialize();
			var with = Bootstrapper.Resolve<IResolveProjectVersion>().ResolveFor("test.csproj");
			if (with == null)
				return;
			var project = with.Reader().Read("test.csproj");
			with.CompiledFileAppender().Append(project, args[1]);
			with.Writer().Write(project);
		}
	}
}

