using System;
using System.IO;
using System.Reflection;
using System.Linq;
using OpenIDENet.Versioning;
using OpenIDENet.Files;
using OpenIDENet.Projects;
using OpenIDENet.FileSystem;
using System.Xml;
using System.Text;
using System.Diagnostics;
using OpenIDENet.EditorEngineIntegration;
namespace OpenIDENet.Arguments.Handlers
{
	class NewHandler : ICommandHandler
	{
		private ILocateEditorEngine _editorFactory;
		private OpenIDENet.Files.IResolveFileTypes _fileTypeResolver;
		
		public string Command { get { return "new"; } }
		
		public NewHandler(OpenIDENet.Files.IResolveFileTypes fileTypeResolver, ILocateEditorEngine editorFactory)
		{
			_fileTypeResolver = fileTypeResolver;
			_editorFactory = editorFactory;
		}
		
		public void Execute(string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation)
		{
			if (arguments.Length < 2)
			{
				Console.WriteLine("Invalid number of arguments. Usage: new {template name} {item name} {template arguments}");
				return;
			}
			
			var className = getFileName(arguments[1]);
			var location = getLocation(arguments[1]);
			var provider = getTypesProviderByLocation(location);
			if (provider == null)
				return;
			
			var with = (IProvideVersionedTypes) provider.TypesProvider;
			var project = with.Reader().Read(provider.ProjectFile);
			
			var template = pickTemplate(arguments[0], project.Settings.Type);
			if (template == null)
			{
				Console.WriteLine("No template with the name {0} exists.", arguments[0]);
				return;
			}
			var ns = getNamespace(location, project);
			template.Run(location, className, ns, project, getArguments(arguments));
			if (template.File == null)
				return;
			with.FileAppenderFor(template.File).Append(project, template.File);
			with.Writer().Write(project);
			
			Console.WriteLine("Created class {0}.{1}", ns, className);
			Console.WriteLine("Full path {0}", template.File.Fullpath);
			Console.WriteLine("");
			
			gotoFile(template.File.Fullpath, template.Line, template.Column, location);
		}
		
		private Template pickTemplate(string templateName, ProjectType type)
		{
			var templateDir = Path.Combine(Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "templates"), type.ToString().ToLower()), "new");
			var template = Directory.GetFiles(templateDir, string.Format("{0}.*", templateName)).FirstOrDefault();
			if (template == null)
				return null;
			return new Template(template, _fileTypeResolver);
		}
		
		private string[] getArguments(string[] args)
		{
			if (args.Length == 1)
				return new string[] {};
			string[] newArgs = new string[args.Length - 1];
			for (int i = 1; i < args.Length; i++)
				newArgs[i - 1] = args[i];
			return newArgs;
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
		
		private string getNamespace(string location, IProject project)
		{
			var projectLocation = Path.GetDirectoryName(project.Fullpath);
			var relativePath = PathExtensions.GetRelativePath(projectLocation, location);
			if (relativePath.Length == 0 || relativePath.Equals(location))
				return project.Settings.DefaultNamespace;
			return string.Format("{0}.{1}", project.Settings.DefaultNamespace, relativePath.Replace(Path.DirectorySeparatorChar.ToString(), "."));
		}
		
		private void gotoFile(string file, int line, int column, string location)
		{
			var instance = _editorFactory.GetInstance(location);
			if (instance == null)
				return;
			instance.GoTo(file, line, column);
			instance.SetFocus();
		}
	}
	
	class Template
	{
		private OpenIDENet.Files.IResolveFileTypes _fileTypeResolver;
		private string _file;
		
		public IFile File { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
		
		public Template(string file, OpenIDENet.Files.IResolveFileTypes fileTypeResolver)
		{
			_fileTypeResolver = fileTypeResolver;
			_file = file;
			Line = 0;
			Column = 0;
		}
		
		public void Run(string location, string itemName, string nameSpace, IProject project, string[] arguments)
		{
			try
			{
				var filename = Path.Combine(location, string.Format("{0}{1}", itemName, run("get_file_extension")));
				if (System.IO.File.Exists(filename))
				{
					Console.WriteLine("File already exists {0}", filename);
					File = null;
					return;
				}
				File = _fileTypeResolver.Resolve(filename);
				if (File == null)
					return;
				
				var xml = getXml(location, project, arguments);
				var tempFile = Path.GetTempFileName();
				System.IO.File.WriteAllText(tempFile, xml);
				var content = run(string.Format("\"{0}\" \"{1}\" \"{2}\"", itemName, nameSpace, tempFile));
				System.IO.File.Delete(tempFile);
				System.IO.File.WriteAllText(filename, content);
				getPositionInfo();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				File = null;
			}
		}
		
		private string getXml(string filename, IProject project, string[] arguments)
		{
			var sb = new StringBuilder();
			using (var writer = XmlWriter.Create(sb))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("parameters");
					writer.WriteElementString("fullpath", filename);
					writer.WriteStartElement("project");
						writer.WriteElementString("fullpath", project.Fullpath);
						writer.WriteElementString("type", project.Settings.Type.ToString());
					writer.WriteEndElement();
					writer.WriteStartElement("custom_parameters");
						arguments.ToList().ForEach(x => writer.WriteElementString("parameter", x));
					writer.WriteEndElement();
				writer.WriteEndElement();
			}
			return sb.ToString();
		}
		
		private void getPositionInfo()
		{
			try
			{
				var positionString = run("get_position").Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
				Line = int.Parse(positionString[0]);
				Column = int.Parse(positionString[1]);
			}
			catch
			{
				Line = 0;
				Column = 0;
			}
		}
		
		private string run(string arguments)
		{
			var proc = new Process();
			proc.StartInfo = new ProcessStartInfo(_file, arguments);
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.Start();
			var output = proc.StandardOutput.ReadToEnd();
			proc.WaitForExit();
			if (output.Length > Environment.NewLine.Length)
				return output.Substring(0, output.Length - Environment.NewLine.Length);
			return output;
		}
	}
}

