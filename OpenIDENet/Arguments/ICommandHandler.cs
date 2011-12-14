using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Arguments
{
	public interface ICommandHandler
	{
		CommandHandlerParameter Usage { get; }
		string Command { get; }
		void Execute(string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation);
	}
}

