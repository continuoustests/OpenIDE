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

		private bool _directoryMode = false;
		private string _lastDirectory = null;

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
			if ((e.Control && e.KeyCode == Keys.N) || e.KeyCode == Keys.Enter)
			{
				if (informationList.Items.Count == 0)
                    return;
                if (informationList.SelectedItems.Count != 1)
                    informationList.Items[0].Selected = true;

				if (_directoryMode)
				{
					var dirContent = getContent();
					var lookFor = Path.DirectorySeparatorChar;
					if (dirContent.LastIndexOf(Path.DirectorySeparatorChar) < dirContent.LastIndexOf(' ') && (dirContent.Count(x => x.Equals('"')) % 2) == 0)
						lookFor = ' ';
					var dir = dirContent.Substring(dirContent.LastIndexOf(lookFor) + 1, dirContent.Length - (dirContent.LastIndexOf(lookFor) + 1));
					var item = informationList.SelectedItems[0].Text;
					if (item.StartsWith(dir) && item.Length > dir.Length)
						textBoxSearch.SelectedText = item.Substring(dir.Length, item.Length - dir.Length);
					else
						textBoxSearch.SelectedText = item;
					return;
				}

				var content = splitParams(informationList.SelectedItems[0].Text);
                var text = splitParams(getContent());
				var autocomplete = "";
				for (int i = 0; i < content.Length; i++)
				{
					if (text.Length < (i + 1))
					{
						autocomplete = content[i];
						break;
					}
					if (text[i] == content[i])
						continue;
					if (content[i].StartsWith(text[i]))
					{
						var selection = getContent();
						var ending = " ";
						if (selection.Length < textBoxSearch.Text.Length && textBoxSearch.Text.Substring(selection.Length, 1) == " ")
							ending = "";
						autocomplete = content[i].Substring(text[i].Length, content[i].Length - text[i].Length) + ending;
						break;
					}
				}
				if (autocomplete.Length > 0)
					textBoxSearch.SelectedText = autocomplete;
			}
			else if (e.Control && e.KeyCode == Keys.Space)
			{
				if (_directoryMode)
					return;
				_directoryMode = true;
				_lastDirectory = _directory;
				listDirectories("");
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
			var textContent = getContent();
			var lookfor = getLookForCharacter(textContent);
			var text = getLastParameter(lookfor, textContent);

			if ((textContent.EndsWith(lookfor) && (textContent.Count(x => x.Equals('"')) % 2) == 0) || textContent.Trim().Length == 0)
				_directoryMode = false;

			var dir = text;
			if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
            	dir = text.TrimStart(new char[] { Path.DirectorySeparatorChar });
            if (!Directory.Exists(dir) && text.StartsWith(Path.DirectorySeparatorChar.ToString()))
                dir = Path.Combine(_directory, dir.TrimEnd(new char[] { Path.DirectorySeparatorChar }));
            if (!dir.EndsWith(":") && Directory.Exists(dir))
            {
                _directoryMode = true;
				_lastDirectory = dir;
            }
			if (_directoryMode)
			{
				if (_lastDirectory == _directory)
					dir = Path.Combine(_lastDirectory, dir);
				listDirectories(dir);
				return;
			}

            var path = "/" + textContent.Trim().Replace(' ', '/');
            _builder.NavigateTo(path);
            listOptions();
        }
		
		private void listDirectories(string filter)
		{
			informationList.Items.Clear();
            Directory.GetDirectories(_lastDirectory)
				.Where(x => x.StartsWith(filter)).ToList()
                .ForEach(x => getFileName(_lastDirectory, x, Path.DirectorySeparatorChar.ToString()));
            Directory.GetFiles(_lastDirectory)
				.Where(x => x.StartsWith(filter)).ToList()
                .ForEach(x => getFileName(_lastDirectory, x, ""));
		}

		private string[] splitParams(string content)
		{
			var list = new List<string>();
			var separator = ' ';
			var word = "";
			for (int i = 0; i < content.Length; i++)
			{
				if (
					(content[i] == ' ' && separator == ' ') ||
					(content[i] == '"' && content[i] == '"') ||
					(word.Length == 0 && content[i] == ' ') ||
					(word.Length == 0 && content[i] == '"'))
				{
					if (word.Length > 0)
					{
						list.Add(word);
					}
					word = "";
					separator = content[i];
				}
				else
				{
					word += content[i].ToString();
				}
			}
			if (word.Length > 0)
				list.Add(word);
			return list.ToArray();
		}

		private string getContent()
		{
			var position = textBoxSearch.SelectionStart;
			return textBoxSearch.Text.Substring(0, position);
		}

		private string getLookForCharacter(string textContent)
		{
			var lookfor = " ";
			var quoteCount = textContent.Count(x => x.Equals('"'));
			if (quoteCount > 0 && (quoteCount % 2) == 0)
				lookfor = "\"";
			return lookfor;
		}

		private string getLastParameter(string lookfor, string textContent)
		{
            var last = textContent.LastIndexOf(lookfor);
            if (last == -1)
                last = 0;
            else
                last++;
            return textContent 
                .Substring(last, textContent.Length - last).Trim(new char[] { ' ', '"' });
		}

		private string getFirstParameter(string lookfor, string textContent)
		{
			var first = textContent.IndexOf(lookfor);
            if (first == -1)
                return textContent;
            else
                return textContent.Substring(0, first);
		}

		private string getLastWord()
		{
			var content = getContent();
			var lookfor = getLookForCharacter(content);
			return getLastParameter(lookfor, content);
		}

        private ListViewItem getFileName(string dir, string x, string tail)
        {
            return informationList.Items.Add(x.Substring(dir.Length, x.Length - dir.Length).TrimStart(new char[] { Path.DirectorySeparatorChar }) + tail);
        }

        private void informationList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (informationList.SelectedItems.Count == 0)
                return;
            labelDescription.Text = _builder.Describe("/" + informationList.SelectedItems[0].Text.Replace(" ", "/"));
        }
    }
}
