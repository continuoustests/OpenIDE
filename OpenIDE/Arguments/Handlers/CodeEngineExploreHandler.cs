using System;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Language;
namespace OpenIDE.Arguments.Handlers
{
	class CodeEngineExploreHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;
		
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Launches the code engines file explorer window");
				return usage;
			}
		}

		public string Command { get { return "explore"; } }
		
		public CodeEngineExploreHandler(ICodeEngineLocator codeEngineFactory)
		{
			_codeEngineFactory = codeEngineFactory;
		}
		
		public void Execute (string[] arguments)
		{
			Console.WriteLine("Handling explore");
			var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				return;
			instance.Explore();
		}
	}
}

