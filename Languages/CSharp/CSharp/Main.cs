using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using CSharp.Commands;
using CSharp.Files;
using CSharp.FileSystem;
using CSharp.Projects;
using CSharp.Responses;
using CSharp.Versioning;
using CSharp.Projects.Readers;
using CSharp.Projects.Writers;
using CSharp.Projects.Appenders;
using CSharp.Projects.Removers;
using CSharp.Projects.Referencers;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Config;
using OpenIDE.Core.Commands;
using OpenIDE.Core.Logging;

namespace CSharp
{
	public class MainClass
	{
        private static Dispatcher _dispatcher;
        private static string _keyPath;
        private static IOutputWriter _cache = new NullOutputWriter();

		public static void Main(string[] args)
		{
			if (args.Length == 0)
				return;

            if (args[0] == "initialize") {
            	Logger.Write("Running C# plugin as daemon");
				var writer = new ConsoleResponseWriter();
            	_keyPath = args[1];
				setupLogging(_keyPath);
            	_cache = new OutputWriter(new NullResponseWriter());
            	_dispatcher = new Dispatcher();
				configureHandlers(_dispatcher);

            	writer.Write("initialized");
            	while (true) {
					var msg = CommandMessage.New(Console.ReadLine());
					Logger.Write("Handling command " + msg);
					if (msg.Command == "shutdown") {
						writer.Write("end-of-conversation");
						break;
					}
		            handleMessage(msg, writer, false);
		            writer.Write("end-of-conversation");
	            }
            } else {
            	Logger.Write("Running C# plugin as comman-line app");
            	_keyPath = Environment.CurrentDirectory;
				setupLogging(_keyPath);
            	_cache = new OutputWriter(new NullResponseWriter());
            	_dispatcher = new Dispatcher();
				configureHandlers(_dispatcher);

            	var msg = new CommandMessage(args[0], null, getParameters(args));
            	handleMessage(msg, new ConsoleResponseWriter(), false);
            }
            Logger.Write("Shutting down C# plugin");
        }

		private static void setupLogging(string path) {
			var reader = new ConfigReader(path);
			var logPath = reader.Get("oi.logpath");
			if (Directory.Exists(logPath))
            	Logger.Assign(new FileLogger(Path.Combine(logPath, "C#.log")));
		}

        private static void handleMessage(CommandMessage msg, IResponseWriter writer, bool serverMode)
        {
            if (msg == null)
                return;
            Logger.Write("Handling message: " + msg);
			var handler = _dispatcher.GetHandler(msg.Command);
			if (handler == null) {
				writer.Write("error|" + msg.Command + " is not a valid C# plugin command. For a list of commands type oi.");
				return;
			}
			try {
				if (handler == null)
					return;
				handler.Execute(writer, msg.Arguments.ToArray());
			} catch (Exception ex) {
				var builder = new OutputWriter(writer);
                writeException(builder, ex);
			}
			Logger.Write("Done handling message");
		}

		static string[] getParameters(string[] args)
		{
			var remaining = new List<string>();
		   	for (int i = 1; i < args.Length; i++)
				remaining.Add(args[i]);
			return remaining.ToArray();
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
				Console.WriteLine("error|Unsupported poject version for project {0}", projectFile);
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
