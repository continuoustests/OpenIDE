using System;
using System.IO;
using OpenIDENet.Arguments;
using OpenIDENet.FileSystem;
using OpenIDENet.Versioning;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Files;

namespace OpenIDENet.Arguments.Handlers
{
	class ReferenceHandler : ICommandHandler
	{
		public string Command { get { return "reference"; } }

		public void Execute(string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation)
		{
			if (arguments.Length != 2)
			{
				Console.WriteLine("The handler needs the full path to the reference. Usage: reference {assembly/project} {project to add reference to");
				return;
			}

			var file = new AssemblyFile(getFile(arguments[0]));
			var projectFile = arguments[1];
			if (!File.Exists(projectFile))
			{
				Console.WriteLine("The project do add this reference to does not exist. Usage: reference {assembly/project} {project to add reference to");
				return;
			}

			var provider = getTypesProviderByLocation(projectFile);
			if (provider == null)
				return;
			
			var with = (IProvideVersionedTypes) provider.TypesProvider;
			if (with == null)
				return;
			var project = with.Reader().Read(provider.ProjectFile);
			if (project == null)
				return;
			with.ReferencerFor(file).Reference(project, file);
			with.Writer().Write(project);

			Console.WriteLine("Added reference {0} to {1}", file, projectFile);
		}

		private string getFile(string argument)
		{
			var filename = Path.GetFileName(argument);
			var dir = Path.GetDirectoryName(argument).Trim();
			if (dir.Length == 0)
				return Environment.CurrentDirectory;
			if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, dir)))
				return Path.Combine(
					Path.Combine(Environment.CurrentDirectory, dir),
					filename);
			if (Directory.Exists(dir))
				return Path.Combine(dir, filename);
			return Path.Combine(Environment.CurrentDirectory, filename);
		}
	}
}
