using System;
using System.IO;
using System.Windows.Forms;
using OpenIDENet.CodeEngine.Core.Bootstrapping;

namespace OpenIDENet.CodeEngine
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
			if (!Directory.Exists(path) && !File.Exists(path))
				return;

			var endpoint = Bootstrapper.GetEndpoint(path);		
			Application.Run(new TrayForm(endpoint, defaultLanguage));
			Bootstrapper.Shutdown();
		}
	}

    
}

