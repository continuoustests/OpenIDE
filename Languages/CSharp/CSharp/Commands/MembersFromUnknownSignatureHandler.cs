using System;
using CSharp.Responses;

namespace CSharp.Commands
{
	class MembersFromUnknownSignatureHandler : ICommandHandler
	{
		public string Usage { get { return null; } }

		public string Command { get { return "members-from-signature"; } }
		
		public void Execute(IResponseWriter writer, string[] args)
		{
			// TODO when the code parsing is done implement this
		}
	}
}
