using System;
namespace OpenIDENet.CodeEngine.Core
{
	public interface ICodeType
	{
		string Fullpath { get; }
		string Signature { get; }
		string Name { get; }
		int Offset { get; }
		int Line { get; }
		int Column { get; }
	}
}

