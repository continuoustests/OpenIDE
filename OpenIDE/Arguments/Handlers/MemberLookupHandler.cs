using System;
using OpenIDE.Core.Language;
using OpenIDE.CodeEngineIntegration;

namespace OpenIDE.Arguments.Handlers
{
	class MemberLookupHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Launches the member lookup window");
				usage.Add("FILE|LINE|COLUMN", "Position for where to fetch signature");
				return usage;
			}
		}

		public string Command { get { return "member-lookup"; } }

		public MemberLookupHandler(ICodeEngineLocator locator)
		{
			_codeEngineFactory = locator;
		}

		public void Execute(string[] arguments)
		{
			var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				return;
			instance.MemberLookup(arguments);
		}
	}
}
