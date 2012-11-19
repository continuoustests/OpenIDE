using System;
using System.Text;
using CSharp.Crawlers.TypeResolvers.CodeEngine;
using CSharp.Projects;
using CSharp.Responses;
using OpenIDE.Core.CodeEngineIntegration;

namespace CSharp.Crawlers.TypeResolvers
{
    public class EnclosingSignatureFromPosition
    {
        private IOutputWriter _globalCache;
        private Func<string,string> _fileReader;
        private Action<string> _fileRemover;
        private Func<string,string> _getDirtyFile;

        public EnclosingSignatureFromPosition(
            IOutputWriter globalCache,
            Func<string,string> fileReader,
            Action<string> fileRemover,
            Func<string,string> getDirtyFile) {
            _globalCache = globalCache;
            _fileReader = fileReader;
            _fileRemover = fileRemover;
            _getDirtyFile = getDirtyFile;
        }

        public string GetSignature(string file, int line, int column) {
            var cache = 
                new DirtyFileParser(
                    _globalCache,
                    _fileReader,
                    _fileRemover,
                    _getDirtyFile).Parse(file);
            
            return new FileContextAnalyzer(_globalCache, cache)
                .GetEnclosingSignature(file, line, column);
        }
    }
}
