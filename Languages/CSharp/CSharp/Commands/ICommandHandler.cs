using System;
using CSharp.Responses;

namespace CSharp.Commands
{
	public interface ICommandHandler
	{
		string Usage { get; }
		string Command { get; }
		void Execute(IResponseWriter writer, string[] arguments);
	}
}

