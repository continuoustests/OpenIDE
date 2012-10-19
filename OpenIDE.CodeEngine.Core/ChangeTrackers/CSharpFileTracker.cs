using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.Core.Logging;
using OpenIDE.CodeEngine.Core.Endpoints;
using System.Linq;
using OpenIDE.Core.Language;
using OpenIDE.Core.Caching;
namespace OpenIDE.CodeEngine.Core.ChangeTrackers
{
	public class PluginFileTracker : IDisposable
	{
		private EventEndpoint _eventDispatcher;
		private List<PluginPattern> _plugins = new List<PluginPattern>();
		private FileChangeTracker _tracker;
		private ICacheBuilder _cache;
		private ICrawlResult _crawlReader;
		
		public void Start(
			string path,
			ICacheBuilder cache,
			ICrawlResult crawlReader,
			PluginLocator pluginLocator,
			EventEndpoint eventDispatcher)
		{
			_cache = cache;
			_crawlReader = crawlReader;
			_eventDispatcher = eventDispatcher;
			_tracker = new FileChangeTracker((x) => {
					_eventDispatcher.Send(
						"codemodel raw-filesystem-change-" +
						x.Type.ToString().ToLower() +
						" \"" + x.Path + "\"");
				});
			pluginLocator.Locate().ToList()
				.ForEach(x =>
					{
						var plugin = new PluginPattern(x);
						_plugins.Add(plugin);
						_cache.Plugins.Add(
							new CachedPlugin(x.GetLanguage(), plugin.Patterns));
					});
			_tracker.Start(path, getFilter(), handleChanges);
		}

		private string getFilter()
		{
			var pattern = "";
			_plugins
				.ForEach(x => 
					{
						x.Patterns
							.ForEach(y =>
								{
									if (pattern == "")
										pattern = "*" + y;
									else
										pattern += "|*" + y;
								});
					});
			return pattern;
		}
			               
		private void handleChanges(Stack<Change> buffer)
		{
			var cacheHandler = new CrawlHandler(_crawlReader, (s) => Logger.Write(s));
			var files = getChanges(buffer);
			files.ForEach(x =>
				{
					_cache.Invalidate(x.Path);
					handle(x);
				});
			_plugins.ForEach(x => x.Handle(cacheHandler));
		}
		
		private List<Change> getChanges(Stack<Change> buffer)
		{
			var list = new List<Change>();
			while (buffer.Count != 0)
			{
				var item = buffer.Pop();
				if (item != null && !list.Exists(x => x.Path.Equals(item.Path)))
					list.Add(item);
			}
			return list;
		}
		
		private void handle(Change file)
		{
			if (file == null)
				return;
			var extension = Path.GetExtension(file.Path).ToLower();
			if (extension == null)
				return;
			
			_eventDispatcher.Send(
				"codemodel filesystem-change-" +
				file.Type.ToString().ToLower() +
				" \"" + file.Path + "\"");
			
			_plugins.ForEach(x =>
				{
					if (x.Supports(extension) && !x.FilesToHandle.Contains(file.Path))
						x.FilesToHandle.Add(file.Path);
			 	});

			_eventDispatcher.Send("codemodel file-crawled \"" + file.Path + "\"");
		}

		public void Dispose()
		{
			_tracker.Dispose();
		}
	}

	class PluginPattern
	{
		public LanguagePlugin Plugin { get; private set; }
		public List<string> Patterns { get; private set; }
		public List<string> FilesToHandle { get; private set; }

		public PluginPattern(LanguagePlugin plugin)
		{
			Plugin = plugin;
			Patterns = new List<string>();
			Patterns.AddRange(
				Plugin
					.GetCrawlFileTypes()
						.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
			FilesToHandle = new List<string>();
		}

		public bool Supports(string extension)
		{
			return Patterns.Count(x => x.ToLower().Equals(extension)) > 0;
		}

		public void Handle(CrawlHandler cacheHandler)
		{
			if (FilesToHandle.Count == 0)
				return;
			cacheHandler.SetLanguage(Plugin.GetLanguage());
			Plugin.Crawl(FilesToHandle, (line) => cacheHandler.Handle(line));
			FilesToHandle.Clear();
		}
	}
}

