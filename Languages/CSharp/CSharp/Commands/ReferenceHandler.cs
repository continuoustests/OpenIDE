using System;
using System.IO;
using CSharp.FileSystem;
using CSharp.Versioning;
using CSharp.Projects.Readers;
using CSharp.Files;
using CSharp.Projects;

namespace CSharp.Commands
{
	public class ReferenceHandler : ICommandHandler
	{
		private Func<string, ProviderSettings> _provider;
		private IProjectHandler _project = new ProjectHandler();
		
		public string Usage {
			get {
				return
					Command + "|\"Adds a reference to a project file\"" +
						"REFERENCE|\"The path to the project or assembly to be referenced\"" +
							"PROJECT|\"The path to the project to add the reference to\" end " +
						"end " +
					"end ";
			}
		}

		public string Command { get { return "reference"; } }
		
		public ReferenceHandler(Func<string, ProviderSettings> provider)
		{
			_provider = provider;
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length != 2)
			{
				Console.WriteLine("error|The handler needs the full path to the reference. " +
								  "Usage: reference {assembly/project} {project to add reference to");
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
				Console.WriteLine("error|The project to add this reference to does not exist. " +
								  "Usage: reference {assembly/project} {project to add reference to");
				return;
			}
			
			if (!_project.Read(projectFile, _provider))
				return;
			_project.Reference(file);
			_project.Write();

			Console.WriteLine("comment|Added reference {0} to {1}", file, projectFile);
		}

		private string getFile(string argument)
		{
			var filename = Path.GetFileName(argument);
			var dir = Path.GetDirectoryName(argument).Trim();
			if (dir.Length == 0)
				return Path.Combine(Environment.CurrentDirectory, filename);
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
