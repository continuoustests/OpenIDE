using System;
using System.Linq;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

namespace OpenIDE.CodeEngine.Core.UI
{
    public class UserInputForm : Form
    {
        private bool _onSelectCalled = false;
        private Action<string> _onSelect;

        public UserInputForm(string prefilled, Action<string> onSelect) {
            initialize();
            _onSelect = onSelect;
            textBox.Text = prefilled;
        }

        void HandleHandleFormClosing (object sender, FormClosingEventArgs e) {
            if (_onSelectCalled)
                _onSelect(null);
            Dispose();
        }

        void HandleTextBoxhandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _onSelect(textBox.Text);
                _onSelectCalled = true;
                Close();
            }
        }

        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox textBox;

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
            this.textBox = new System.Windows.Forms.TextBox();
            
            //
            // textBox
            //
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.Location = new System.Drawing.Point(12, 5);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(300, 45);
            this.textBox.TabIndex = 1;
            this.textBox.KeyDown += HandleTextBoxhandleKeyDown;

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 55);
            this.Controls.Add(this.textBox);
            this.KeyPreview = true;
            this.Name = "UserInputForm";
            this.Text = "Type a value";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();
            this.FormClosing += HandleHandleFormClosing;
        }
    }
}