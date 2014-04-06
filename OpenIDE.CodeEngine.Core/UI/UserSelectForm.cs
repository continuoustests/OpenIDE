using System;
using System.Linq;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

namespace OpenIDE.CodeEngine.Core.UI
{
    public class UserSelectForm : Form
    {
        private bool _onSelectCalled = false;
        private List<string> _items = new List<string>();
        private Action<string> _onSelect;

        public UserSelectForm(IEnumerable<string> items, Action<string> onSelect) {
            initialize();
            _onSelect = onSelect;
            _items.AddRange(items);
            populateList(_items);
        }

        void populateList(IEnumerable<string> items) {
            informationList.Items.Clear();
            foreach (var item in items)
                informationList.Items.Add(item);
            if (informationList.Items.Count > 0)
                informationList.Items[0].Selected = true;
        }

        void HandleHandleFormClosing (object sender, FormClosingEventArgs e) {
            if (_onSelectCalled)
                _onSelect(null);
            Dispose();
        }

        void HandleTextBoxSearchhandleTextChanged(object sender, System.EventArgs e)
        {
            populateList(_items.Where(x => x.Contains(textBoxSearch.Text.Trim())));
        }

        void HandleTextBoxSearchhandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (informationList.SelectedItems.Count != 1)
                    return;
                _onSelect(informationList.SelectedItems[0].Text);
                _onSelectCalled = true;
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
                Close();
            }
        }

        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.ListView informationList;
        private System.Windows.Forms.ColumnHeader columnHeader1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void initialize()
        {
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.informationList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            
            //
            // textBoxSearch
            //
            this.textBoxSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSearch.Location = new System.Drawing.Point(12, 5);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(300, 45);
            this.textBoxSearch.TabIndex = 1;
            this.textBoxSearch.TextChanged += HandleTextBoxSearchhandleTextChanged;
            this.textBoxSearch.KeyDown += HandleTextBoxSearchhandleKeyDown;
            // 
            // informationList
            // 
            this.informationList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom)));
            this.informationList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {this.columnHeader1});
            this.informationList.Location = new System.Drawing.Point(12, 25);
            this.informationList.Name = "informationList";
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                this.informationList.Size = new System.Drawing.Size(300, 320);
            else
                this.informationList.Size = new System.Drawing.Size(300, 304);
            this.informationList.TabIndex = 2;
            this.informationList.UseCompatibleStateImageBehavior = false;
            this.informationList.View = System.Windows.Forms.View.Details;
            this.informationList.HideSelection = false;
            this.informationList.MultiSelect = false;
            this.informationList.FullRowSelect = true;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Value";
            this.columnHeader1.Width = 320;

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 362);
            this.Controls.Add(this.textBoxSearch);
            this.Controls.Add(this.informationList);
            this.KeyPreview = true;
            this.Name = "UserSelectForm";
            this.Text = "Select value";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();
            this.FormClosing += HandleHandleFormClosing;
        }
    }
}