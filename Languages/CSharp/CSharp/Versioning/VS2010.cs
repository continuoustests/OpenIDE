using System;
using System.IO;

namespace CSharp.Versioning
{
	public class VS2010 : IAmProjectVersion
	{
		public bool IsValid(string projecFile)
		{
			if (!File.Exists(projecFile))
				return false;
			var content = File.ReadAllText(projecFile);
			return content.Contains("<ProductVersion>11.0.") ||
				content.Contains("<ProductVersion>10.0.") ||
				content.Contains("<ProductVersion>9.0.") ||
				content.Contains("<ProductVersion>8.0.");
		}
	}
}

