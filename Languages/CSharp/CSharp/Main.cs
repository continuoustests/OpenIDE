using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using CSharp.Commands;
using CSharp.Files;
using CSharp.FileSystem;
using CSharp.Projects;
using CSharp.Versioning;
using CSharp.Projects.Readers;
using CSharp.Projects.Writers;
using CSharp.Projects.Appenders;
using CSharp.Projects.Removers;
using CSharp.Projects.Referencers;

namespace CSharp
{
	public class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
				return;
			var dispatcher = new Dispatcher();
			configureHandlers(dispatcher);
			var handler = dispatcher.GetHandler(args[0]);
			if (handler == null)
				return;

			try {
				handler.Execute(getParameters(args));
			} catch (Exception ex) {
				var builder = new OutputWriter();
				builder.Error(ex.Message.Replace(Environment.NewLine, ""));
				if (ex.StackTrace != null) {
					ex.StackTrace
						.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList()
						.ForEach(line => builder.Error(line));
				}
			}
		}

		static string[] getParameters(string[] args)
		{
			var remaining = new List<string>();
			for (int i = 1; i < args.Length; i++)
				remaining.Add(args[i]);
			return remaining.ToArray();
		}
		
		static void configureHandlers(Dispatcher dispatcher)
		{
			dispatcher.Register(new GetUsageHandler(dispatcher));
			dispatcher.Register(new CrawlHandler());
			dispatcher.Register(new CrawlFileTypesHandler());
			dispatcher.Register(new SignatureFromPositionHandler());
			dispatcher.Register(new MembersFromUnknownSignatureHandler());
			dispatcher.Register(new CreateHandler(getReferenceTypeResolver()));
			dispatcher.Register(new AddFileHandler(getTypesProvider));
			dispatcher.Register(new DeleteFileHandler(getTypesProvider));
			dispatcher.Register(new DereferenceHandler(getTypesProvider));
			dispatcher.Register(new NewHandler(getFileTypeResolver(), getTypesProvider));
			dispatcher.Register(new ReferenceHandler(getTypesProvider));
			dispatcher.Register(new RemoveFileHandler(getTypesProvider));
		}
		
		static VSFileTypeResolver getFileTypeResolver()
		{
			return new VSFileTypeResolver(
				new IFile[]
					{
						new CompileFile(),
						// Keep this one last as it accept all types
						new NoneFile()
					});
		}
		
		static VSFileTypeResolver getReferenceTypeResolver()
		{
			return new VSFileTypeResolver(
				new IFile[]
					{
						new AssemblyFile(),
						new VSProjectFile()
					});
		}
		
		static ProviderSettings getTypesProvider(string location)
		{
			var path = "";
			if (location.Contains(".." + Path.DirectorySeparatorChar))
				path = new PathParser(Environment.CurrentDirectory).ToAbsolute(location);
			else
				path = Path.GetFullPath(location);

			if (File.Exists(path) || Path.GetFileName(path).Contains("*"))
				path = Path.GetDirectoryName(path);

			path = Path.GetFullPath(path);
			var projectFile = new ProjectLocator().Locate(path);
			if (projectFile == null)
			{
				Console.WriteLine("error|Could not locate a project file for {0}", path);
				return null;
			}
			return getTypesProviderByProject(projectFile);
		}

		static ProviderSettings getTypesProviderByProject(string projectFile)
		{
			var fs = new FS();
			var projectReferencer = new ProjectReferencer(fs, getTypesProviderByProject);
			var assemblyReferencer = new AssemblyReferencer(fs);
			var resolver = 
				new ProjectVersionResolver(new IProvideVersionedTypes[] {
						new VersionedTypeProvider<VS2010>(
							new IReadProjectsFor[] { new DefaultReader(fs) },
							new IResolveFileTypes[] { getFileTypeResolver() },
							new IAppendFiles[] { new VSFileAppender(fs) },
							new IRemoveFiles[] { new DefaultRemover(fs) },
							new IWriteProjectFileToDiskFor[] { new DefaultWriter() },
							new IAddReference[] { projectReferencer, assemblyReferencer },
							new IRemoveReference[] { projectReferencer, assemblyReferencer })
					});
			var project = resolver.ResolveFor(projectFile);
			if (project == null)
				Console.WriteLine("Unsupported poject version for project {0}", projectFile);
			return new ProviderSettings(projectFile, project);
		}
	}

	public class ProviderSettings
	{
		public string ProjectFile { get; private set; }
		public IProvideVersionedTypes TypesProvider { get; private set; }
		
		public ProviderSettings(string projectFile, IProvideVersionedTypes provider)
		{
			ProjectFile = projectFile;
			TypesProvider = provider;
		}
	}
}
