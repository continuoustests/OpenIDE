using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenIDE.Core.Language;
using OpenIDE.Core.Logging;
using OpenIDE.Core.RScripts;
using OpenIDE.Core.Scripts;

namespace OpenIDE.CodeEngine.Core.ReactiveScripts
{
	public class ReactiveScriptEngine
	{
		private string _keyPath;
		private ScriptTouchHandler _touchHandler;
		private ReactiveScriptReader _reader;
		private List<ReactiveScript> _scripts;	
		private List<string> _pausedScripts;
		private Action<string,string> _outputDispatcher;
		private Action<string> _dispatch;

		public ReactiveScriptEngine(string path, PluginLocator locator, Action<string,string> outputDispatcher, Action<string> dispatch)
		{
			_keyPath = path;
			_outputDispatcher = outputDispatcher;
			_dispatch = dispatch;
			_pausedScripts = new List<string>();
			_reader =
				new ReactiveScriptReader(
					_keyPath,
					() => { return locator; },
					_outputDispatcher,
					(m) => _dispatch(m));
			_touchHandler = new ScriptTouchHandler(_reader.GetPaths());
			_scripts = _reader.Read();
			foreach (var script in _scripts) {
				if (script.IsService)
					script.StartService();
			}
		}

		public void Handle(string message)
		{
			Logger.Write("Waiting for reactive script lock for "+message);
			lock (_scripts)
			{
				var touchState = _touchHandler.Handle(message);
				if (touchState != ScriptTouchEvents.None) {
					handleScriptTouched(message, touchState);
                }
                Logger.Write("Running affected scripts");
				_scripts
					.Where(x => 
						!_pausedScripts.Contains(x.Name) &&
						x.ReactsTo(message)).ToList()
					.ForEach(x => {
						Logger.Write("Running reactive script: "+x.Name);
						x.Run(message);
					});
                Logger.Write("Running affected scripts - Done");
			}
		}

		public string GetState(string name)
		{
			if (!_scripts.Any(x => x.Name == name))
				return "unknown";
			else if (_pausedScripts.Contains(name))
				return "paused";
			else if (_scripts.Any(x => x.Name == name && x.IsServiceRunning))
				return "ready-service";
			else
				return "ready";
		}

		public void Shutdown() {
			foreach (var script in _scripts)
				script.Shutdown();
		}

		private void handleScriptTouched(string message, ScriptTouchEvents type) {
			var path = _touchHandler.GetPath(message);
            if (new ScriptFilter().IsValid(path) == false)
                return;
			if (type == ScriptTouchEvents.Removed) {
                Logger.Write("Removing touched rscript");
				removeScript(path);
				return;
			}
			// Read script and dispatch errors
			var script = _reader.ReadScript(path, true);
			if (script == null) {
                Logger.Write("No "+path+" rscript found or script crashed. exiting..");
                removeScript(path);
				return;
            }
            if (type == ScriptTouchEvents.Pause) {
            	script.Shutdown();
            	if (!_pausedScripts.Contains(script.Name))
            		_pausedScripts.Add(script.Name);
            }
            if (type == ScriptTouchEvents.Resume) {
            	_pausedScripts.Remove(script.Name);
            	if (script.IsService && !script.IsServiceRunning)
					script.StartService();
            }
			if (type == ScriptTouchEvents.Changed || type == ScriptTouchEvents.Added || type == ScriptTouchEvents.Restart) {
                Logger.Write("Reloading / adding existing rscript");
				removeScript(path);
				_scripts.Add(script);
				if (script.IsService)
					script.StartService();
			}
		}

		private void removeScript(string file) {
			var script = _scripts.FirstOrDefault(x => x.File == file);
			if (script == null)
				return;
			script.Shutdown();
			_scripts.Remove(script);
		}
	}
}
