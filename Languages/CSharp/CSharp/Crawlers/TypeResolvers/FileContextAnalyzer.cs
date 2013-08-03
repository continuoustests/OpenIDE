using System;
using System.Linq;
using System.Collections.Generic;

namespace CSharp.Crawlers.TypeResolvers
{
	public class FileContextAnalyzer
	{
		private IOutputWriter _cache;
        private List<ICodeReference> _references;
        private List<ICodeReference> _referenceContainers;

		public FileContextAnalyzer(IOutputWriter globalCache, IOutputWriter cache)
		{
			_cache = cache;
            buildReferenceMap();
		}

		public string GetEnclosingSignature(string file, int line, int column)
		{
            var match = getParent(file, line, column);

            if (match == null)
                return null;
            
            return match.Signature;
		}

        public string GetSignatureFromNameAndPosition(string file, string name, int line, int column)
        {
            var parent = getParent(file, line, column);
            var match = _references.FirstOrDefault(
                x => 
                    x.Parent == parent.ToNamespaceSignature() && 
                    x.Name == name);
            if (match != null)
                return match.Signature;
            return null;
        }

        private ICodeReference getParent(string file, int line, int column)
        {
            if (_referenceContainers.Count == 0)
                return null;

            var insideOf = _referenceContainers
                    .Where(x => x.Line <= line && x.EndLine >= line)
                    .ToArray();
            if (insideOf.Length == 0)
                return null;

            return _referenceContainers
                .FirstOrDefault(x => x.Line == insideOf.Max(y => y.Line));
        }

        private void buildReferenceMap()
        {
            _referenceContainers = new List<ICodeReference>();
            _references = new List<ICodeReference>();

            _referenceContainers.AddRange(_cache.Namespaces);
            _referenceContainers.AddRange(_cache.Classes);
            _referenceContainers.AddRange(_cache.Interfaces);
            _referenceContainers.AddRange(_cache.Structs);
            _referenceContainers.AddRange(_cache.Enums);
            _referenceContainers.AddRange(_cache.Methods);

            _references.AddRange(_referenceContainers);
            _references.AddRange(_cache.Parameters);
            _references.AddRange(_cache.Fields);
            _references.AddRange(_cache.Variables);
        }
	}
}