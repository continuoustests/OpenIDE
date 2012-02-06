using System;

namespace OpenIDE.CodeEngine.Core.Caching
{
	class SearchSorter
	{
		public int Sort(string name, string signature, string compareString)
		{
			if (name.Equals(compareString))
				return 1000;
			if (name.ToLower().Equals(compareString.ToLower()))
				return 2000;
			if (name.StartsWith(compareString))
				return 3000 + (name.Length - name.IndexOf(compareString));
			if (name.ToLower().StartsWith(compareString.ToLower()))
				return 4000 + (name.Length - name.IndexOf(compareString));
			if (name.EndsWith(compareString))
				return 5000;
			if (name.Contains(compareString))
				return 6000 + (name.Length - name.IndexOf(compareString));
			if (name.Contains(compareString))
				return 7000 + (name.Length - name.IndexOf(compareString));

			if (signature.Equals(compareString))
				return 8000;
			if (signature.ToLower().Equals(compareString.ToLower()))
				return 9000;
			if (signature.StartsWith(compareString))
				return 10000 + (signature.Length - signature.IndexOf(compareString));
			if (signature.ToLower().StartsWith(compareString.ToLower()))
				return 11000 + (signature.Length - signature.IndexOf(compareString));
			if (signature.EndsWith(compareString))
				return 12000;
			if (signature.ToLower().EndsWith(compareString.ToLower()))
				return 13000;
			if (signature.Contains(compareString))
				return 14000 + (signature.Length - signature.IndexOf(compareString));
			if (signature.ToLower().Contains(compareString.ToLower()))
				return 14000 + (signature.Length - signature.IndexOf(compareString));
			return 100000;
		}
	}
}
