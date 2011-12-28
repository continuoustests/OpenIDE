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
	class MainClass
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
			handler.Execute(getParameters(args));
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
			dispatcher.Register(new CreateHandler(new VSFileTypeResolver()));
			dispatcher.Register(new AddFileHandler(getTypesProvider));
			dispatcher.Register(new DeleteFileHandler(getTypesProvider));
			dispatcher.Register(new DereferenceHandler(getTypesProvider));
			dispatcher.Register(new NewHandler(new VSFileTypeResolver(), getTypesProvider));
			dispatcher.Register(new ReferenceHandler(getTypesProvider));
			dispatcher.Register(new RemoveFileHandler(getTypesProvider));
		}
		
		static ProviderSettings getTypesProvider(string location)
		{
			location = Path.GetFullPath(location);
			if (File.Exists(location))
				location = Path.GetDirectoryName(location);
			var projectFile = new ProjectLocator().Locate(location);
			if (projectFile == null)
			{
				Console.WriteLine("Could not locate a project file for {0}", location);
				return null;
			}
			var fs = new FS();
			var project = 
				new ProjectVersionResolver(new IProvideVersionedTypes[] {
						new VersionedTypeProvider<VS2010>(
							new IReadProjectsFor[] { new DefaultReader(fs) },
							new IResolveFileTypes[] { new VSFileTypeResolver() },
							new IAppendFiles[] { new VSFileAppender(fs) },
							new IRemoveFiles[] { new DefaultRemover(fs) },
							new IWriteProjectFileToDiskFor[] { new DefaultWriter() },
							new IAddReference[] { new ProjectReferencer(fs), new AssemblyReferencer(fs) },
							new IRemoveReference[] { new ProjectReferencer(fs), new AssemblyReferencer(fs) })
					})
					.ResolveFor(projectFile);
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
