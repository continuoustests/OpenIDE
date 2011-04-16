using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Arguments
{
	interface ICommandHandler
	{
		string Command { get; }
		void Execute(string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation);
	}
}

