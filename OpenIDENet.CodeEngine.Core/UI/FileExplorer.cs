using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenIDENet.CodeEngine.Core.Caching;
using System.IO;
using OpenIDENet.CodeEngine.Core.Caching.Search;

namespace OpenIDENet.CodeEngine.Core.UI
{
    public partial class FileExplorer : Form
    {
        private ITypeCache _cache;
		private Action<string, int, int> _action;
		private Action _cancelAction;
		
        public FileExplorer(ITypeCache cache, Action<string, int, int> action, Action cancelAction)
        {
            InitializeComponent();
			Refresh();
			_cache = cache;
			_action = action;
			_cancelAction = cancelAction;
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            var files = _cache.FindFiles(textBoxSearch.Text.Trim());
            treeViewFiles.Nodes.Clear();
            listResult(files, treeViewFiles.Nodes, (result) => { return Path.GetFileName(result.File); });
        }

        private void listResult(List<FileFindResult> files, TreeNodeCollection nodes, Func<FileFindResult, string> getName)
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
            if (result.Type != FileFindResultType.File)
                node.Nodes.Add("");
        }

        private void treeViewFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var tag = e.Node.Tag;
            if (tag == null)
                return;
            else if (tag.GetType().Equals(typeof(FileFindResult)))
                labelInfo.Text = ((FileFindResult)tag).File;
        }

        private void treeViewFiles_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count != 1 && e.Node.Nodes[0].Tag != null)
                return;
            var result = (FileFindResult)e.Node.Tag;
            if (result.Type == FileFindResultType.Directory)
                addSubNodes(_cache.GetFilesInDirectory(result.File), e.Node);
            if (result.Type == FileFindResultType.Project)
                addSubNodes(_cache.GetFilesInProject(result.File), e.Node);
            if (result.Type == FileFindResultType.DirectoryInProject)
                addSubNodes(_cache.GetFilesInProject(result.File, result.ProjectPath), e.Node);
        }

        private void addSubNodes(List<FileFindResult> files, TreeNode node)
        {
            node.Nodes.Clear();
            listResult(files, node.Nodes, (result) => { return Path.GetFileName(result.File); });
        }

        private void FileExplorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancelAction();
        }

        private void treeViewFiles_DoubleClick(object sender, EventArgs e)
        {
            if (treeViewFiles.SelectedNode.Tag == null)
                return;
            var result = (FileFindResult)treeViewFiles.SelectedNode.Tag;
            if (result.Type == FileFindResultType.Project || result.Type == FileFindResultType.File)
            {
                _action(result.File, 0, 0);
                Close();
            }
        }

        private void FileExplorer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
    }
}
