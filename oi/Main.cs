using System;
using OpenIDENet.Projects;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects.Writers;
using OpenIDENet.Messaging;

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
			var reader = new DefaultReader();
			var project = reader.Read("test.csproj");
			var appender = new VS2010FileAppender(new MessageBus());
			appender.Append(project, args[1]);
			var writer = new DefaultWriter();
			writer.Write(project);
		}
	}
}

