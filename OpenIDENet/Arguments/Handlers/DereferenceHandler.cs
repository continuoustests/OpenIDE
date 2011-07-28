using System;
using System.IO;
using OpenIDENet.Arguments;
using OpenIDENet.FileSystem;
using OpenIDENet.Versioning;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Files;
using OpenIDENet.Projects;

namespace OpenIDENet.Arguments.Handlers
{
	class DereferenceHandler : ICommandHandler
	{
		private IProjectHandler _project = new ProjectHandler();

		public string Command { get { return "dereference"; } }

		public void Execute(string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation)
		{
			if (arguments.Length != 2)
			{
				Console.WriteLine("The handler needs the full path to the reference. " +
								  "Usage: dereference {assembly/project} {project to remove reference from}");
				return;
			}
			
			var fullpath = getFile(arguments[0]);
			IFile file;
			if (ProjectFile.SupportsExtension(fullpath))
				file = new ProjectFile(fullpath);
			else
				file = new AssemblyFile(fullpath);
			var projectFile = arguments[1];
			if (!File.Exists(projectFile))
			{
				Console.WriteLine("The project to remove this reference for does not exist. " +
								  "Usage: dereference {assembly/project} {project to remove reference from}");
				return;
			}
			
			if (!_project.Read(projectFile, getTypesProviderByLocation))
				return;
			_project.Dereference(file);
			_project.Write();

			Console.WriteLine("Rereferenced {0} from {1}", file, projectFile);
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
