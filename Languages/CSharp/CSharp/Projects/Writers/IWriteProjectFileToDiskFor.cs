using System;
using CSharp.Versioning;

namespace CSharp.Projects.Writers
{
	public interface IWriteProjectFileToDiskFor
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		void Write(Project project);
	}
}

