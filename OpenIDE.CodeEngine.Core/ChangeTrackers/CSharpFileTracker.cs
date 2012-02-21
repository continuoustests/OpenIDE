using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Logging;
using System.Linq;
using OpenIDE.Core.Language;
namespace OpenIDE.CodeEngine.Core.ChangeTrackers
{
	public class PluginFileTracker : IDisposable
	{
		private List<PluginPattern> _plugins = new List<PluginPattern>();
		private FileChangeTracker _tracker;
		private ICacheBuilder _cache;
		private ICrawlResult _crawlReader;
		
		public void Start(string path, ICacheBuilder cache, ICrawlResult crawlReader, PluginLocator pluginLocator)
		{
			_cache = cache;
			_crawlReader = crawlReader;
			_tracker = new FileChangeTracker();
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
			               
		private void handleChanges(Stack<FileSystemEventArgs> buffer)
		{
			var cacheHandler = new CrawlHandler(_crawlReader);
			var files = getChanges(buffer);
			files.ForEach(x =>
				{
					_cache.Invalidate(x.FullPath);
					handle(x);
				});
			_plugins.ForEach(x => x.Handle(cacheHandler));
		}
		
		private List<FileSystemEventArgs> getChanges(Stack<FileSystemEventArgs> buffer)
		{
			var list = new List<FileSystemEventArgs>();
			while (buffer.Count != 0)
			{
				var item = buffer.Pop();
				if (item != null && !list.Contains(item))
					list.Add(item);
			}
			return list;
		}
		
		private void handle(FileSystemEventArgs file)
		{
			if (file == null)
				return;
			var extension = Path.GetExtension(file.FullPath).ToLower();
			if (extension == null)
				return;
			_plugins.ForEach(x =>
				{
					if (x.Supports(extension) && !x.FilesToHandle.Contains(file.FullPath))
						x.FilesToHandle.Add(file.FullPath);
				});
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
			foreach (var line in Plugin.Crawl(FilesToHandle))
				cacheHandler.Handle(line);
			FilesToHandle.Clear();
		}
	}
}

