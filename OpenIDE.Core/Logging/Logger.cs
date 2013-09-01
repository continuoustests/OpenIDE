using System;

namespace OpenIDE.Core.Logging
{
	public static class Logger
	{
		private static ILogger _logger = null;

		public static bool IsEnabled { get { return isEnabled(); } }

		public static void Assign(ILogger logger)
		{
			_logger = logger;
			Write("Assigned logger");
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
		private object _padlock = new object();
		private long _lastLogItem = 0;
		private long _start = 0;

		public void Write(string message) {
			write(message);
		}

		public void Write(string message, params object[] args) {
			write(string.Format(message, args));
		}

		public void Write(Exception ex) {
			write(ex.ToString());
		}

		private void write(string message) {
			lock (_padlock) {
				if (_start == 0) {
					_start = DateTime.Now.Ticks;
					_lastLogItem = _start;
				}
				var fromLast = DateTime.Now.Ticks - _lastLogItem;
				_lastLogItem = _lastLogItem + fromLast;
				var total = DateTime.Now.Ticks - _start;
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine("{0},{1} - {2}", Math.Round((new TimeSpan(total)).TotalMilliseconds, 0), Math.Round((new TimeSpan(fromLast)).TotalMilliseconds), message);
				Console.ResetColor();
			}
		}
	}
}
