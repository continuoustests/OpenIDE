using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenIDE.Arguments;
using OpenIDE.CommandBuilding;
using System.IO;
using System.Diagnostics;

namespace OpenIDE.UI
{
    public partial class SnippetForm : Form
    {
		private SnippetReplaceController _controller;
		private bool _controlledUpdate = false;

		public string Content { get; private set; }

        public SnippetForm(string[] replaces, string content)
        {
            InitializeComponent();
			_controller = new SnippetReplaceController(replaces, content);
			updateForm();
        }
		
		void HandleHandleFormClosing (object sender, FormClosingEventArgs e)
        {
			Visible = false;
			Dispose();
        }

        private void textBoxPlaceholders_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
			else if (e.KeyCode == Keys.Enter)
				buttonRun_Click(this, new EventArgs());
			else
			{
				setContent();
				updateForm();
			}
        }
		
        private bool commandEndsWithSpace(string selection)
        {
            return selection.Length < textBoxPlaceholders.Text.Length &&
				   textBoxPlaceholders.Text.Substring(selection.Length, 1) == " ";
        }

        private void textBoxPlaceholders_TextChanged(object sender, EventArgs e)
        {
			setContent();
			updateForm();
        }

		private void textBoxPreview_textChanged(object sender, EventArgs e)
		{
			if (_controlledUpdate)
				return;
			_controller.SetModifiedContent(textBoxPreview.Text);
			updateLabel();
		}

        private void buttonRun_Click(object sender, EventArgs e)
        {
			Content = textBoxPreview.Text;
            Close();
        }

		private void setContent()
		{
			_controller.SetContent(textBoxPlaceholders.Text, textBoxPlaceholders.SelectionStart);
		}

		private void updateForm()
		{
			_controlledUpdate = true;
			textBoxPreview.Text = _controller.ModifiedSnippet;
			updateLabel();
			_controlledUpdate = false;
		}

		private void updateLabel()
		{
			labelInfo.Text = "Currently replacing: " + _controller.CurrentPlaceholder;
		}
    }
}
