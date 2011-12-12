using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Arguments
{
	interface ICommandHandler
	{
		CommandHandlerParameters Usage { get; }
		string Command { get; }
		void Execute(string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation);
	}
}

