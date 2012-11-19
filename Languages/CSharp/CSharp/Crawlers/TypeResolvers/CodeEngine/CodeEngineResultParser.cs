using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Projects;

namespace CSharp.Crawlers.TypeResolvers.CodeEngine
{
    public class CodeEngineResultParser
    {
        private FileRef _currentFile = null;

        public List<ICodeReference> ParseRefs(string resultString)
        {
            var refs = new List<ICodeReference>();
            foreach (var command in resultString.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
                try {
				    var chunks = command.Trim()
					    .Split(new char[] { '|' }, StringSplitOptions.None);
				    if (chunks.Length == 0)
					    continue;
				    if (chunks[0] == "file")
					    _currentFile = handleFile(chunks);
				    if (chunks[1] == "signature") {
                        var reference = handleSignature(chunks);
                        if (reference != null)
					        refs.Add(reference);
                    }
			    } catch {
			    }
            }
            return refs;
        }

        private FileRef handleFile(string[] chunks)
		{
			return new FileRef(chunks[1], null);
		}
		
		private ICodeReference handleSignature(string[] chunks)
		{
            if (chunks[5] == "class")
                return new Class(_currentFile, chunks[2], chunks[4], chunks[6], int.Parse(chunks[7]), int.Parse(chunks[8]));
            if (chunks[5] == "interface")
                return new Interface(_currentFile, chunks[2], chunks[4], chunks[6], int.Parse(chunks[7]), int.Parse(chunks[8]));
            if (chunks[5] == "struct")
                return new Struct(_currentFile, chunks[2], chunks[4], chunks[6], int.Parse(chunks[7]), int.Parse(chunks[8]));
            if (chunks[5] == "enum")
                return new EnumType(_currentFile, chunks[2], chunks[4], chunks[6], int.Parse(chunks[7]), int.Parse(chunks[8]));
			return null;
		}
    }
}