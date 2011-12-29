using System;

namespace CSharp.Commands
{
	class CrawlFileTypesHandler : ICommandHandler
	{
		public string Usage { get { return null; } }

		public string Command { get { return "crawl-file-types"; } }

		public void Execute(string[] args)
		{
			Console.WriteLine("*.csproj|*.cs");
		}
	}
}
