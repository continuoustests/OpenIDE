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
            var usingAliasesMap = getUsingAliasesMap(types);
            foreach (var type in types) {
                if (type.Type.StartsWith("System."))
                    continue;
                var typeToMatch = type.Type.Replace("[]", "");
                var matchingType = matchToAliases(type.File.File, typeToMatch, usingAliasesMap);
                if (matchingType == null) {
                    var usings = getUsings(usingsMap, type);
                    matchingType = getMatchingType(typeToMatch, usings);
                    if (matchingType == null)
                        matchingType = _writer.FirstMatchingTypeFromName(typeToMatch);
                }
                if (matchingType != null)
                    type.Resolve(type.Type.Replace(typeToMatch, matchingType));
                else
                    Console.WriteLine(string.Format("{0} {1}:{2}:{3}", type.Type, type.File.File, type.Location.Line, type.Location.Column));
            }
        }

        private string matchToAliases(string file, string type, Dictionary<string,UsingAlias[]> usingAliasesMap) {
            UsingAlias[] aliases;
            if (!usingAliasesMap.TryGetValue(file, out aliases))
                return null;
            var match = aliases.FirstOrDefault(x => x.Name == type);
            if (match != null) {
                if (_writer.ContainsType(match.Namespace))
                    return match.Namespace;
                var firstMatch = _writer.FirstMatchingTypeFromName(match.Namespace);
                if (firstMatch != null)
                    return firstMatch;
                return match.Namespace;
            }
            return null;
        }

        private List<string> getUsings(Dictionary<string, string[]> usingsMap, PartialType type) {
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
            return list;
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

        private string getMatchingType(string type, IEnumerable<string> usings) {
            foreach (var usng in usings) {
                var signature = usng + "." + type;
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

        private Dictionary<string,UsingAlias[]> getUsingAliasesMap(PartialType[] types) {
            var usingsAliasesMap = new Dictionary<string,UsingAlias[]>();
            types.GroupBy(x => x.File.File).ToList()
                .ForEach(x => 
                    usingsAliasesMap.Add(
                        x.Key, 
                        _writer.UsingAliases
                            .Where(y => y.File.File == x.Key)
                            .Select(y => y).ToArray()));
            return usingsAliasesMap;
        }
    }
}
