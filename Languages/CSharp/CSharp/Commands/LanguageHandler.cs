using System;

namespace CSharp.Commands
{
	class LanguageHandler : ICommandHandler
	{
		public string Usage { get { return null; } }
		public string Command { get { return "get-language"; } }

		public void Execute(string[] args)
		{
			Console.WriteLine("C#");
		}
	}
}
