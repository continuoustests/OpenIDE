using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenIDENet.UI
{
    public partial class RunCommandForm : Form
    {	
        public RunCommandForm()
        {
            InitializeComponent();
        }
		
		void HandleHandleFormClosing (object sender, FormClosingEventArgs e)
        {
			Visible = false;
			Dispose();
        }
		
		void HandleTextBoxSearchhandleTextChanged(object sender, System.EventArgs e)
        {
        }
		
		void HandleTextBoxSearchhandleKeyDown(object sender, KeyEventArgs e)
        {
			if (e.KeyCode == Keys.Enter)
			{
			}
			else if (e.KeyCode == Keys.Down)
			{
			}
			else if (e.KeyCode == Keys.Up)
			{
			}
			else if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
        }
    }
}
