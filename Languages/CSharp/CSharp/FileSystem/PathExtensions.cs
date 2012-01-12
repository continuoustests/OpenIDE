using System;
using System.IO;
namespace CSharp.FileSystem
{
	public static class PathExtensions
	{
		public static string GetRelativePath(string templateFile, string fullpathFile)
		{
			var envtemplateFile = removePathEscape(AdjustToEnvironment(templateFile));
			var envfullpathFile = removePathEscape(AdjustToEnvironment(fullpathFile));
			if (envfullpathFile.Equals(envtemplateFile))
				return fullpathFile;
			var templateDir = envtemplateFile;
			if (!Directory.Exists(envtemplateFile))
				templateDir = Path.GetDirectoryName(envtemplateFile);
			if (envfullpathFile.Equals(templateDir))
				return fullpathFile;
			if (templateDir.Length == 0)
				return fullpathFile;
			if (envfullpathFile.Length > templateDir.Length && envfullpathFile.Substring(0, templateDir.Length).Equals(templateDir))
				return extractRelativePath(fullpathFile, templateDir);
			int level;
			if ((level = checkRecursiveLevels(templateDir, envfullpathFile)) != -1)
				return getRelativePathFromLevel(templateDir, fullpathFile, level);
			return fullpathFile;
		}
		
		private static string removePathEscape(string path)
		{
			if (path.Substring(path.Length - 1, 1).Equals(Path.DirectorySeparatorChar.ToString()))
				return path.Substring(0, path.Length - 1);
			return path;
		}
		
		private static string getRelativePathFromLevel(string templateDir, string fullPathFile, int level)
		{
			var dir = templateDir;
			for (int i = 0; i < level; i++)
				dir = Path.GetDirectoryName(dir);
			var relativePath = extractRelativePath(fullPathFile, dir);
			for (int i = 0; i < level; i++)
				relativePath = Path.Combine("..", relativePath);
			return relativePath;
		}
				
		private static string extractRelativePath(string path, string templatePath)
		{
			if (path.Length.Equals(templatePath.Length))
				return "";
			return path.Substring(templatePath.Length + 1, path.Length - (templatePath.Length + 1));
		}
		
		private static int checkRecursiveLevels(string templateDir, string fullPathFile)
		{
			return checkRecursiveLevels(templateDir, fullPathFile, 1);
		}
		
		private static int checkRecursiveLevels(string templateDir, string fullPathFile, int level)
		{
			var dir = Path.GetDirectoryName(templateDir);
			if (dir == null || dir.Length == 0)
				return -1;
			if (fullPathFile.Length >= dir.Length && fullPathFile.Substring(0, dir.Length).Equals(dir))
				return level;
			return checkRecursiveLevels(dir, fullPathFile, level + 1);
		}
		
		public static string AdjustToEnvironment(string path)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				return path;
			else
				return path.ToLower();
		}
	}
}

