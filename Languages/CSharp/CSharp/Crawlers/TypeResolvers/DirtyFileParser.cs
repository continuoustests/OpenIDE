using System;
using CSharp.Projects;
using CSharp.Responses;

namespace CSharp.Crawlers.TypeResolvers
{
	class DirtyFileParser
	{
		private IOutputWriter _globalCache;
        private Func<string,string> _fileReader;
        private Action<string> _fileRemover;
        private Func<string,string> _getDirtyFile;
        private bool _parseLocalVariables = false;

		public DirtyFileParser(
			IOutputWriter globalCache,
			Func<string,string> fileReader,
            Action<string> fileRemover,
            Func<string,string> getDirtyFile)
		{
			_globalCache = globalCache;
            _fileReader = fileReader;
            _fileRemover = fileRemover;
            _getDirtyFile = getDirtyFile;
		}

		public DirtyFileParser ParseLocalVariables()
		{
			_parseLocalVariables = true;
			return this;
		}

		public OutputWriter Parse(string file)
		{
			var dirtyFile = _getDirtyFile(file);
            var usingDirtyFile = false;
            if (dirtyFile != null) {
                dirtyFile = parseDirtyFile(dirtyFile);
                if (dirtyFile.Trim() != "") {
                    usingDirtyFile = true;
                    file = dirtyFile.Trim();
                }
            }

            var parser = new NRefactoryParser();
            if (_parseLocalVariables)
            	parser.ParseLocalVariables();
            var cache = new OutputWriter(new NullResponseWriter());
            parser.SetOutputWriter(cache);
            var fileRef = new FileRef(file, null);
            parser.ParseFile(fileRef, () => _fileReader(file));
            if (usingDirtyFile)
                _fileRemover(file);

            cache.BuildTypeIndex();
            new TypeResolver(new OutputWriterCacheReader(cache, _globalCache))
                .ResolveAllUnresolved(cache);
            return cache;
		}

		private string parseDirtyFile(string dirtyFile) {
            try {
                return dirtyFile.Replace(Environment.NewLine, "").Split(new[] { '|' })[1];
            } catch {
                return "";
            }
        }
	}
}