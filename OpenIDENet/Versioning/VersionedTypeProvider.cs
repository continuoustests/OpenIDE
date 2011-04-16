using System;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects.Writers;
using OpenIDENet.Bootstrapping;
using OpenIDENet.Files;
using OpenIDENet.Projects.Removers;
namespace OpenIDENet.Versioning
{
	public class VersionedTypeProvider<T> : IProvideVersionedTypes where T : IAmProjectVersion
	{
		private IReadProjectsFor[] _readers;
		private IResolveFileTypes[] _fileTypeResolvers;
		private IAppendFiles[] _compiledFileAppenders;
		private IRemoveFiles[] _compiledFileRemovers;
		private IWriteProjectFileToDiskFor[] _writers;
		
		public VersionedTypeProvider(IReadProjectsFor[] readers, IResolveFileTypes[] fileTypeResolvers, IAppendFiles[] compiledFileAppenders, IRemoveFiles[] compiledFileRemovers, IWriteProjectFileToDiskFor[] writers)
		{
			_readers = readers;
			_fileTypeResolvers = fileTypeResolvers;
			_compiledFileAppenders = compiledFileAppenders;
			_compiledFileRemovers = compiledFileRemovers;
			_writers = writers;
		}
		
		public IReadProjectsFor Reader()
		{
			foreach (var reader in _readers)
			{
				if (reader.SupportsProject<T>())
					return reader;
			}
			return null;
		}
		
		public IResolveFileTypes FileTypeResolver()
		{
			foreach (var resolver in _fileTypeResolvers)
			{
				if (resolver.SupportsProject<T>())
					return resolver;
			}
			return null;
		}

		public IAppendFiles FileAppenderFor(IFile file)
		{
			foreach (var appender in _compiledFileAppenders)
			{
				if (appender.SupportsProject<T>() && appender.SupportsFile(file))
					return appender;
			}
			return null;
		}
		
		public IRemoveFiles FileRemoverFor(IFile file)
		{
			foreach (var remover in _compiledFileRemovers)
			{
				if (remover.SupportsProject<T>() && remover.SupportsFile(file))
					return remover;
			}
			return null;
		}

		public IWriteProjectFileToDiskFor Writer()
		{
			foreach (var writer in _writers)
			{
				if (writer.SupportsProject<T>())
					return writer;
			}
			return null;
		}
	}
}

