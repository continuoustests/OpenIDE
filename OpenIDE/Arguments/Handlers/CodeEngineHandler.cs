using System;
using OpenIDE.CodeEngineIntegration;
using OpenIDE.Core.Language;
namespace OpenIDE.Arguments.Handlers
{
	class CodeEngineGoToHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;
		
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Launches the code engines type search window");
				usage.Add("[SEARCH-STRING]", "When passed it will perform the search in the command line");
				return usage;
			}
		}

		public string Command { get { return "gototype"; } }
		
		public CodeEngineGoToHandler(ICodeEngineLocator codeEngineFactory)
		{
			_codeEngineFactory = codeEngineFactory;
		}
		
		public void Execute (string[] arguments)
		{
			var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				return;
			if (arguments.Length == 0)
				instance.GoToType();
			else
				consoleSearch(instance, arguments[0]);
		}

		private void consoleSearch(Instance instance, string search)
		{
			var result = instance.GetCodeRefs("name=*" + search + "*");
		}
	}
}

