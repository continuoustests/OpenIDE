using System;
using System.Linq;

namespace OpenIDE.CodeEngine.Core.Caching
{
	public class PluginFinder
	{
		private ICacheBuilder _cache;

		public PluginFinder(ICacheBuilder cache)
		{
			_cache = cache;
		}
		
		public string FindLanguage(string nameOfExtension)
		{
			var plugin = _cache.Plugins.FirstOrDefault(x => x.Name == nameOfExtension);
			if (plugin != null)
				return plugin.Name;
			plugin = _cache.Plugins.FirstOrDefault(x => x.Extensions.Contains(nameOfExtension));
			if (plugin != null)
				return plugin.Name;
			return null;
		}
	}
}
