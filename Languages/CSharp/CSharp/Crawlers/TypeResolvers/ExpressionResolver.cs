using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Crawlers.TypeResolvers
{
    class ExpressionResolver
    {
        private IOutputWriter _writer;
        private Action<PartialType> _typeResolver;

        public ExpressionResolver(IOutputWriter writer, Action<PartialType> typeResolver) {
            _writer = writer;
            _typeResolver = typeResolver;
        }

        public string Resolve(PartialType type, string typeToMatch)
        {
            if (!typeToMatch.Contains(".") && !typeToMatch.Contains("("))
                return null;
            if (_writer.ContainsType(typeToMatch))
                return typeToMatch;
            var chunks = getChunks(typeToMatch);
            string currentType = null;
            string currentSignature = "";
            var isUsing = false;
            // As we are dealing with members we need to handle member (method/field) and it's
            // parent class/struct...
            var parents = new[] { type.Parent, getParent(type.Parent) };
            Func<string, string> typeFromSignature = _writer.VariableTypeFromSignature;
            foreach (var chunk in chunks)
            {
                // Prepare loop scoped variables to default
                isUsing = false;
                currentSignature = appendToSignature(currentSignature, chunk);

                // Match against scoped member variables local/member
                currentType = matchToInstanceTypes(currentType, parents, typeFromSignature, chunk);

                // Match against namespaces
                if (currentType == null) {
                    currentType = matchToNamespaces(currentSignature);
                    isUsing = true;
                }

                // Reset signature fetcher (only for static a single round)
                typeFromSignature = _writer.VariableTypeFromSignature;

                // Match against static containers (if not namespace it's static)
                if (currentType == null) {
                    currentType = resolveType(type, chunk);
                    if (currentType != null)
                        typeFromSignature = _writer.StaticMemberFromSignature;
                }

                if (currentType == null)
                    return null;

                // Resolve type
                if (!isUsing)
                    currentType = resolveType(type, currentType, currentType);

                // Set resolved type / static class as next parent
                parents = new[] { currentType };
            }
            return currentType;
        }

        private static string[] getChunks(string typeToMatch)
        {
            if (typeToMatch.StartsWith("this."))
                typeToMatch = typeToMatch.Substring("this.".Length, typeToMatch.Length - "this.".Length);
            return typeToMatch
                .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private string resolveType(PartialType type, string unresolvedType)
        {
            return resolveType(type, unresolvedType, null);
        }

        private string resolveType(PartialType type, string unresolvedType, string defaultValue)
        {
            string currentType = defaultValue;
            _typeResolver(
                new PartialType(type.File, type.Location, unresolvedType, type.Parent,
                                (s) => currentType = s));
            return currentType;
        }

        private string matchToNamespaces(string currentSignature)
        {
            if (_writer.Namespaces.Any(x => x.Name == currentSignature))
                return currentSignature;
            return null;
        }

        private string matchToInstanceTypes(string currentType, string[] parents, Func<string, string> typeFromSignature, string chunk)
        {
            foreach (var parent in parents)
            {
                if (parent == null)
                    continue;
                currentType = typeFromSignature(parent + "." + chunk);
                if (currentType == null)
                    currentType = matchToBaseTypes(currentType, typeFromSignature, chunk, parent);
                if (currentType != null)
                    break;
            }
            return currentType;
        }

        private string matchToBaseTypes(string currentType, Func<string, string> typeFromSignature, string chunk, string parent)
        {
            foreach (var baseType in _writer.CollectBases(parent))
            {
                currentType =
                    typeFromSignature(baseType + "." + chunk);
                if (currentType != null)
                    break;
            }
            return currentType;
        }

        private static string appendToSignature(string currentSignature, string chunk)
        {
            if (currentSignature.Length == 0)
                currentSignature = chunk;
            else
                currentSignature += "." + chunk;
            return currentSignature;
        }

        private string getParent(string child) {
            if (child.LastIndexOf('.') != -1)
                return child.Substring(0, child.LastIndexOf('.'));
            return null;
        }
    }
}
