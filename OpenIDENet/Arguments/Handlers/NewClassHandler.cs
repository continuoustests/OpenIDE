using System;
using System.IO;
using System.Text;
using OpenIDENet.Versioning;
using OpenIDENet.Projects;
using OpenIDENet.Files;
using OpenIDENet.FileSystem;

namespace OpenIDENet.Arguments.Handlers
{
	class NewClassHandler : ICommandHandler
	{
		private IFS _fs;
		
		public string Command { get { return "newclass"; } }
		
		public NewClassHandler(IFS fs)
		{
			_fs = fs;
		}
		
		public void Execute(string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation)
		{
			if (arguments.Length != 1)
			{
				Console.WriteLine("Invalid number of arguments");
				return;
			}
			
			var className = getFileName(arguments[0]);
			var location = getLocation(arguments[0]);
			var provider = getTypesProviderByLocation(location);
			if (provider == null)
				return;
			
			var with = (IProvideVersionedTypes) provider.TypesProvider;
			var project = with.Reader().Read(provider.ProjectFile);
			
			var file = getFileName(className, location, project);
			if (_fs.FileExists(file))
			{
				Console.WriteLine("File already exists {0}", file);
				return;
			}
			var writtenFile = writeToFile(className, location, project);
			with.FileAppenderFor(writtenFile).Append(project, writtenFile);
			with.Writer().Write(project);
		}
		
		private string getFileName(string classname)
		{
			return Path.GetFileNameWithoutExtension(classname);
		}
		
		private string getLocation(string className)
		{
			var dir = Path.GetDirectoryName(className).Trim();
			if (dir.Length == 0)
				return Environment.CurrentDirectory;
			return dir;
		}
		
		private string getFileName(string className, string location, IProject project)
		{
			var fileName = Path.Combine(location, className);
			return fileName + CompileFile.DefaultExtensionFor(project.Settings.Type);
		}
		
		private IFile writeToFile(string className, string location, IProject project)
		{
			var fileName = getFileName(className, location, project);
			var ns = getNamespace(location, project);
			_fs.WriteAllText(fileName, getContent(className, project, ns));
			Console.WriteLine("Created class {0}.{1}", ns, className);
			Console.WriteLine("Full path {0}", fileName);
			Console.WriteLine("");
			return new CompileFile(fileName);
		}
		
		private string getNamespace(string location, IProject project)
		{
			var projectLocation = Path.GetDirectoryName(project.Fullpath);
			var relativePath = PathExtensions.GetRelativePath(projectLocation, location);
			if (relativePath.Length == 0 || relativePath.Equals(location))
				return project.Settings.DefaultNamespace;
			return string.Format("{0}.{1}", project.Settings.DefaultNamespace, relativePath.Replace(Path.DirectorySeparatorChar.ToString(), "."));
		}
		
		private string getContent(string className, IProject project, string ns)
		{
			var sb = new StringBuilder();
			sb.AppendLine("using System;");
			sb.AppendLine("");
			sb.AppendLine("namespace " + ns);
			sb.AppendLine("{");
			sb.AppendLine("\tclass " + className);
			sb.AppendLine("\t{");
			sb.AppendLine("\t}");
			sb.AppendLine("}");
			return sb.ToString();
		}
	}
}