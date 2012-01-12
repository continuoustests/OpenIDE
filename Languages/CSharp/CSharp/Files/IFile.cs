using System;
namespace CSharp.Files
{
	public interface IFile
	{
		string Fullpath { get; }
		IFile New(string fullPath);
		bool SupportsExtension(string extension);
	}
}

