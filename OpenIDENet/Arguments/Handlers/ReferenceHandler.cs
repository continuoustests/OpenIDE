using System;
using System.IO;
using OpenIDENet.Arguments;
using OpenIDENet.FileSystem;
using OpenIDENet.Versioning;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Files;
using OpenIDENet.Projects;
using OpenIDENet.Languages;

namespace OpenIDENet.Arguments.Handlers
{
	class ReferenceHandler : ICommandHandler
	{
		private IProjectHandler _project = new ProjectHandler();
		
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					SupportedLanguage.CSharp,
					CommandType.ProjectCommand,
					Command,
					"Adds a reference to a project file");
				usage
					.Add("REFERENCE", "The path to the project or assembly to be referenced")
						.Add("PROJECT", "The path to the project to add the reference to");
				return usage;
			}
		}

		public string Command { get { return "reference"; } }

		public void Execute(string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation)
		{
			if (arguments.Length != 2)
			{
				Console.WriteLine("The handler needs the full path to the reference. " +
								  "Usage: reference {assembly/project} {project to add reference to");
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
				Console.WriteLine("The project to add this reference to does not exist. " +
								  "Usage: reference {assembly/project} {project to add reference to");
				return;
			}
			
			if (!_project.Read(projectFile, getTypesProviderByLocation))
				return;
			_project.Reference(file);
			_project.Write();

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
