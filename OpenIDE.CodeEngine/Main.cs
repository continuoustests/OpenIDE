using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using OpenIDE.CodeEngine.Core.Bootstrapping;
using OpenIDE.CodeEngine.Core.Endpoints;
using OpenIDE.Core.Config;
using OpenIDE.Core.Logging;

namespace OpenIDE.CodeEngine
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length < 1)
				return;
			var path =  args[0];
			setupLogging(path);
			
			Logger.Write("Initializing with path: {0}", path);
			string defaultLanguage = null;
			if (args.Length > 1) {
				defaultLanguage = args[1];	
				Logger.Write("Default language is: {0}", defaultLanguage);
			}
			string[] enabledLanguages = null;
			if (args.Length > 2 && args[2].Length > 0) {
				enabledLanguages = args[2].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				Logger.Write("Enabled languages are: {0}", args[2]);
			}
			if (!Directory.Exists(path) && !File.Exists(path))
				return;

			var endpoint = Bootstrapper.GetEndpoint(path, enabledLanguages);		
			if (!runForm(endpoint, defaultLanguage))
				startEngine(endpoint);
			Bootstrapper.Shutdown();
		}

		private static void setupLogging(string path) {
			var reader = new ConfigReader(path);
			var logPath = reader.Get("oi.logpath");
			if (Directory.Exists(logPath))
            	Logger.Assign(new FileLogger(Path.Combine(logPath, "OpenIDE.CodeEngine.log")));
		}
		
		private static bool runForm(CommandEndpoint endpoint, string defaultLanguage)
		{
			try {
				var form = new TrayForm(
					endpoint,
					defaultLanguage,
					Bootstrapper.GetCacheBuilder());
				Application.Run(form);
				return true;
			} catch {
				return false;
			}
		}

		private static void startEngine(CommandEndpoint endpoint)
        {
        	Logger.Write("Application initialized");
            endpoint.Start();
            while (endpoint.IsAlive)
                Thread.Sleep(100);
            Logger.Write("Shutting down");
        }
	}
}

