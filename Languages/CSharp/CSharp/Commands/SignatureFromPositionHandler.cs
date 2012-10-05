using System;

namespace CSharp.Commands
{
	class SignatureFromPositionHandler : ICommandHandler
	{
		public string Usage { get { return null; } }

		public string Command { get { return "signature-from-position"; } }
		
		public void Execute(string[] args)
		{
			// parse arguments
            // get all dirty buffers
            // parse all dirty buffers
            // parse given file (with variable declaration)
            // get variable / member / type name under or behind position
            // find type of declaration
            // clean up temp buffers
            // return type
		}
	}
}
