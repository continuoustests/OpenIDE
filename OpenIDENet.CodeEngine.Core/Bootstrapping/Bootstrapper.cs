using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Commands;
using OpenIDENet.CodeEngine.Core.Handlers;
using OpenIDENet.CodeEngine.Core.Endpoints;
using OpenIDENet.CodeEngine.Core.ChangeTrackers;
using OpenIDENet.CodeEngine.Core.Logging;
using OpenIDENet.CodeEngine.Core.EditorEngine;
using OpenIDENet.Core.Language;
using OpenIDENet.Core.Windowing;


namespace OpenIDENet.CodeEngine.Core.Bootstrapping
{
	public class Bootstrapper
	{
		private static string _path;
		private static List<IHandler> _handlers = new List<IHandler>();
		private static CommandEndpoint _endpoint;

		public static CommandEndpoint GetEndpoint(string path)
		{
			_path = path;
			Logger.Assign(new FileLogger());
            var cache = new TypeCache();
			var crawlHandler = new CrawlHandler(cache);
			var pluginLocator = new PluginLocator(
				Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
				(msg) => {});
			initPlugins(pluginLocator, crawlHandler);
			var tracker = new PluginFileTracker();
			tracker.Start(
				_path,
				cache,
				pluginLocator);

			

            _endpoint = new CommandEndpoint(_path, cache);
			_endpoint.AddHandler(messageHandler);
			
			_handlers.AddRange(new IHandler[] {
					new GetCodeRefsHandler(_endpoint, cache)
				});
			return _endpoint;
		}

		private static void messageHandler(string message, ITypeCache cache, Editor editor)
		{
			var msg = CommandMessage.New(message);
			_handlers
				.Where(x => x.Handles(msg)).ToList()
				.ForEach(x => x.Handle(msg));
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
