using System;
using System.Linq;

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

		public void Execute(string[] args)
		{
			var output = "";
			_dispatcher.GetHandlers()
				.Where(x => x.Usage != null).ToList()
				.ForEach(x => output += x.Usage + " ");
			Console.WriteLine(output);
		}
	}
}
