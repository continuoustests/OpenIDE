using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;  
using System.Text;
using System.Windows.Forms;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.Core.Caching;

namespace OpenIDE.CodeEngine.Core.UI
{
    public partial class TypeSearchForm : Form
    {
    	private System.Threading.SynchronizationContext _syncContext;
		private ITypeCache _cache;
		private Action<string, int, int> _action;
		private Action _cancelAction;
		private DateTime _lastSearch = DateTime.Now;
        private bool _runSearch = false;
        private Queue<string> _searchTerms = new Queue<string>();
        private System.Threading.Thread _searchThread;
        private string _lastText = "";
        private DateTime _lastKeypress = DateTime.Now;
		
        public TypeSearchForm(ITypeCache cache, Action<string, int, int> action, Action cancelAction)
        {
            InitializeComponent();
        	_syncContext = AsyncOperationManager.SynchronizationContext;
			Refresh();
			_cache = cache;
			_action = action;
			_cancelAction = cancelAction;
            _runSearch = true;
			writeStatistics();
            _searchThread = new System.Threading.Thread(continuousSearch);
            _searchThread.Start();
        }
		
		void writeStatistics()
		{
			var text = string.Format("Projects: {2}, Files: {0}, Code References: {1}",
				_cache.FileCount,
				_cache.CodeReferences,
				_cache.ProjectCount);
			if (labelInfo.Text != text)
        		labelInfo.Text = text;
		}
		
		void HandleHandleFormClosing (object sender, FormClosingEventArgs e)
        {
            _runSearch = false;
            _searchThread.Join();
			Visible = false;
			Dispose();
        }

        private void continuousSearch() {
            // The sprinkled thread sleeps is for mono 3.x
            // winforms stuff freaking out
            while (_runSearch) {
                if (_lastKeypress.AddMilliseconds(200) > DateTime.Now || _searchTerms.Count == 0) {
                    System.Threading.Thread.Sleep(50);
                    continue;
                }
                var searchText = "";
                while (_searchTerms.Count > 0) {
                    searchText = _searchTerms.Dequeue();
                }
                try
                {
                    var items = _cache.Find(searchText).Take(30).ToList();
                    if (items.Count > 30)
                        items = items.GetRange(0, 30);
                    _syncContext.Post(nothing => informationList.Items.Clear(), null);
                    foreach (var item in items) {
                        _syncContext.Post(nothing => addItem(item), null);
                    }
                    _syncContext.Post(nothing => {
                        if (informationList.Items.Count > 0)
                            informationList.Items[0].Selected = true;
                    }, items);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        void HandleTextBoxSearchhandleKeyUp(object sender, KeyEventArgs e)
        {
            if (_lastText != textBoxSearch.Text) {
                _searchTerms.Enqueue(textBoxSearch.Text);
                _lastText = textBoxSearch.Text;
                _lastKeypress = DateTime.Now;
            }
        }

		void HandleTextBoxSearchhandleKeyDown(object sender, KeyEventArgs e)
        {
			if (e.KeyCode == Keys.Enter)
			{
	        	if (informationList.SelectedItems.Count != 1)
					return;
				var type = (ICodeReference) informationList.SelectedItems[0].Tag;
				_action.Invoke(type.File, type.Line, type.Column);
				Close();
			}
			else if (e.KeyCode == Keys.Down)
			{
				if (informationList.Items.Count == 0)
					return;
				if (informationList.SelectedItems.Count != 1)
					return;
				if (informationList.SelectedItems[0].Index < informationList.Items.Count - 1)
					informationList.Items[informationList.SelectedItems[0].Index + 1].Selected = true;
			}
			else if (e.KeyCode == Keys.Up)
			{
				if (informationList.Items.Count == 0)
					return;
				if (informationList.SelectedItems.Count != 1)
					return;
				if (informationList.SelectedItems[0].Index != 0)
					informationList.Items[informationList.SelectedItems[0].Index - 1].Selected = true;
			}
			else if (e.KeyCode == Keys.Escape)
			{
				_cancelAction.Invoke();
				Close();
			}
        }
		
		private void addItem(ICodeReference type)
		{
			var item = informationList.Items.Add(type.Language);
			item.SubItems.Add(type.Type);
			item.SubItems.Add(type.Name);
			item.SubItems.Add(
				string.Format("{0} ({1})",
					type.Signature,
					Path.GetFileName(type.File)));
			item.Tag = type;
		}
    }
}
