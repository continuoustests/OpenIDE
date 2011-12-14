using System;
using OpenIDENet.Languages;
using OpenIDENet.CodeEngineIntegration;
namespace OpenIDENet.Arguments.Handlers
{
	class CodeEngineExploreHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;
		
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					SupportedLanguage.CSharp,
					CommandType.FileCommand,
					Command,
					"Launches the file explorer window");
				return usage;
			}
		}

		public string Command { get { return "explore"; } }
		
		public CodeEngineExploreHandler(ICodeEngineLocator codeEngineFactory)
		{
			_codeEngineFactory = codeEngineFactory;
		}
		
		public void Execute (string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation)
		{
			Console.WriteLine("Handling explore");
			var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				return;
			instance.Explore();
		}
	}
}

