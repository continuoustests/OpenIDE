using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenIDE.Arguments;
using OpenIDE.CommandBuilding;
using OpenIDE.Core.Logging;
using System.IO;
using System.Diagnostics;
using CoreExtensions;

namespace OpenIDE.UI
{
    public partial class RunCommandForm : Form
    {
        private string _directory;
        private CommandBuilder _builder;

		private bool _directoryMode = false;
		private string _lastDirectory = null;
        private string _additionalParameters;

        public RunCommandForm(string directory, string additionalParameters, CommandBuilder builder)
        {
            InitializeComponent();
            _directory = directory;
            _additionalParameters = additionalParameters;
            if (_additionalParameters == null)
                _additionalParameters = "";
            _builder = builder;
            listOptions();
            if (informationList.Items.Count > 0)
                informationList.Items[0].Selected = true;
            Text = "Running command from " + _directory;
            labelInfo.Text = "Already selected parameters: " + additionalParameters;
			if (additionalParameters != null && additionalParameters.Length > 0)
			{
				textBoxSearch.Text = additionalParameters + " ";
				textBoxSearch.SelectionStart = textBoxSearch.Text.Length;
				textBoxSearch_TextChanged(this, new EventArgs());
			}
        }
		
		void HandleHandleFormClosing (object sender, FormClosingEventArgs e)
        {
			Visible = false;
			Dispose();
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
            if (!selectFirstitem())
                return;

			if ((e.Control && e.KeyCode == Keys.N) || e.KeyCode == Keys.Enter)
			    autoComplete();
			else if (e.Control && e.KeyCode == Keys.Space)
			{
				if (_directoryMode)
					return;
                e.Handled = true;
                e.SuppressKeyPress = true;
                listWorkingDirectory(e);
			}
            else if (e.KeyCode == Keys.Down)
            {
                if (informationList.SelectedItems[0].Index < informationList.Items.Count - 1)
                    informationList.Items[informationList.SelectedItems[0].Index + 1].Selected = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (informationList.SelectedItems[0].Index != 0)
                    informationList.Items[informationList.SelectedItems[0].Index - 1].Selected = true;
            }
        }

        private void autoComplete()
        {
            if (_directoryMode)
                autoCompletePath();
            else
                autoCompleteCommand();
        }

        private void listWorkingDirectory(KeyEventArgs e)
        {
            _directoryMode = true;
            _lastDirectory = _directory;
            listDirectories("");
        }

        private void autoCompleteCommand()
        {
            var selection = informationList.SelectedItems[0].Text;
            var autocomplete = new CommandAutoCompletion()
                .AutoComplete(
                    selection,
                    getContent());
            if (autocomplete.Length == 0)
                return;
            if (!commandEndsWithSpace(selection))
                autocomplete += " ";
            textBoxSearch.SelectedText = autocomplete;
        }

        private bool commandEndsWithSpace(string selection)
        {
            return selection.Length < textBoxSearch.Text.Length &&
				   textBoxSearch.Text.Substring(selection.Length, 1) == " ";
        }

        private void autoCompletePath()
        {
            textBoxSearch.SelectedText = 
                new PathAutoCompletion()
                    .AutoComplete(
                        informationList.SelectedItems[0].Text,
                        getContent());
        }

        private bool selectFirstitem()
        {
            if (informationList.Items.Count == 0)
                return false;
            if (informationList.SelectedItems.Count != 1)
                informationList.Items[0].Selected = true;
            return true;
        }

        private void listOptions()
        {

            informationList.Items.Clear();
            _builder.AvailableCommands.ToList()
                .ForEach(x => informationList.Items.Add(x.Replace("/", " ").Trim()));
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
			var textContent = getContent();
			var lookfor = getLookForCharacter(textContent);
			var text = getLastParameter(lookfor, textContent);

            var dir = preparePath(textContent, lookfor, text);
			if (_directoryMode)
			{
				if (_lastDirectory == _directory)
					dir = Path.Combine(_lastDirectory, dir);
				listDirectories(dir);
				return;
			}
            else
            {
                var path = "/" + textContent.Trim().Replace(' ', '/');
                _builder.NavigateTo(path);
                listOptions();
            }
        }

        private string preparePath(string textContent, string lookfor, string text)
        {
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
            return dir;
        }
		
		private void listDirectories(string filter)
		{
			informationList.Items.Clear();
            Directory.GetDirectories(_lastDirectory)
                .Where(x => matchPath(filter, x)).ToList()
                .ForEach(x => getFileName(_lastDirectory, x, Path.DirectorySeparatorChar.ToString()));
            Directory.GetFiles(_lastDirectory)
                .Where(x => matchPath(filter, x)).ToList()
                .ForEach(x => getFileName(_lastDirectory, x, ""));
		}

        private static bool matchPath(string filter, string x)
        {
            if (filter.Length == 0)
                return true;
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                return x.StartsWith(filter);
            else
                return x.ToLower().StartsWith(filter.ToLower());
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

        private void run(string arguments)
        {
            var proc = new Process();
			proc.Query(
                "oi",
                arguments,
                false,
                _directory,
                (error, line) => {
                        if (error) {
                            Logger.Write("Failed to run oi with " + arguments + " in " + _directory);
                            Logger.Write(line);
                            return;
                        }
                        Console.WriteLine(line);
                    });

            /*if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                proc.StartInfo = new ProcessStartInfo("oi", arguments);
            else
			{
                proc.StartInfo = 
					new ProcessStartInfo(
						"cmd.exe", "/c oi \"" +
						arguments.Replace("\"", "^\"") + "\"");
			}
			Console.WriteLine("Running: " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.WorkingDirectory = _directory;
            proc.Start();
            var output = proc.StandardOutput.ReadToEnd();
            if (output.Length > Environment.NewLine.Length)
                output = output.Substring(0, output.Length - Environment.NewLine.Length);
            Console.WriteLine(output);*/
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            run(textBoxSearch.Text);
            Close();
        }
    }
}
