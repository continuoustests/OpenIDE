using System;
using OpenIDENet.Projects;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects.Writers;
using OpenIDENet.Files;
using OpenIDENet.Projects.Removers;
namespace OpenIDENet.Versioning
{
	public interface IProvideVersionedTypes
	{
		IReadProjectsFor Reader();
		IResolveFileTypes FileTypeResolver();
		IAppendFiles FileAppenderFor(IFile file);
		IRemoveFiles FileRemoverFor(IFile file);
		IWriteProjectFileToDiskFor Writer();
		IAddReference ReferencerFor(IFile file);
	}
}
