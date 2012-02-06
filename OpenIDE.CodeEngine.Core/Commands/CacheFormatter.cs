using System;
using OpenIDE.CodeEngine.Core.Caching;

namespace OpenIDE.CodeEngine.Core.Commands
{
	class CacheFormatter
	{
		public string Format(Project project)
		{
			return string.Format("project|{0}",
				project.File);
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
			return string.Format("signature|{0}|{1}|{2}|{3}|{4}|{5}",
				reference.Signature,
				reference.Name,
				reference.Type,
				reference.Offset,
				reference.Line,
				reference.Column);
		}
		
		public string Format(ISignatureReference reference)
		{
			return string.Format("reference|{0}|{1}|{2}|{3}",
				reference.Signature,
				reference.Offset,
				reference.Line,
				reference.Column);
		}
	}
}
