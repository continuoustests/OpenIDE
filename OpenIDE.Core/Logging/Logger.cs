using System;

namespace OpenIDE.Core.Logging
{
	public static class Logger
	{
		private static ILogger _logger = null;

		public static void Assign(ILogger logger)
		{
			_logger = logger;
		}
		
		public static void Write(string message)
		{
			if (!isEnabled()) return;
			_logger.Write(message);
		}

		public static void Write(string message, params object[] args)
		{
			if (!isEnabled()) return;
			_logger.Write(message, args);
		}

		public static void Write(Exception ex)
		{
			if (!isEnabled()) return;
			_logger.Write(ex);
		}

		private static bool isEnabled()
		{
			return _logger != null;
		}
	}

	public class ConsoleLogger : ILogger
	{
		public void Write(string message) {
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		public void Write(string message, params object[] args) {
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine(message, args);
			Console.ResetColor();
		}

		public void Write(Exception ex) {
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine(ex.ToString());
			Console.ResetColor();
		}
	}
}
