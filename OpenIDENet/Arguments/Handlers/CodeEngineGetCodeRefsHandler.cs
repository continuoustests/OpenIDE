using System;
using OpenIDENet.CodeEngineIntegration;
using OpenIDENet.Core.Language;

namespace OpenIDENet.Arguments.Handlers
{
	class CodeEngineGetCodeRefsHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;
		
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Queries for code references");
				return usage;
			}
		}

		public string Command { get { return "get-code-refs"; } }
		
		public CodeEngineGetCodeRefsHandler(ICodeEngineLocator codeEngineFactory)
		{
			_codeEngineFactory = codeEngineFactory;
		}
		
		public void Execute (string[] arguments)
		{
			var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				return;
			instance.GetCodeRefs();
		}
	}
}
