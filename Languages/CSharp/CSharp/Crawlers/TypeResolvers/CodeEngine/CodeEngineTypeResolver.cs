using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.FileSystem;

namespace CSharp.Crawlers.TypeResolvers.CodeEngine
{
    public class CodeEngineTypeResolver
    {
        private Func<ICodeEngineInstance> _codeModelFactory;
        private ICodeEngineInstance _codeModel;

        public CodeEngineTypeResolver(Func<ICodeEngineInstance> codeModelFactory) {
            _codeModelFactory = codeModelFactory;
        }

        public string MatchTypeName(string typeName, IEnumerable<string> usings) {
            if (_codeModel == null) {
                _codeModel = _codeModelFactory();
                if (_codeModel == null)
                    return null;
                _codeModel.KeepAlive();
            }
            var refs = new CodeEngineResultParser()
                .ParseRefs( _codeModel.GetCodeRefs("language=C#,name=" + typeName));
            if (refs.Count == 0)
                return null;
            foreach (var usng in usings) {
                var match = refs.FirstOrDefault(x => x.Parent + "." + x.Name == usng + "." + typeName);
                if (match != null)
                    return match.Parent + "." + match.Name;
            }
            return null;
        }
    }
}
