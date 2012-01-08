using System;

namespace CSharp.Commands
{
	public interface ICommandHandler
	{
		string Usage { get; }
		string Command { get; }
		void Execute(string[] arguments);
	}
}

