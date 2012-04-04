using System;

namespace OpenIDE.CodeEngine.Core.ChangeTrackers
{
	public enum ChangeType
	{
		DirectoryCreated,
		DirectoryDeleted,
		FileCreated,
		FileChanged,
		FileDeleted
	}

	public class Change
	{
		public ChangeType Type { get; private set; }
		public string Path { get; private set; }

		public Change(ChangeType type, string path)
		{
			Type = type;
			Path = path;
		}
	}
}
