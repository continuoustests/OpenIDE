using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Projects.Parsers
{
	public interface IParseProject<T> where T : IAmProjectVersion
	{
		IProject Parse(string xml);
	}
}