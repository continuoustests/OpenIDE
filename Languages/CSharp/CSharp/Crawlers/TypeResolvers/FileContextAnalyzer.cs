using System;
using System.Linq;
using System.Collections.Generic;

namespace CSharp.Crawlers.TypeResolvers
{
	public class FileContextAnalyzer
	{
		private IOutputWriter _globalCache;
		private IOutputWriter _cache;

		public FileContextAnalyzer(IOutputWriter globalCache, IOutputWriter cache)
		{
			_globalCache = globalCache;
			_cache = cache;
		}

		public string GetEnclosingSignature(string file, int line, int column)
		{
			var references = buildReferenceMap();

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

        public string GetSignatureFromTypeAndPosition(string file, string type, int line, int column)
        {
            var parent = GetEnclosingSignature(file, line, column);
            return parent + "." + type;
        }

        private List<ICodeReference> buildReferenceMap()
        {
            var references = new List<ICodeReference>();
            references.AddRange(_cache.Namespaces);
            references.AddRange(_cache.Classes);
            references.AddRange(_cache.Interfaces);
            references.AddRange(_cache.Structs);
            references.AddRange(_cache.Enums);
            references.AddRange(_cache.Methods);
            return references;
        }
	}
}