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
	class DereferenceHandler : ICommandHandler
	{
		private string _keyPath;
		private Func<string, ProviderSettings> _getTypesProviderByLocation;
		private IProjectHandler _project = new ProjectHandler();
		
		public string Usage {
			get {
				return 
					Command + "|\"Dereferences a project/assembly from given project\"" +
						"REFERENCE|\"Path to the reference to remove\"" +
							"[PROJECT]|\"Project to remove the reference from\" end " +
						"end " +
					"end ";
			}
		}

		public string Command { get { return "dereference"; } }

		public DereferenceHandler(Func<string, ProviderSettings> provider, string keyPath)
		{
			_getTypesProviderByLocation = provider;
			_keyPath = keyPath;
		}

		public void Execute(IResponseWriter writer, string[] arguments)
		{
			if (arguments.Length < 1)
			{
				writer.Write("error|The handler needs the full path to the reference. " +
								  "Usage: dereference REFERENCE [PROJECT");
				return;
			}
			
			if (arguments.Length > 1) {
				var projectFile = getFile(arguments[1]);
				if (!File.Exists(projectFile))
				{
					writer.Write("error|The project to remove this reference for does not exist. " +
									  "Usage: dereference REFERENCE [PROJECT");
					return;
				}
				
				if (!_project.Read(projectFile, _getTypesProviderByLocation))
					return;
			} else {
				if (!_project.Read(Environment.CurrentDirectory, _getTypesProviderByLocation)) {
					writer.Write("error|Could not locate project within " + Environment.CurrentDirectory + ". " +
								 "Usage: dereference REFERENCE [PROJECT]");
					return;
				}
			}

			var fullpath = getFile(arguments[0]);
			IFile file;

			if (new VSProjectFile().SupportsExtension(fullpath))
				file = new VSProjectFile().New(fullpath);
			else
				file = new AssemblyFile().New(fullpath);

			_project.Dereference(file);
			_project.Write();

			writer.Write("Rereferenced {0} from {1}", fullpath, _project.Fullpath);
		}

		private string getFile(string argument)
		{
			var filename = Path.GetFileName(argument);
			var dir = Path.GetDirectoryName(argument).Trim();
			if (dir.Length == 0)
				return _keyPath;
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
