using System;
namespace CSharp.Versioning
{
	public interface IAmProjectVersion
	{
		bool IsValid(string projecFile);
	}
}

