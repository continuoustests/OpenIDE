using System;
namespace CSharp.Crawlers
{
	public interface ICodeReference
	{
		string Type { get; }
		string File { get; }
		string Signature { get; }
		string Name { get; }
		int Offset { get; }
		int Line { get; }
		int Column { get; }
	}
}

