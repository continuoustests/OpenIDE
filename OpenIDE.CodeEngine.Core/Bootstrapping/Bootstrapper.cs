using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Commands;
using OpenIDE.CodeEngine.Core.Handlers;
using OpenIDE.CodeEngine.Core.Endpoints;
using OpenIDE.CodeEngine.Core.Endpoints.Tcp;
using OpenIDE.CodeEngine.Core.ChangeTrackers;
using OpenIDE.CodeEngine.Core.Logging;
using OpenIDE.CodeEngine.Core.EditorEngine;
using OpenIDE.Core.Caching;
using OpenIDE.Core.Language;
using OpenIDE.Core.Windowing;


namespace OpenIDE.CodeEngine.Core.Bootstrapping
{
	public class Bootstrapper
	{
		private static string _path;
		private static PluginFileTracker _tracker;
		private static List<IHandler> _handlers = new List<IHandler>();
		private static CommandEndpoint _endpoint;
		private static TypeCache _cache;

		public static CommandEndpoint GetEndpoint(string path, string[] enabledLanguages)
		{
			_path = path;
			Logger.Assign(new FileLogger());
            _cache = new TypeCache();
			var crawlHandler = new CrawlHandler(_cache, (s) => Logger.Write(s));
			var pluginLocator = new PluginLocator(
				enabledLanguages,
				Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
				(msg) => {});
			initPlugins(pluginLocator, crawlHandler);
			_tracker = new PluginFileTracker();
			_tracker.Start(
				_path,
				_cache,
				_cache,
				pluginLocator);

            _endpoint = new CommandEndpoint(_path, _cache);
			_endpoint.AddHandler(messageHandler);
			
			_handlers.AddRange(new IHandler[] {
					new GetProjectsHandler(_endpoint, _cache),
					new GetFilesHandler(_endpoint, _cache),
					new GetCodeRefsHandler(_endpoint, _cache),
					new GetSignatureRefsHandler(_endpoint, _cache),
					new GoToDefinitionHandler(_endpoint, _cache, pluginLocator),
					new FindTypeHandler(_endpoint, _cache),
					new SnippetEditHandler(_endpoint, _cache, _path),
					new SnippetDeleteHandler(_cache, _path)
				});
			return _endpoint;
		}

		public static ICacheBuilder GetCacheBuilder()
		{
			return _cache;
		}

		public static void Shutdown()
		{
			_tracker.Dispose();
		}

		private static void messageHandler(MessageArgs message, ITypeCache cache, Editor editor)
		{
			var msg = CommandMessage.New(message.Message);
			_handlers
				.Where(x => x.Handles(msg)).ToList()
				.ForEach(x => x.Handle(message.ClientID, msg));
		}
		
		private static void initPlugins(PluginLocator locator, CrawlHandler handler)
		{
			new Thread(() =>
				{
					locator.Locate().ToList()
						.ForEach(x => 
							{
								try {
									foreach (var line in x.Crawl(new string[] { _path }))
										handler.Handle(line);
								} catch (Exception ex) {
									Logger.Write(ex.ToString());
								}
							});
				}).Start();
		}
	}
}
