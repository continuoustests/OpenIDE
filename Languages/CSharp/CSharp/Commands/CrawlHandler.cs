using System;
using System.Linq;
using System.IO;
using OpenIDE.Core.Logging;
using CSharp.Crawlers;
using CSharp.Responses;

namespace CSharp.Commands
{
	class CrawlHandler : ICommandHandler
	{
        private IOutputWriter _mainCache;

        public CrawlHandler(IOutputWriter mainCache)
        {
            _mainCache = mainCache;
        }

		public string Usage { get { return null; } }

		public string Command { get { return "crawl-source"; } }

		public void Execute(IResponseWriter writer, string[] args)
		{
			var output = new OutputWriter(writer);
			if (args.Length != 1)
			{
                output.WriteError("crawl-source requires one parameters which is path to the " +
							    "file containing files/directories to crawl");
				return;
			}
			var crawler = new CSharpCrawler(output, _mainCache);
            if (Directory.Exists(args[0])) {
                crawler.Crawl(new CrawlOptions(args[0]));
            } else {
			    File.ReadAllText(args[0])
					    .Split(new string[] {  Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList()
				        .ForEach(x => {
						        crawler.Crawl(new CrawlOptions(x));
					        });
            }
            System.Threading.ThreadPool.QueueUserWorkItem((m) => {
                Logger.Write("Merging crawl result into cache");
                var cacheToMerge = (IOutputWriter)m;
                if (_mainCache == null)
                    return;
                _mainCache.MergeWith(cacheToMerge);
                Logger.Write("Disposing and cleaning up");
                cacheToMerge.Dispose();
                GC.Collect(5);
            }, output);
		}
	}
}
