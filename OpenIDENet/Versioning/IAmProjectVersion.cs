using System;
namespace OpenIDENet.Versioning
{
	public interface IAmProjectVersion
	{
		bool IsValid(string projecFile);
	}
}

