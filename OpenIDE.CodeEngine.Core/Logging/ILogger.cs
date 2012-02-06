using System;

namespace OpenIDE.CodeEngine.Core.Logging
{
	public interface ILogger
	{
		void Write(string message);
		void Write(string message, params object[] args);
		void Write(Exception ex);
	}
}
