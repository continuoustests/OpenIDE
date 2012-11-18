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
        private static bool _startService = false;
        private static Dispatcher _dispatcher;
        private static string _keyPath;
        private static Instance _codeEngine;
        private static IOutputWriter _cache = new NullOutputWriter();

		public static void Main(string[] args)
		{
			//args = new[] {"send","crawl-source","~/src/OpenIDE"};
			if (args.Length == 0)
				return;
            args = parseParameters(args);
            _dispatcher = new Dispatcher();
			configureHandlers(_dispatcher);
            CommandMessage msg = null;
            if (args.Length > 0)
                msg = new CommandMessage(args[0], null, getParameters(args));
            if (_startService)
                startServer(msg);
            else
                handleMessage(msg, new ConsoleResponseWriter(), false);
        }

        private static void startServer(CommandMessage startMessage) {
            var shutdown = false;
            var path = _keyPath;
            var server = new TcpServer();
            server.IncomingMessage += (s, m) => {
                var writer = new ServerResponseWriter(server, m.ClientID);
                var msg = CommandMessage.New(m.Message);
                handleMessage(msg, writer, true);
                writer.Write("EndOfConversation");
                if (msg.Command == "shutdown")
                    shutdown = true;
            };
            server.Start();
            var token = TokenHandler.WriteInstanceInfo(path, server.Port);
            while (!shutdown)
                System.Threading.Thread.Sleep(100);
            if (File.Exists(token))
                File.Delete(token);
        }

        private static void handleMessage(CommandMessage msg, IResponseWriter writer, bool serverMode)
        {
            if (msg == null)
                return;
			var handler = _dispatcher.GetHandler(msg.Command);
			if (!serverMode) {
				var client = TokenHandler.GetClient(_keyPath, (m) => writer.Write(m));
	            var send = new SendHandler(client);
	            if (client.IsConnected) {
	            	if (send.Command == msg.Command) {
	            		handler = send;
					} else {
						var args = new List<string>();
						args.Add(msg.Command);
						args.AddRange(msg.Arguments);
						msg = new CommandMessage("send", null, args.ToArray());
	            		handler = send;
					}
	            } else {
					if (msg.Command == "send") {
						msg = new CommandMessage(msg.Arguments[0], null, getParameters(msg.Arguments.ToArray()));
	            		handler = _dispatcher.GetHandler(msg.Command);
					}
					if (handler == null)
						return;
				}
			}
			try {
				handler.Execute(writer, msg.Arguments.ToArray());
			} catch (Exception ex) {
				var builder = new OutputWriter(writer);
                writeException(builder, ex);
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

        static string[] parseParameters(string[] args) {
            _cache = new OutputWriter(new NullResponseWriter());
            _codeEngine =
                new CodeEngineDispatcher(new OpenIDE.Core.FileSystem.FS())
                    .GetInstance(Environment.CurrentDirectory);
            if (args[0] == "initialize") {
                _startService = true;
                if (args.Length == 2)
                    _keyPath = args[1];
                else
                    _keyPath = Environment.CurrentDirectory;
            } else {
                if (_codeEngine != null)
                    _keyPath = _codeEngine.Key;
                else
                    _keyPath = Environment.CurrentDirectory;
            }
            return args;
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
			dispatcher.Register(new CrawlHandler(_cache));
			dispatcher.Register(new CrawlFileTypesHandler());
			dispatcher.Register(new CreateHandler(getReferenceTypeResolver()));
			dispatcher.Register(new AddFileHandler(getTypesProvider));
			dispatcher.Register(new DeleteFileHandler(getTypesProvider));
			dispatcher.Register(new DereferenceHandler(getTypesProvider));
			dispatcher.Register(new NewHandler(getFileTypeResolver(), getTypesProvider));
			dispatcher.Register(new ReferenceHandler(getTypesProvider));
			dispatcher.Register(new RemoveFileHandler(getTypesProvider));
			dispatcher.Register(new SignatureFromPositionHandler(_cache));
			dispatcher.Register(new MembersFromUnknownSignatureHandler());
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
