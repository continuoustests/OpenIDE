using System;
using OpenIDENet.CodeEngine.Core.Commands;

namespace OpenIDENet.CodeEngine.Core.Handlers
{
	public interface IHandler
	{
		bool Handles(CommandMessage message);
		void Handle(Guid clientID, CommandMessage message);
	}
}
