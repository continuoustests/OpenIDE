using System;
using System.IO;
namespace OpenIDENet.Versioning
{
	public class VS2010 : IAmProjectVersion
	{
		public bool IsValid(string projecFile)
		{
			if (!File.Exists(projecFile))
				return false;
			return File.ReadAllText(projecFile).Contains("<ProductVersion>9.0.") ||
				File.ReadAllText(projecFile).Contains("<ProductVersion>8.0.");
		}
	}
}

