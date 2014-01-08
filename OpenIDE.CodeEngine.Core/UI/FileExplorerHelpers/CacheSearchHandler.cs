using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using OpenIDE.Core.Caching;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Caching.Search;
using OpenIDE.Core.Logging;
using System.Diagnostics;
using CoreExtensions;

namespace OpenIDE.CodeEngine.Core.UI.FileExplorerHelpers
{
	class CacheSearchHandler : ISearchHandler
	{
		private ITypeCache _cache;
		private string _defaultLanguage;
		private TreeView _list;

		public CacheSearchHandler(ITypeCache cache, string defaultLanguage, TreeView list)
		{
			_cache = cache;
			_defaultLanguage = defaultLanguage;
			_list = list;
		}
		
		public void ListFromSearch(string expression)
		{
			var files = _cache.FindFiles(expression).ToList();
            _list.Nodes.Clear();
            listResult(files, _list.Nodes, (result) => { return Path.GetFileName(result.File); });
		}

		public string GetDescription(TreeNode node)
		{
			var tag = node.Tag;
            if (tag == null)
                return null;
            else if (tag.GetType().Equals(typeof(FileFindResult)))
                return ((FileFindResult)tag).File;
			return null;
		}

		public void OneUp(TreeNode node)
		{
			var tag = node.Tag;
			if (tag == null)
				return;
			if (!node.Tag.GetType().Equals(typeof(FileFindResult)))
				return;
			var result = (FileFindResult)node.Tag;
			ListFromSearch(Path.GetDirectoryName(result.File));
			foreach (TreeNode newnode in _list.Nodes)
				newnode.Expand();
			_list.SelectedNode = _list.Nodes[0];
		}

		public void BeforeExpand(TreeNode node)
		{
			if (node.Nodes.Count != 1 && node.Nodes[0].Tag != null)
                return;
            var result = (FileFindResult)node.Tag;
            if (result.Type == FileFindResultType.Directory)
			{
                addSubNodes(
					_cache.GetFilesInDirectory(result.File + Path.DirectorySeparatorChar.ToString()).ToList(),
					node.Nodes);
			}
            if (result.Type == FileFindResultType.DirectoryInProject)
			{
                addSubNodes(
					_cache.GetFilesInProject(
						result.File + Path.DirectorySeparatorChar.ToString(),
						result.ProjectPath).ToList(),
					node.Nodes);
			}
		}

		public FilePosition PositionFromnode(TreeNode node)
		{
			if (node.Tag == null)
	            return null;
            var result = (FileFindResult)node.Tag;
            if (result.Type == FileFindResultType.Project || result.Type == FileFindResultType.File)
				return new FilePosition(result.File, 0, 0);
			return null;
		}

        public void Run(TreeNode node)
        {
            if (node.Tag == null)
                return;
            var result = (FileFindResult)node.Tag;
            var directory = Path.GetDirectoryName(result.File);
            if (result.Type == FileFindResultType.Directory || result.Type == FileFindResultType.DirectoryInProject)
                directory = result.File;
			var additionalParameters = "";
			if (_defaultLanguage != null)
				additionalParameters = " --default.language=" + _defaultLanguage;

            var proc = new Process();
            proc.Run("oi", additionalParameters, false, directory);
        }

		private void listResult(
			List<FileFindResult> files,
			TreeNodeCollection nodes,
			Func<FileFindResult, string> getName)
        {
            files
                .Where(x => isDirectory(x))
                .OrderBy(x => getName(x)).ToList()
                .ForEach(x => addNode(nodes, getName(x), x));
            files
                .Where(x => !isDirectory(x))
                .OrderBy(x => getName(x)).ToList()
                .ForEach(x => addNode(nodes, getName(x), x));
        }

		private static bool isDirectory(FileFindResult x)
        {
            return x.Type == FileFindResultType.Directory || x.Type == FileFindResultType.DirectoryInProject;
        }

        private void addNode(TreeNodeCollection nodes, string name, FileFindResult result)
        {
            int image = 0;
            if (result.Type == FileFindResultType.File)
                image = 4;
            if (result.Type == FileFindResultType.Project)
                image = 5;
            var node = nodes.Add(name);
            node.ImageIndex = image;
            node.SelectedImageIndex = image;
            node.Tag = result;
            if (isDirectory(result))
                node.Nodes.Add("");
        }

		private void addSubNodes(List<FileFindResult> files, TreeNodeCollection nodes)
        {
            nodes.Clear();
            listResult(files, nodes, (result) => { return Path.GetFileName(result.File); });
        }
	}
}
