using System;
using System.Linq;
using System.Collections.Generic;
using CSharp.Commands;
using CSharp.Files;

namespace CSharp
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
				return;
			var dispatcher = new Dispatcher();
			configureHandlers(dispatcher);
			var handler = dispatcher.GetHandler(args[0]);
			if (handler == null)
				return;
			handler.Execute(getParameters(args));
		}

		static string[] getParameters(string[] args)
		{
			var remaining = new List<string>();
			for (int i = 1; i < args.Length; i++)
				remaining.Add(args[i]);
			return remaining.ToArray();
		}

		static void configureHandlers(Dispatcher dispatcher)
		{
			dispatcher.Register(new GetUsageHandler(dispatcher));
			dispatcher.Register(new CreateHandler(new VSFileTypeResolver()));
			dispatcher.Register(new AddFileHandler());
			dispatcher.Register(new DeleteFileHandler());
			dispatcher.Register(new DereferenceHandler());
			dispatcher.Register(new NewHandler(new VSFileTypeResolver()));
			dispatcher.Register(new ReferenceHandler());
			dispatcher.Register(new RemoveFileHandler());
		}
	}
}
