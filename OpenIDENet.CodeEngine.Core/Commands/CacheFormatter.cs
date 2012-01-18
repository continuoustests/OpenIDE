using System;

namespace OpenIDENet.CodeEngine.Core.Commands
{
	class CacheFormatter
	{
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

		public string FormatFile(string file)
		{
			return string.Format("file|{0}", file);
		}
	}
}
