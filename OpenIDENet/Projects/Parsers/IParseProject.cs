using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Projects.Parsers
{
	public interface IParseProject<T> where T : IAmVisualStudioVersion
	{
		IProject Parse(string xml);
	}
}