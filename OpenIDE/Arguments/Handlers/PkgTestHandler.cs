using System;
using System.IO;
using System.Diagnostics;
using OpenIDE.Core.Language;
using CoreExtensions;

namespace OpenIDE.Arguments.Handlers
{
	class PkgTestHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Runs tests for all or parts of the system");
				return usage;
			}
		}

		public string Command { get { return "pkgtest"; } }

		public void Execute(string[] arguments) {
			new Process()
				.Query(
					Path.Combine(Environment.CurrentDirectory, "mytest.oi-pgk-tests.bat"),
					"",
					true,
					Environment.CurrentDirectory,
					(error, line) => Console.WriteLine(line));
			Console.WriteLine("Testing shit *.oi-pkg-tests.*");
		}
	}
}