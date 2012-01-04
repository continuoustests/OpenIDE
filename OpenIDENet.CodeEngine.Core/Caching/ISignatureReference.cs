using System;

namespace OpenIDENet.CodeEngine.Core.Caching
{
	public interface ISignatureReference
	{
		string File { get; }
		string Signature { get; }
		int Offset { get; }
		int Line { get; }
		int Column { get; }
	}
	
	public class SignatureReference : ISignatureReference
	{
		public string File { get; private set; }
		public string Signature { get; private set; }
		public int Offset { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }

		public SignatureReference(
			string file,
			string signature,
			int offset,
			int line,
			int column)
		{
			File = file;
			Signature = signature;
			Offset = offset;
			Line = line;
			Column = column;
		}
	}
}
