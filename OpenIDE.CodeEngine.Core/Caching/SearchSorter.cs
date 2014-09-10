using System;

namespace OpenIDE.CodeEngine.Core.Caching
{
	class SearchSorter
	{
		public int Sort(string name, string signature, string filename, string[] compareStrings)
		{
			var score = 0;
			foreach (var compareString in compareStrings) {
				if (name.Equals(compareString)) {
					score += 1000;
					continue;
				}
				if (name.ToLower().Equals(compareString.ToLower())) {
					score += 2000;
					continue;
				}
				if (name.StartsWith(compareString)) {
					score += 3000 + (name.Length - name.IndexOf(compareString));
					continue;
				}
				if (name.ToLower().StartsWith(compareString.ToLower())) {
					score += 4000 + (name.Length - name.IndexOf(compareString));
					continue;
				}
				if (name.EndsWith(compareString)) {
					score += 5000;
					continue;
				}
				if (name.Contains(compareString)) {
					score += 6000 + (name.Length - name.IndexOf(compareString));
					continue;
				}
				if (name.Contains(compareString)) {
					score += 7000 + (name.Length - name.IndexOf(compareString));
					continue;
				}

				if (signature.Equals(compareString)) {
					score += 8000;
					continue;
				}
				if (signature.ToLower().Equals(compareString.ToLower())) {
					score += 9000;
					continue;
				}
				if (signature.StartsWith(compareString)) {
					score += 10000 + (signature.Length - signature.IndexOf(compareString));
					continue;
				}
				if (signature.ToLower().StartsWith(compareString.ToLower())) {
					score += 11000 + (signature.Length - signature.IndexOf(compareString));
					continue;
				}
				if (signature.EndsWith(compareString)) {
					score += 12000;
					continue;
				}
				if (signature.ToLower().EndsWith(compareString.ToLower())) {
					score += 13000;
					continue;
				}
				if (signature.Contains(compareString)) {
					score += 14000 + (signature.Length - signature.IndexOf(compareString));
					continue;
				}
				if (signature.ToLower().Contains(compareString.ToLower())) {
					score += 14000 + (signature.Length - signature.IndexOf(compareString));
					continue;
				}

				if (filename.Contains(compareString)) {
					score += 14000 + (filename.Length - filename.IndexOf(compareString));
					continue;
				}
				score += 100000;
			}
			return score;
		}
	}
}
