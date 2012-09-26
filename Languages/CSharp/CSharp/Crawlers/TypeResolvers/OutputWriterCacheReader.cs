using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Crawlers.TypeResolvers
{
    public class OutputWriterCacheReader : ICacheReader
    {
        private IOutputWriter _writer;

        public OutputWriterCacheReader(IOutputWriter writer) {
            _writer = writer;
        }

        public void ResolveMatchingType(params PartialType[] types) {
            var usingsMap = getUsingsMap(types);
            foreach (var type in types) {
                string[] usings = getUsings(usingsMap, type);
                var matchingType = getMatchingType(type, usings);
                if (matchingType != null)
                    type.Resolve(matchingType);
            }
        }

        private string[] getUsings(Dictionary<string, string[]> usingsMap, PartialType type) {
            string[] usings;
            if (!usingsMap.TryGetValue(type.File.File, out usings))
                usings = new string[] { };
            var list = new List<string>();
            var currentNamespace = getNamespaceFromPoint(type);
            if (currentNamespace != null)
                list.Add(currentNamespace);
            list.AddRange(usings);
            return list.ToArray();
        }

        private string getNamespaceFromPoint(PartialType type) {
            var currentNamespace = _writer.Namespaces
                .FirstOrDefault(x => 
                    x.File.File == type.File.File &&
                    x.Line <= type.Location.Line);
            if (currentNamespace != null)
                return currentNamespace.Name;
            return null;
        }

        private string getMatchingType(PartialType type, string[] usings) {
            var match = matchIn(_writer.Enums, type, usings);
            if (match != null)
                return match;
            match = matchIn(_writer.Structs, type, usings);
            if (match != null)
                return match;
            match = matchIn(_writer.Interfaces, type, usings);
            if (match != null)
                return match;
            match = matchIn(_writer.Classes, type, usings);
            if (match != null)
                return match;
            return null;
        }

        private string matchIn(IEnumerable<ICodeReference> list, PartialType type, string[] usings)
        {
            var item = list
                .FirstOrDefault(x => 
                    x.Name == type.Type && 
                    matchToAvailableNamespaces(usings, x));
            if (item != null)
                return item.Signature;
            return null;
        }

        private Dictionary<string,string[]> getUsingsMap(PartialType[] types) {
            var usingsMap = new Dictionary<string,string[]>();
            types.GroupBy(x => x.File.File).ToList()
                .ForEach(x => 
                    usingsMap.Add(
                        x.Key, 
                        _writer.Usings
                            .Where(y => y.File.File == x.Key)
                            .Select(y => y.Name).ToArray()));
            return usingsMap;
        }

        private bool matchToAvailableNamespaces(string[] usings, ICodeReference reference) {
            return usings.Contains(reference.Namespace);
        }
    }
}
