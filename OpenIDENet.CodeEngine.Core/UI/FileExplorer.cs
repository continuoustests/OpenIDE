using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.UI.FileExplorerHelpers;

namespace OpenIDENet.CodeEngine.Core.UI
{
    public partial class FileExplorer : Form
    {
        private ITypeCache _cache;
		private Action<string, int, int> _action;
		private Action _cancelAction;
		private ISearchHandler _handler;
		
        public FileExplorer(ITypeCache cache, Action<string, int, int> action, Action cancelAction)
        {
            InitializeComponent();
			Refresh();
			_cache = cache;
			_action = action;
			_cancelAction = cancelAction;
			_handler = new CacheSearchHandler(_cache, treeViewFiles);
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
			_handler.ListFromSearch(textBoxSearch.Text.Trim());
        }

        private void treeViewFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var description = _handler.GetDescription(e.Node);
			if (description != null)
				labelInfo.Text = description;
        }

        private void treeViewFiles_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            _handler.BeforeExpand(e.Node);
        }

        private void FileExplorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancelAction();
			Visible = false;
			Dispose();
        }

        private void treeViewFiles_DoubleClick(object sender, EventArgs e)
        {
			var position = _handler.PositionFromnode(treeViewFiles.SelectedNode);
			if (position != null)
	            _action(position.Fullpath, position.Line, position.Column);
        }

        private void FileExplorer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void treeViewFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
				var position = _handler.PositionFromnode(treeViewFiles.SelectedNode);
				if (position != null)
                    _action(position.Fullpath, position.Line, position.Column);
            }
            if (e.Control && e.KeyCode == Keys.F)
            {
                textBoxSearch.SelectAll();
                textBoxSearch.Focus();
            }
            if (e.KeyCode.Equals(Keys.K))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                System.Windows.Forms.SendKeys.Send("{UP}");
                return;
            }
            if (e.KeyCode.Equals(Keys.J))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                System.Windows.Forms.SendKeys.Send("{DOWN}");
                return;
            }

            if (e.KeyCode.Equals(Keys.H))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                System.Windows.Forms.SendKeys.Send("{LEFT}");
                return;
            }

            if (e.KeyCode.Equals(Keys.L))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                System.Windows.Forms.SendKeys.Send("{RIGHT}");
                return;
            }
			if (e.Control && e.KeyCode == Keys.Up)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				_handler.OneUp(treeViewFiles.SelectedNode);
				return;
			}
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
        }
    }
}
