using System;
using System.IO;
using CSharp.FileSystem;
using CSharp.Versioning;
using CSharp.Projects.Readers;
using CSharp.Files;
using CSharp.Projects;

namespace CSharp.Commands
{
	class DereferenceHandler : ICommandHandler
	{
		private Func<string, ProviderSettings> _getTypesProviderByLocation;
		private IProjectHandler _project = new ProjectHandler();
		
		public string Usage {
			get {
				return 
					Command + "|\"Dereferences a project/assembly from given project\"" +
						"REFERENCE|\"Path to the reference to remove\"" +
							"PROJECT|\"Project to remove the reference from\" end " +
						"end " +
					"end ";
			}
		}

		public string Command { get { return "dereference"; } }

		public DereferenceHandler(Func<string, ProviderSettings> provider)
		{
			_getTypesProviderByLocation = provider;
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length != 2)
			{
				Console.WriteLine("error|The handler needs the full path to the reference. " +
								  "Usage: dereference {assembly/project} {project to remove reference from}");
				return;
			}
			
			var fullpath = getFile(arguments[0]);
			IFile file;
			if (new VSProjectFile().SupportsExtension(fullpath))
				file = new VSProjectFile().New(fullpath);
			else
				file = new AssemblyFile().New(fullpath);
			var projectFile = arguments[1];
			if (!File.Exists(projectFile))
			{
				Console.WriteLine("error|The project to remove this reference for does not exist. " +
								  "Usage: dereference {assembly/project} {project to remove reference from}");
				return;
			}
			
			if (!_project.Read(projectFile, _getTypesProviderByLocation))
				return;
			_project.Dereference(file);
			_project.Write();

			Console.WriteLine("comment|Rereferenced {0} from {1}", file, projectFile);
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
