using System;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments
{
	public interface ICommandHandler
	{
		CommandHandlerParameter Usage { get; }
		string Command { get; }
		void Execute(string[] arguments);
	}
}

