using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Crawlers.TypeResolvers.CodeEngine;
using CSharp.Projects;
using CSharp.Responses;
using OpenIDE.Core.CodeEngineIntegration;

namespace CSharp.Crawlers.TypeResolvers
{
    public class EnclosingSignatureFromPosition
    {
        private Func<string,string> _fileReader;
        private Action<string> _fileRemover;
        private Func<string,string> _getDirtyFile;

        public EnclosingSignatureFromPosition(
            Func<string,string> fileReader,
            Action<string> fileRemover,
            Func<string,string> getDirtyFile) {
            _fileReader = fileReader;
            _fileRemover = fileRemover;
            _getDirtyFile = getDirtyFile;
        }

        public string GetSignature(string file, int line, int column) {
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
            var cache = new OutputWriter(new NullResponseWriter());
            parser.SetOutputWriter(cache);
            var fileRef = new FileRef(file, null);
            parser.ParseFile(fileRef, () => _fileReader(file));
            if (usingDirtyFile)
                _fileRemover(file);

            cache.BuildTypeIndex();
            new TypeResolver(new OutputWriterCacheReader(cache))
                .ResolveAllUnresolved(cache);
            
            var references = new List<ICodeReference>();
            references.AddRange(cache.Namespaces);
            references.AddRange(cache.Classes);
            references.AddRange(cache.Interfaces);
            references.AddRange(cache.Structs);
            references.AddRange(cache.Enums);
            references.AddRange(cache.Methods);

            if (references.Count == 0)
                return null;

            var insideOf = references
                    .Where(x => x.Line <= line && x.EndLine >= line);
            if (insideOf.Count() == 0)
                return null;

            var match = references
                .FirstOrDefault(x => x.Line == insideOf.Max(y => y.Line));

            if (match == null)
                return null;

            return match.GenerateFullSignature();
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
