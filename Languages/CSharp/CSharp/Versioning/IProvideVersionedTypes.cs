using System;
using CSharp.Projects;
using CSharp.Projects.Readers;
using CSharp.Projects.Appenders;
using CSharp.Projects.Writers;
using CSharp.Files;
using CSharp.Projects.Removers;

namespace CSharp.Versioning
{
	public interface IProvideVersionedTypes
	{
		IReadProjectsFor Reader();
		IResolveFileTypes FileTypeResolver();
		IAppendFiles FileAppenderFor(IFile file);
		IRemoveFiles FileRemoverFor(IFile file);
		IWriteProjectFileToDiskFor Writer();
		IAddReference ReferencerFor(IFile file);
		IRemoveReference DereferencerFor(IFile file);
	}
}
