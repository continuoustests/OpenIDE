using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharp.FileSystem;
using CSharp.Projects;
namespace CSharp.Crawlers
{
	public class SolutionReader
	{
		private string _file;
		
		public SolutionReader(string solutionFile)
		{
			_file = solutionFile;
		}
		
		public List<Project> ReadProjects()
		{
			var projects = new List<Project>();
			if (!File.Exists(_file))
				return projects;
			var content = File.ReadAllText(_file);
			var projectChunks = readAllChunks(content);
			projectChunks.ForEach(x => 
			                       {
										var file = getProjectPath(x);
										if (file != null)
											projects.Add(new Project(file));
								   });
			return projects;
		}
		
		private List<string> readAllChunks(string content)
		{
			int currentPosition = 0;
			var chunks = new List<string>();
			while (true)
			{
				int start = findProjectStart(content, currentPosition);
				if (start == -1)
					break;
				int length = findLengthToProjectEnd(content, start);
				if (length == -1)
					break;
				chunks.Add(content.Substring(start, length));
				currentPosition = start + length;
			}
			return chunks;
		}
		
		private int findProjectStart(string content, int index)
		{
			var searchFor = "Project(";
			var start =  content.IndexOf(searchFor, index);
			if (start == -1)
				return -1;
			return start + searchFor.Length;
		}
		
		private int findLengthToProjectEnd(string content, int index)
		{
			var searchFor = "EndProject";
			var end =  content.IndexOf(searchFor, index);
			if (end == -1)
				return -1;
			return end - index;
		}
		
		private string getProjectPath(string projectChunk)
		{
			var chunks = projectChunk.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length != 3)
				return null;
			return new PathParser(chunks[1].Replace("\"", "").Trim())
				.ToAbsolute(_file)
				.Replace("\\", Path.DirectorySeparatorChar.ToString());
		}
	}
}

