using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using FSWatcher;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.Core.Logging;
namespace OpenIDE.CodeEngine.Core.ChangeTrackers
{
	public class FileChangeTracker : IDisposable
	{
		private string _watchPath;
		private string[] _patterns;
		private HashSet<string> _ignoreDirectories = new HashSet<string>();
		private Thread _changeHandlerThread;
		private Watcher _watcher;
		private Stack<Change> _buffer = new Stack<Change>();
		private Action<Stack<Change>> _changeHandler;
		private Action<Change> _rawHandler;

		public FileChangeTracker(Action<Change> rawHandler)
		{
			_rawHandler = rawHandler;
		}
		
		public void Start(string path, string pattern, Action<Stack<Change>> changeHandler, string[] ignoreDirectories)
		{
			_watchPath = path;
			_patterns = pattern
				.Replace("*", "")
				.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			_changeHandler = changeHandler;
			_ignoreDirectories = new HashSet<string>();
			ignoreDirectories.ToList().ForEach(x => _ignoreDirectories.Add(x));
			_changeHandlerThread = new Thread(startChangeHandler);
			_changeHandlerThread.Start();
			start();
			_patterns.ToList().ForEach(x => Logger.Write(x));
		}
		
		private void startChangeHandler(object state)
		{
			while (true)
			{
				Thread.Sleep(300);
				_changeHandler.Invoke(_buffer);
			}
		}
		
		private void start()
		{
			var keepWatching = true;
			while (keepWatching) {
				try {
					keepWatching = false;
					_watcher = new Watcher(
						_watchPath,
						(dir) => WatcherChangeHandler(ChangeType.DirectoryCreated, dir),
						(dir) => WatcherChangeHandler(ChangeType.DirectoryDeleted, dir),
						(file) => WatcherChangeHandler(ChangeType.FileCreated, file),
						(file) => WatcherChangeHandler(ChangeType.FileChanged, file),
						(file) => WatcherChangeHandler(ChangeType.FileDeleted, file));
					_watcher.Watch();
				} catch (Exception ex) {
					Logger.Write(ex);
					keepWatching = true;
				}
			}
		}
		
		private void WatcherChangeHandler(ChangeType type, string path)
        {
        	if (_ignoreDirectories.Any(x => path.StartsWith(x)))
        		return;
        	
			var change = new Change(type, path);
			_rawHandler(change);
			
			if (!_patterns.Contains(Path.GetExtension(path)))
				return;
			_buffer.Push(change);
        }

		public void Dispose()
		{
			if (_watcher != null)
				_watcher.StopWatching();
			if (_changeHandlerThread != null)
				_changeHandlerThread.Abort();
		}
	}
}

