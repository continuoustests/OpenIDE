using System;
using System.IO;
using CSharp.FileSystem;
using CSharp.Responses;
using CSharp.Versioning;
using CSharp.Projects.Readers;
using CSharp.Files;
using CSharp.Projects;

namespace CSharp.Commands
{
	public class ReferenceHandler : ICommandHandler
	{
		private string _keyPath;
		private Func<string, ProviderSettings> _provider;
		private IProjectHandler _project = new ProjectHandler();
		
		public string Usage {
			get {
				return
					Command + "|\"Adds a reference to a project file\"" +
						"REFERENCE|\"The path to the project or assembly to be referenced\"" +
							"[PROJECT]|\"The path to the project to add the reference to\" end " +
						"end " +
					"end ";
			}
		}

		public string Command { get { return "reference"; } }
		
		public ReferenceHandler(Func<string, ProviderSettings> provider, string keyPath)
		{
			_provider = provider;
			_keyPath = keyPath;
		}

		public void Execute(IResponseWriter writer, string[] arguments)
		{
			if (arguments.Length < 1)
			{
				writer.Write("error|The handler needs the full path to the reference. " +
								  "Usage: reference REFERENCE [PROJECT]");
				return;
			}
			
			if (arguments.Length > 1) {
				var projectFile = getFile(arguments[1]);
				if (!File.Exists(projectFile)) {
					writer.Write("error|The project to add this reference to does not exist. " +
									  "Usage: reference REFERENCE [PROJECT]");
					return;
				}
				if (!_project.Read(projectFile, _provider))
					return;
			} else {
				if (!_project.Read(Environment.CurrentDirectory, _provider)) {
					writer.Write("error|Could not locate project within " + Environment.CurrentDirectory + ". " +
								 "Usage: reference REFERENCE [PROJECT]");
					return;
				}
			}

			var fullpath = new PathParser(arguments[0]).ToAbsolute(Environment.CurrentDirectory);
			IFile file;

			if (new VSProjectFile().SupportsExtension(fullpath))
				file = new VSProjectFile().New(fullpath);
			else
				file = new AssemblyFile().New(fullpath);
			
			_project.Reference(file);
			_project.Write();

			writer.Write("Added reference {0} to {1}", fullpath, _project.Fullpath);
		}

		private string getFile(string argument)
		{
			var filename = Path.GetFileName(argument);
			var dir = Path.GetDirectoryName(argument).Trim();
			if (dir.Length == 0)
				return Path.Combine(_keyPath, filename);
			if (Directory.Exists(Path.Combine(_keyPath, dir)))
				return Path.Combine(
					Path.Combine(_keyPath, dir),
					filename);
			if (Directory.Exists(dir))
				return Path.Combine(dir, filename);
			return Path.Combine(_keyPath, filename);
		}
	}
}
