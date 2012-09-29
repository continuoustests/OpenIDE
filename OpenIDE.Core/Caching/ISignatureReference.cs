using System;

namespace OpenIDE.Core.Caching
{
	public interface ISignatureReference
	{
		string File { get; }
		string Signature { get; }
		int Line { get; }
		int Column { get; }
	}
	
	public class SignatureReference : ISignatureReference
	{
		public string File { get; private set; }
		public string Signature { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }

		public SignatureReference(
			string file,
			string signature,
			int line,
			int column)
		{
			File = file;
			Signature = signature;
			Line = line;
			Column = column;
		}
	}
}
