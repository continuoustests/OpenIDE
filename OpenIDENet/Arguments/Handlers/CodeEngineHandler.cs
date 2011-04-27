using System;
using OpenIDENet.CodeEngineIntegration;
namespace OpenIDENet.Arguments.Handlers
{
	class CodeEngineHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;
		
		public string Command { get { return "gototype"; } }
		
		public CodeEngineHandler(ICodeEngineLocator codeEngineFactory)
		{
			_codeEngineFactory = codeEngineFactory;
		}
		
		public void Execute (string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation)
		{
			Console.WriteLine("Handling go to type");
			var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				return;
			instance.GoToType();
		}
	}
}

