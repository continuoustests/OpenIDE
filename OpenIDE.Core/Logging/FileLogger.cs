using System;
using System.IO;
using System.Reflection;

namespace OpenIDE.Core.Logging
{
	public class FileLogger : ILogger
	{
		private string _file;
		private object _padlock = new object();
		private long _lastLogItem = 0;
		private long _start = 0;

		public FileLogger(string filePath)
		{
			_file = filePath;
		}
		public void Write(string message)
		{
			write(message);
		}

		public void Write(string message, params object[] args)
		{
			write(string.Format(message, args));
		}

		public void Write(Exception ex)
		{
			write(getException(ex));	
		}

		private string getException(Exception ex)
		{
			var message = ex.ToString();
			if (ex.InnerException != null)
				message += Environment.NewLine + getException(ex.InnerException);
			return message;
		}

		private void write(string message)
		{
			lock (_padlock) {
				try {
					if (_start == 0) {
						_start = DateTime.Now.Ticks;
						_lastLogItem = _start;
					}
					var fromLast = DateTime.Now.Ticks - _lastLogItem;
					_lastLogItem = _lastLogItem + fromLast;
					var total = DateTime.Now.Ticks - _start;
					using (var writer = new StreamWriter(_file, true))
					{
						writer.WriteLine(
							string.Format(
								"{0},{1} - {2}",
								Math.Round((new TimeSpan(total)).TotalMilliseconds, 0),
								Math.Round((new TimeSpan(fromLast)).TotalMilliseconds),
								message
							)
						);	
					}
				} catch {
				}
			}
		}
	}
}
