using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Arguments
{
	public class ProviderSettings
	{
		public string ProjectFile { get; private set; }
		public IProvideVersionedTypes TypesProvider { get; private set; }
		
		public ProviderSettings(string projectFile, IProvideVersionedTypes provider)
		{
			ProjectFile = projectFile;
			TypesProvider = provider;
		}
	}
}

