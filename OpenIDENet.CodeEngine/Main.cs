using System;
using System.Windows.Forms;
using OpenIDENet.CodeEngine.Core.Crawlers;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Endpoints;
using System.IO;
using System.Threading;
using OpenIDENet.CodeEngine.Core.ChangeTrackers;
using OpenIDENet.CodeEngine.Core.Logging;
using OpenIDENet.CodeEngine.Core.UI;
using OpenIDENet.CodeEngine.Core.EditorEngine;

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
			
			Logger.Assign(new FileLogger());
			var options = new CrawlOptions(path);
			var cache = new TypeCache();
			var wather = new CSharpFileTracker();
			wather.Start(options.Directory, cache);
			new CSharpCrawler(cache).InitialCrawl(options);
			
			var endpoint = new CommandEndpoint(options.Directory, cache, handleMessage);
			endpoint.Start(path);
			while (endpoint.IsAlive)
				Thread.Sleep(100);
		}

		private static void handleMessage(string message, ITypeCache cache, Editor editor)
		{
			if (message == "gototype")
				goToType(cache, editor);
			if (message == "explore")
				explore(cache, editor);
		}

		private static void goToType(ITypeCache cache, Editor editor)
		{
			var form = new TypeSearchForm(cache, (file, line, column) => { editor.GoTo(file, line, column); }, () => { editor.SetFocus(); });
			form.ShowDialog();
		}

        private static void explore(ITypeCache cache, Editor editor)
        {
            var form = new FileExplorer(cache, (file, line, column) => { editor.GoTo(file, line, column); }, () => { editor.SetFocus(); });
            form.ShowDialog();
        }
	}
}

