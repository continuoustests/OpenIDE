using System;
using CSharp.Crawlers.TypeResolvers;
using CSharp.Projects;
namespace CSharp.Crawlers
{
	public interface ICodeReference
	{
        bool AllTypesAreResolved { get; }

		string Type { get; }
		FileRef File { get; }
        string Namespace { get; }
		string Signature { get; }
		string Name { get; }
        string Scope { get; }
		int Line { get; }
		int Column { get; }
        string JSON { get; }

        string GenerateFullSignature();

        void ResolveTypes(ICacheReader cache);
	}
}

