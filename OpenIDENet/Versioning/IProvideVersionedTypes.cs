using System;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects.Writers;
namespace OpenIDENet.Versioning
{
	public interface IProvideVersionedTypes
	{
		IReadProjectsFor Reader();
		IAppendCompiledFilesFor CompiledFileAppender();
		IWriteProjectFileToDiskFor Writer();
	}
}