using System;

namespace CSharp.Crawlers
{
	public class CrawlOptions
	{
		public bool IsSolutionFile { get; private set; }
		public string File { get; private set; }
		public string Directory { get; private set; }
		
		public CrawlOptions(string path)
		{
			IsSolutionFile = System.IO.File.Exists(path) && System.IO.Path.GetExtension(path).Equals(".sln");
			File = null;
			Directory = null;
			if (System.IO.File.Exists(path))
			{
				File = path;
				Directory = System.IO.Path.GetDirectoryName(path);
			}
			else
			{
				File = null;
				Directory = path;
			}
		}

		public override string ToString() {
			if (File == null)
				return Directory;
			else
				return File;
		}
	}
	
	public interface ICrawler
	{
		void Crawl(CrawlOptions options);
	}
}

