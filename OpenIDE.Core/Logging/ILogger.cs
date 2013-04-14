using System;

namespace OpenIDE.Core.Logging
{
	public interface ILogger
	{
		void Write(string message);
		void Write(string message, params object[] args);
		void Write(Exception ex);
	}
}
