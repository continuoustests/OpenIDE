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
			return true;	
		}
	}
}

