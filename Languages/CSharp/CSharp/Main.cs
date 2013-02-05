using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using CSharp.Commands;
using CSharp.Files;
using CSharp.FileSystem;
using CSharp.Projects;
using CSharp.Responses;
using CSharp.Tcp;
using CSharp.Versioning;
using CSharp.Projects.Readers;
using CSharp.Projects.Writers;
using CSharp.Projects.Appenders;
using CSharp.Projects.Removers;
using CSharp.Projects.Referencers;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Commands;

namespace CSharp
{
	public class MainClass
	{
        private static Dispatcher _dispatcher;
        private static string _keyPath;
        private static Instance _codeEngine;
        private static IOutputWriter _cache = new NullOutputWriter();

		public static void Main(string[] args)
		{
			_cache = new OutputWriter(new NullResponseWriter());
            _codeEngine =
                new CodeEngineDispatcher(new OpenIDE.Core.FileSystem.FS())
                    .GetInstance(Environment.CurrentDirectory);
            _keyPath = args[0];

            _dispatcher = new Dispatcher();
			configureHandlers(_dispatcher);

			while (true) {
				var msg = CommandMessage.New(Console.ReadLine());
				if (msg.Command == "shutdown")
					break;
	            handleMessage(msg, new ConsoleResponseWriter(), false);
            }
        }

        private static void handleMessage(CommandMessage msg, IResponseWriter writer, bool serverMode)
        {
            if (msg == null)
                return;
			var handler = _dispatcher.GetHandler(msg.Command);
			if (handler == null) {
				writer.Write("comment|" + msg.Command + " is not a valid C# plugin command. For a list of commands type oi.");
				return;
			}
			try {
				if (handler == null)
					return;
				handler.Execute(writer, msg.Arguments.ToArray());
			} catch (Exception ex) {
				var builder = new OutputWriter(writer);
                writeException(builder, ex);
			} finally {
				writer.Write("end-of-conversation");
			}
		}

		static void writeException(OutputWriter builder, Exception ex) {
			if (ex == null)
				return;
			builder.WriteError(ex.Message.Replace(Environment.NewLine, ""));
			if (ex.StackTrace != null) {
				ex.StackTrace
					.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    .ForEach(line => builder.WriteError(line));
			}
			writeException(builder, ex.InnerException);
		}
		
		static void configureHandlers(Dispatcher dispatcher)
		{
			dispatcher.Register(new GetUsageHandler(dispatcher));
			dispatcher.Register(new CrawlHandler(_cache));
			dispatcher.Register(new CrawlFileTypesHandler());
			dispatcher.Register(new CreateHandler(getReferenceTypeResolver(), _keyPath));
			dispatcher.Register(new AddFileHandler(getTypesProvider));
			dispatcher.Register(new DeleteFileHandler(getTypesProvider));
			dispatcher.Register(new ReferenceHandler(getTypesProvider, _keyPath));
			dispatcher.Register(new DereferenceHandler(getTypesProvider, _keyPath));
			dispatcher.Register(new NewHandler(getFileTypeResolver(), getTypesProvider, _keyPath));
			dispatcher.Register(new RemoveFileHandler(getTypesProvider));
			dispatcher.Register(new SignatureFromPositionHandler(_cache));
			dispatcher.Register(new MembersFromUnknownSignatureHandler());
			dispatcher.Register(new GoToDefinitionHandler(_cache));
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
