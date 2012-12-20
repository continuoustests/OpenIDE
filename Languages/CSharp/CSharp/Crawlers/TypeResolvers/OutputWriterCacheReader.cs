using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Crawlers.TypeResolvers.CodeEngine;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.FileSystem;

namespace CSharp.Crawlers.TypeResolvers
{
    public class OutputWriterCacheReader : ICacheReader
    {
        private IOutputWriter _localCache;
        private IOutputWriter _globalCache;
        private ExpressionResolver _expression;

        public OutputWriterCacheReader(IOutputWriter localCache, IOutputWriter globalCache) {
            _localCache = localCache;
            _globalCache = globalCache;
            _expression = new ExpressionResolver(_localCache, ResolveMatchingType);
        }

        public void ResolveMatchingType(PartialType type) {
            ResolveMatchingType(new[] { type });
        }

        public void ResolveMatchingType(params PartialType[] types) {
            var usingsMap = getUsingsMap(_localCache, types);
            var usingAliasesMap = getUsingAliasesMap(_localCache, types);
            foreach (var type in types) {
                if (!resolveTypeFromCache(_localCache, usingsMap, usingAliasesMap, type))
                    resolveTypeFromCache(_globalCache, usingsMap, usingAliasesMap, type);
            }
        }

        private bool resolveTypeFromCache(IOutputWriter cache, Dictionary<string, string[]> usingsMap, Dictionary<string, UsingAlias[]> usingAliasesMap, PartialType type) {
            // Cannot do this since expressions might start with System.
            //if (type.Type.StartsWith("System."))
            //    continue;
            var typeToMatch = type.Type.Replace("[]", "");
            var matchingType = _expression.Resolve(type, typeToMatch);
            if (matchingType == null)
                matchingType = cache.VariableTypeFromSignature(type.Parent + "." + type.Type);
            var usings = getUsings(usingsMap, type);
            if (matchingType == null)
            {
                matchingType = matchToAliases(cache, type.File.File, typeToMatch, usingAliasesMap);
            }
            if (matchingType == null)
            {
                matchingType = getMatchingType(cache, typeToMatch, usings);
                if (matchingType == null)
                    matchingType = cache.FirstMatchingTypeFromName(typeToMatch);
            }
            if (matchingType != null)
                type.Resolve(type.Type.Replace(typeToMatch, matchingType));
            return matchingType != null;
        }

        private string matchToAliases(IOutputWriter cache, string file, string type, Dictionary<string,UsingAlias[]> usingAliasesMap) {
            UsingAlias[] aliases;
            if (!usingAliasesMap.TryGetValue(file, out aliases))
                return null;
            var match = aliases.FirstOrDefault(x => x.Name == type);
            if (match != null) {
                if (cache.ContainsType(match.Parent))
                    return match.Parent;
                var firstMatch = cache.FirstMatchingTypeFromName(match.Parent);
                if (firstMatch != null)
                    return firstMatch;
                return match.Parent;
            }
            return null;
        }

        private List<string> getUsings(Dictionary<string, string[]> usingsMap, PartialType type) {
            string[] usings;
            if (!usingsMap.TryGetValue(type.File.File, out usings))
                usings = new string[] { };
            var list = new List<string>();
            var chunks = type.Parent.Split(new[] { '.' });
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

        private string getNamespaceFromPoint(IOutputWriter cache, PartialType type) {
            var currentNamespace = cache.Namespaces
                .FirstOrDefault(x => 
                    x.File.File == type.File.File &&
                    x.Line <= type.Location.Line);
            if (currentNamespace != null)
                return currentNamespace.Name;
            return null;
        }

        private string getMatchingType(IOutputWriter cache, string type, IEnumerable<string> usings) {
            foreach (var usng in usings) {
                var signature = usng + "." + type;
                if (cache.ContainsType(signature))
                    return signature;
            }
            return null;
        }

        private Dictionary<string,string[]> getUsingsMap(IOutputWriter cache, PartialType[] types) {
            var usingsMap = new Dictionary<string,string[]>();
            types.GroupBy(x => x.File.File).ToList()
                .ForEach(x => 
                    usingsMap.Add(
                        x.Key,
                        cache.Usings
                            .Where(y => y.File.File == x.Key)
                            .Select(y => y.Name).ToArray()));
            return usingsMap;
        }

        private Dictionary<string,UsingAlias[]> getUsingAliasesMap(IOutputWriter cache, PartialType[] types) {
            var usingsAliasesMap = new Dictionary<string,UsingAlias[]>();
            types.GroupBy(x => x.File.File).ToList()
                .ForEach(x => 
                    usingsAliasesMap.Add(
                        x.Key,
                        cache.UsingAliases
                            .Where(y => y.File.File == x.Key)
                            .Select(y => y).ToArray()));
            return usingsAliasesMap;
        }
    }
}
