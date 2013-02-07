using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Logging;
using OpenIDE.Core.RScripts;
using OpenIDE.Core.Language;

namespace OpenIDE.CodeEngine.Core.ReactiveScripts
{
	public class ReactiveScriptEngine
	{
		private string _keyPath;
		private ScriptTouchHandler _touchHandler;
		private ReactiveScriptReader _reader;
		private List<ReactiveScript> _scripts;	
		private Action<string> _dispatch;

		public ReactiveScriptEngine(string path, PluginLocator locator, Action<string> dispatch)
		{
			_keyPath = path;
			_dispatch = dispatch;
			_reader = 
				new ReactiveScriptReader(
				Path.GetDirectoryName(
					Path.GetDirectoryName(
						Assembly.GetExecutingAssembly().Location)),
				_keyPath,
				() => { return locator; },
				(m) => _dispatch(m));
			_touchHandler = new ScriptTouchHandler(_reader.GetPaths());
			_scripts = _reader.Read();
		}

		public void Handle(string message)
		{
			lock (_scripts)
			{
				var touchState = _touchHandler.Handle(message);
				if (touchState != ScriptTouchEvents.None)
					handleScriptTouched(message, touchState);
				_scripts
					.Where(x => x.ReactsTo(message)).ToList()
					.ForEach(x => x.Run(message));
			}
		}

		private void handleScriptTouched(string message, ScriptTouchEvents type) {
			var path = _touchHandler.GetPath(message);
			if (type == ScriptTouchEvents.Removed) {
				_scripts.RemoveAll(x => x.File.Equals(path));
				return;
			}
			var script = _reader.ReadScript(path);
			if (script == null)
				return;
			if (type == ScriptTouchEvents.Changed) {
				_scripts.RemoveAll(x => x.File.Equals(path));
				_scripts.Add(script);
			}
		}
	}
}
