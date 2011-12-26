using System;
using CSharp.Projects;
using CSharp.Projects.Readers;
using CSharp.Projects.Appenders;
using CSharp.Projects.Writers;
using CSharp.Files;
using CSharp.Projects.Removers;

namespace CSharp.Versioning
{
	public class VersionedTypeProvider<T> : IProvideVersionedTypes where T : IAmProjectVersion
	{
		private IReadProjectsFor[] _readers;
		private IResolveFileTypes[] _fileTypeResolvers;
		private IAppendFiles[] _compiledFileAppenders;
		private IRemoveFiles[] _compiledFileRemovers;
		private IWriteProjectFileToDiskFor[] _writers;
		private IAddReference[] _referenceHandlers;
		private IRemoveReference[] _dereferenceHandlers;
		
		public VersionedTypeProvider(
			IReadProjectsFor[] readers,
			IResolveFileTypes[] fileTypeResolvers,
			IAppendFiles[] compiledFileAppenders,
			IRemoveFiles[] compiledFileRemovers,
			IWriteProjectFileToDiskFor[] writers,
			IAddReference[] referencehandlers,
			IRemoveReference[] dereferencehandlers)
		{
			_readers = readers;
			_fileTypeResolvers = fileTypeResolvers;
			_compiledFileAppenders = compiledFileAppenders;
			_compiledFileRemovers = compiledFileRemovers;
			_writers = writers;
			_referenceHandlers = referencehandlers;
			_dereferenceHandlers = dereferencehandlers;
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
		
		public IAddReference ReferencerFor(IFile file)
		{
			foreach (var appender in _referenceHandlers)
			{
				if (appender.SupportsProject<T>() && appender.SupportsFile(file))
					return appender;
			}
			return null;
		}
		
		public IRemoveReference DereferencerFor(IFile file)
		{
			foreach (var dereferencer in _dereferenceHandlers)
			{
				if (dereferencer.SupportsProject<T>() && dereferencer.SupportsFile(file))
					return dereferencer;
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

