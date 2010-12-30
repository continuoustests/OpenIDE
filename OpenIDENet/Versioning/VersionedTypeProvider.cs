using System;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects.Writers;
using OpenIDENet.Bootstrapping;
namespace OpenIDENet.Versioning
{
	public class VersionedTypeProvider<T> : IProvideVersionedTypes where T : IAmProjectVersion
	{
		private IReadProjectsFor[] _readers;
		private IAppendCompiledFilesFor[] _compiledFileAppenders;
		private IWriteProjectFileToDiskFor[] _writers;
		
		public VersionedTypeProvider(IReadProjectsFor[] readers, IAppendCompiledFilesFor[] compiledFileAppenders, IWriteProjectFileToDiskFor[] writers)
		{
			_readers = readers;
			_compiledFileAppenders = compiledFileAppenders;
			_writers = writers;
		}
		
		public IReadProjectsFor Reader()
		{
			foreach (var reader in _readers)
			{
				if (reader.SupportsVersion<T>())
					return reader;
			}
			return null;
		}

		public IAppendCompiledFilesFor CompiledFileAppender()
		{
			foreach (var appender in _compiledFileAppenders)
			{
				if (appender.SupportsVersion<T>())
					return appender;
			}
			return null;
		}

		public IWriteProjectFileToDiskFor Writer()
		{
			foreach (var writer in _writers)
			{
				if (writer.SupportsVersion<T>())
					return writer;
			}
			return null;
		}
	}
}

