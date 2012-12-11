using System;
using System.Collections.Generic;
using CSharp.Crawlers.TypeResolvers;
using CSharp.Projects;
namespace CSharp.Crawlers
{
	public interface ICodeReference
	{
        bool AllTypesAreResolved { get; set; }

		string Type { get; }
		FileRef File { get; }
        string Namespace { get; }
		string Signature { get; }
		string Name { get; }
        string Scope { get; }
		int Line { get; }
		int Column { get; }
        int EndLine { get; }
        int EndColumn { get; }
        string JSON { get; }
        
        string ToNamespaceSignature();

        IEnumerable<ResolveStatement> GetResolveStatements();
	}
}

