using System;
using System.Windows.Forms;
using OpenIDENet.CodeEngine.Core.Crawlers;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Endpoints;
using System.IO;
using System.Threading;

namespace OpenIDENet.CodeEngine
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length != 1)
				return;
			var path = args[0];
			if (!Directory.Exists(path) && !File.Exists(path))
				return;
			
			var options = new CrawlOptions(path);
			var cache = new TypeCache();
			new CSharpCrawler(cache).InitialCrawl(options);
			
			var endpoint = new CommandEndpoint(options.Directory, cache);
			endpoint.Start(path);
			while (endpoint.IsAlive)
				Thread.Sleep(100);
		}
	}
}

