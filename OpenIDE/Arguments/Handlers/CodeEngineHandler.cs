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
			Console.WriteLine("Handling go to type");
			instance.GoToType();
		}
	}
}

