using System;
using System.Collections.Generic;
using CSharp.Crawlers.TypeResolvers;
using CSharp.Projects;
namespace CSharp.Crawlers
{
    public interface ISourceItem
    {
        long ID { get; }
        string Signature { get; }
        void SetID(long id);
    }

	public interface ICodeReference : ISourceItem
	{
        bool AllTypesAreResolved { get; set; }

		string Type { get; }
		FileRef File { get; }
        string Parent { get; }
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

