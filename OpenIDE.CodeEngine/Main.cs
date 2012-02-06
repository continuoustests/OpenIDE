using System;
using System.IO;
using System.Windows.Forms;
using OpenIDE.CodeEngine.Core.Bootstrapping;

namespace OpenIDE.CodeEngine
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length < 1)
				return;
			var path = args[0];
			string defaultLanguage = null;
			if (args.Length > 1)
				defaultLanguage = args[1];	
			string[] enabledLanguages = null;
			if (args.Length > 2 && args[2].Length > 0)
				enabledLanguages = args[2].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			if (!Directory.Exists(path) && !File.Exists(path))
				return;

			var endpoint = Bootstrapper.GetEndpoint(path, enabledLanguages);		
			Application.Run(
				new TrayForm(
					endpoint,
					defaultLanguage,
					Bootstrapper.GetCacheBuilder()));
			Bootstrapper.Shutdown();
		}
	}
}

