using System;
using System.Linq;
using System.IO;
using CSharp.Crawlers;
namespace CSharp.Commands
{
	class CrawlHandler : ICommandHandler
	{
		public string Usage { get { return null; } }

		public string Command { get { return "crawl-source"; } }

		public void Execute(string[] args)
		{
			var output = new OutputWriter();
			if (args.Length != 1)
			{
				output.Error("crawl-source requires one parameters which is path to the " +
							 "file containing files/directories to crawl");
				return;
			}
			var crawler = new CSharpCrawler(output);
			File.ReadAllText(
				args[0])
					.Split(new string[] { 
						Environment.NewLine
					}, StringSplitOptions.RemoveEmptyEntries
					).ToList()
				.ForEach(x =>
					{
						crawler.Crawl(new CrawlOptions(x));
					});
		}
	}
}
