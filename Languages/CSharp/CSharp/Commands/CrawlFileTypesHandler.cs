using System;
using CSharp.Responses;

namespace CSharp.Commands
{
	class CrawlFileTypesHandler : ICommandHandler
	{
		public string Usage { get { return null; } }

		public string Command { get { return "crawl-file-types"; } }

		public void Execute(IResponseWriter writer, string[] args)
		{
			writer.Write(".csproj|.cs");
		}
	}
}
