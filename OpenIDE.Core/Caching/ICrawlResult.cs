using System.Collections.Generic;

namespace OpenIDE.Core.Caching
{
	public interface ICrawlResult
	{
		void Add(Project project);
		void Add(ProjectFile file);
		void Add(ICodeReference reference);
		void Add(IEnumerable<ICodeReference> references);
		void Add(ISignatureReference reference);
	}
}
