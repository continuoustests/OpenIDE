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
		private bool _delayedSearchTriggered = false;
		
        public TypeSearchForm(ITypeCache cache, Action<string, int, int> action, Action cancelAction)
        {
            InitializeComponent();
        	_syncContext = AsyncOperationManager.SynchronizationContext;
			Refresh();
			_cache = cache;
			_action = action;
			_cancelAction = cancelAction;
			writeStatistics();
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
			Visible = false;
			Dispose();
        }
		
		void HandleTextBoxSearchhandleTextChanged(object sender, System.EventArgs e)
        {
        	if (DateTime.Now < _lastSearch.AddMilliseconds(500)) {
        		_delayedSearchTriggered = true;
        		return;
        	}
        	System.Threading.ThreadPool.QueueUserWorkItem((o) => {
				performSearch();
			});
        }

        private void performSearch() {
        	_lastSearch = DateTime.Now;
        	_syncContext.Post(message =>
            {
	        	try
				{
		        	informationList.Items.Clear();
					var items = _cache.Find(textBoxSearch.Text).Take(30).ToList();
					if (items.Count > 30)
						items = items.GetRange(0, 30);
					items.ForEach(x => addItem(x));
					if (informationList.Items.Count > 0)
						informationList.Items[0].Selected = true;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
				var runSearch = _delayedSearchTriggered;
				_delayedSearchTriggered = false;
				if (runSearch)
					performSearch();
			}, null);
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
