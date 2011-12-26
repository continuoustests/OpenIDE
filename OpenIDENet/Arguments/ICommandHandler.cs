using System;
namespace OpenIDENet.Arguments
{
	public interface ICommandHandler
	{
		CommandHandlerParameter Usage { get; }
		string Command { get; }
		void Execute(string[] arguments);
	}
}

