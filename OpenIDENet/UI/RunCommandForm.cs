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

namespace OpenIDENet.UI
{
    public partial class RunCommandForm : Form
    {
        private string _directory;
        private CommandBuilder _builder;

        public RunCommandForm(string directory, CommandBuilder builder)
        {
            InitializeComponent();
            _directory = directory;
            _builder = builder;
            listOptions();
            if (informationList.Items.Count > 0)
                informationList.Items[0].Selected = true;
            Text = "Running command from " + _directory;
        }

        private void listOptions()
        {
            informationList.Items.Clear();
            _builder.AvailableCommands.ToList()
                .ForEach(x => informationList.Items.Add(x.Replace("/", " ").Trim()));
        }
		
		void HandleHandleFormClosing (object sender, FormClosingEventArgs e)
        {
			Visible = false;
			Dispose();
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (informationList.Items.Count == 0)
                    return;
                if (informationList.SelectedItems.Count != 1)
                    informationList.Items[0].Selected = true;
                if (informationList.SelectedItems[0].Index < informationList.Items.Count - 1)
                    informationList.Items[informationList.SelectedItems[0].Index + 1].Selected = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (informationList.Items.Count == 0)
                    return;
                if (informationList.SelectedItems.Count != 1)
                    informationList.Items[0].Selected = true;
                if (informationList.SelectedItems[0].Index != 0)
                    informationList.Items[informationList.SelectedItems[0].Index - 1].Selected = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            var last = textBoxSearch.Text.LastIndexOf(" ");
            if (last == -1)
                last = 0;
            else
                last++;
            var text = textBoxSearch.Text
                .Substring(last, textBoxSearch.Text.Length - last).Trim();
            var dir = text
                .TrimStart(new char[] { Path.DirectorySeparatorChar });
            if (!Directory.Exists(dir) && text.StartsWith(Path.DirectorySeparatorChar.ToString()))
                dir = Path.Combine(_directory, dir.TrimEnd(new char[] { Path.DirectorySeparatorChar }));
            labelDescription.Text = dir;
            if (!dir.EndsWith(":") && Directory.Exists(dir))
            {
                informationList.Items.Clear();
                Directory.GetDirectories(dir).ToList()
                    .ForEach(x => getFileName(dir, x));
                Directory.GetFiles(dir).ToList()
                    .ForEach(x => getFileName(dir, x));
            }
            else if (textBoxSearch.Text.EndsWith(" "))
            {
                var path = "/" + textBoxSearch.Text.Trim().Replace(' ', '/');
                _builder.NavigateTo(path);
                listOptions();
            }
            else
            {
                var path = "/" + textBoxSearch.Text.Trim().Replace(' ', '/');
                _builder.NavigateTo(path.Substring(0, path.LastIndexOf('/')));
                listOptions();
            }
        }

        private ListViewItem getFileName(string dir, string x)
        {
            return informationList.Items.Add(x.Substring(dir.Length, x.Length - dir.Length).TrimStart(new char[] { Path.DirectorySeparatorChar }));
            if (x.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return informationList.Items.Add(x.Substring(dir.Length - 1, x.Length - dir.Length + 1));
            else
                return informationList.Items.Add(x.Substring(dir.Length + 1, x.Length - dir.Length - 1));
        }

        private void informationList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (informationList.SelectedItems.Count == 0)
                return;
            labelDescription.Text = _builder.Describe("/" + informationList.SelectedItems[0].Text.Replace(" ", "/"));
        }
    }
}
