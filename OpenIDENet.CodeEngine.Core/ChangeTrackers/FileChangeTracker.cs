using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Logging;
namespace OpenIDENet.CodeEngine.Core.ChangeTrackers
{
	public class FileChangeTracker
	{
		private string _watchPath;
		private string[] _patterns;
		private FileSystemWatcher _watcher;
		private Stack<FileSystemEventArgs> _buffer = new Stack<FileSystemEventArgs>();
		private Action<Stack<FileSystemEventArgs>> _changeHandler;
		
		public void Start(string path, string pattern, Action<Stack<FileSystemEventArgs>> changeHandler)
		{
			_watchPath = path;
			_patterns = pattern
				.Replace("*", "")
				.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			_changeHandler = changeHandler;
			new Thread(startChangeHandler).Start();
			new Thread(start).Start();
		}
		
		private void startChangeHandler(object state)
		{
			while (true)
			{
				Thread.Sleep(300);
				_changeHandler.Invoke(_buffer);
			}
		}
		
		private void start(object state)
		{
			startListener();
		}
		
		private void startListener()
		{
			if (_watcher != null)
			{
				_watcher.Changed -= WatcherChangeHandler;
	            _watcher.Created -= WatcherChangeHandler;
	            _watcher.Deleted -= WatcherChangeHandler;
	            _watcher.Renamed -= WatcherChangeHandler;
	            _watcher.Error -= WatcherErrorHandler;
			}
			
			_watcher = new FileSystemWatcher
                           {
                               NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Attributes,
                               IncludeSubdirectories = true
                           };
			_watcher.Changed += WatcherChangeHandler;
            _watcher.Created += WatcherChangeHandler;
            _watcher.Deleted += WatcherChangeHandler;
            _watcher.Renamed += WatcherChangeHandler;
            _watcher.Error += WatcherErrorHandler;
			_watcher.Path = _watchPath;
			_watcher.EnableRaisingEvents = true;
		}
		
		private void WatcherChangeHandler(object sender, FileSystemEventArgs e)
        {
			if (!_patterns.Contains(Path.GetExtension(e.FullPath)))
				return;
            addToBuffer(e);
        }

        private void WatcherErrorHandler(object sender, ErrorEventArgs e)
        {
        }
		
		private void addToBuffer(FileSystemEventArgs file)
        {
			if (Directory.Exists(file.FullPath) && (file.ChangeType == WatcherChangeTypes.Created || file.ChangeType == WatcherChangeTypes.Renamed))
				startListener();
			
            _buffer.Push(file);
        }
	}
}

