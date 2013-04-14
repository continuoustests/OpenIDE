using System;
using System.Linq;
using CSharp.Responses;

namespace CSharp.Commands
{
	class GetUsageHandler : ICommandHandler
	{
		private Dispatcher _dispatcher;
		public string Usage { get { return null; } }
		
		public string Command { get { return "get-command-definitions"; } }

		public GetUsageHandler(Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public void Execute(IResponseWriter writer, string[] args)
		{
			var output = "";
			_dispatcher.GetHandlers()
				.Where(x => x.Usage != null).ToList()
				.ForEach(x => output += x.Usage + " ");
			writer.Write(output);
		}
	}
}
