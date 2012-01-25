using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenIDENet.Arguments;
using OpenIDENet.CommandBuilding;
using System.IO;
using System.Diagnostics;

namespace OpenIDENet.UI
{
    public partial class SnippetForm : Form
    {
		private SnippetReplaceController _controller;

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
        }
		
        private bool commandEndsWithSpace(string selection)
        {
            return selection.Length < textBoxPlaceholders.Text.Length &&
				   textBoxPlaceholders.Text.Substring(selection.Length, 1) == " ";
        }

        private void textBoxPlaceholders_TextChanged(object sender, EventArgs e)
        {
			_controller.SetContent(textBoxPlaceholders.Text, textBoxPlaceholders.SelectionStart);
			updateForm();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            Close();
        }

		private void updateForm()
		{
			textBoxPreview.Text = _controller.ModifiedSnippet;
			labelInfo.Text = "Currently replacing: " + _controller.CurrentPlaceholder;
		}
    }
}
