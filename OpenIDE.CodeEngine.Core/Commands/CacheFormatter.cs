using System;
using OpenIDE.Core.Caching;
using OpenIDE.CodeEngine.Core.Caching;

namespace OpenIDE.CodeEngine.Core.Commands
{
	class CacheFormatter
	{
		public string Format(Project project)
		{
			return string.Format("project|{0}|{1}",
				project.File, project.JSON);
		}

		public string FormatProject(string file)
		{
			return string.Format("project|{0}", file);
		}

		public string Format(ProjectFile file)
		{
			return string.Format("file|{0}",
				file.File);
		}

		public string FormatFile(string file)
		{
			return string.Format("file|{0}", file);
		}

		public string Format(ICodeReference reference)
		{
			return string.Format("{0}|signature|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                reference.Language,
				reference.Signature,
				reference.Name,
				reference.Type,
                reference.Scope,
				reference.Line,
				reference.Column,
                reference.JSON);
		}
		
		public string Format(ISignatureReference reference)
		{
			return string.Format("reference|{0}|{1}|{2}",
				reference.Signature,
				reference.Line,
				reference.Column);
		}
	}
}
