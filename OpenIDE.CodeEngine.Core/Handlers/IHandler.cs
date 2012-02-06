using System;
using OpenIDE.CodeEngine.Core.Commands;

namespace OpenIDE.CodeEngine.Core.Handlers
{
	public interface IHandler
	{
		bool Handles(CommandMessage message);
		void Handle(Guid clientID, CommandMessage message);
	}
}
