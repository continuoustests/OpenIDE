using System;
using OpenIDENet.Languages;
using OpenIDENet.CodeEngineIntegration;
namespace OpenIDENet.Arguments.Handlers
{
	class CodeEngineGoToHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;
		
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					SupportedLanguage.All,
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
			Console.WriteLine("Handling go to type");
			var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				return;
			instance.GoToType();
		}
	}
}

