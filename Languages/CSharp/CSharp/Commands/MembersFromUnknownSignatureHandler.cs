using System;

namespace CSharp.Commands
{
	class MembersFromUnknownSignatureHandler : ICommandHandler
	{
		public string Usage { get { return null; } }

		public string Command { get { return "members-from-signature"; } }
		
		public void Execute(string[] args)
		{
			// TODO when the code parsing is done implement this
		}
	}
}
