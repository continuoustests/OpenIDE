using System;
using OpenIDE.Core.Language;
using OpenIDE.Core.CodeEngineIntegration;

namespace OpenIDE.Arguments.Handlers
{
	class GoToDefinitionHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Goes to the signature definition of the reference at specified position");
				usage.Add("FILE|LINE|COLUMN", "Position for where to fetch signature");
				return usage;
			}
		}
		
		public string Command { get { return "goto-definition"; } }
	
		public GoToDefinitionHandler(ICodeEngineLocator locator)
		{
			_codeEngineFactory = locator;
		}

		public void Execute(string[] arguments)
		{
			using (var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory)) {
				if (instance == null)
					return;
				instance.GoToDefinition(arguments);
			}
		}
	}
}
