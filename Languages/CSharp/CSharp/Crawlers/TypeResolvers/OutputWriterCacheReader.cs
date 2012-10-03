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

        private HashSet<string> _usedTypes = new HashSet<string>();
        public void ResolveMatchingType(params PartialType[] types) {
            var usingsMap = getUsingsMap(types);
            foreach (var type in types) {
                if (type.Type == "")
                    continue;
                if (type.Type.StartsWith("System."))
                    continue;
                string matchingType = null;
                var time = DateTime.Now.Ticks;
                var before = DateTime.Now.Ticks - time;
                string[] usings = getUsings(usingsMap, type);
                foreach (var usng in usings) {
                    var signature = usng + "." + type.Type;
                    if (_usedTypes.Contains(signature))
                        matchingType = signature;
                }
                var afterUsings = DateTime.Now.Ticks - time;
                if (matchingType == null) {
                    matchingType = getMatchingType(type, usings);
                    var afterMatch = DateTime.Now.Ticks - time;
                }
                if (matchingType != null) {
                    type.Resolve(matchingType);
                    _usedTypes.Add(matchingType);
                } else {
                    Console.WriteLine(type.Type);
                }
                var span = DateTime.Now.Ticks - time;
                //Console.WriteLine(span.ToString().PadLeft(15, ' ') + " " + type.File.File + ": " + type.Type);
            }
        }

        private string[] getUsings(Dictionary<string, string[]> usingsMap, PartialType type) {
            string[] usings;
            if (!usingsMap.TryGetValue(type.File.File, out usings))
                usings = new string[] { };
            var list = new List<string>();
            var chunks = type.Namespace.Split(new[] { '.' });
            var currentNS = "";
            foreach (var chunk in chunks) {
                if (currentNS != "")
                    currentNS += ".";
                currentNS += chunk;
                list.Add(currentNS);
            }
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
            foreach (var usng in usings) {
                var signature = usng + "." + type.Type;
                if (_writer.ContainsType(signature))
                    return signature;
            }
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
    }
}
