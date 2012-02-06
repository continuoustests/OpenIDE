using System;

namespace CSharp.Commands
{
	class SignatureFromPositionHandler : ICommandHandler
	{
		public string Usage { get { return null; } }

		public string Command { get { return "signature-from-position"; } }
		
		public void Execute(string[] args)
		{
			// TODO when the code parsing is done implement this
		}
	}
}
