using System;
namespace OpenIDE.Core.UI
{
    partial class SnippetForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelInfo = new System.Windows.Forms.Label();
            this.textBoxPlaceholders = new System.Windows.Forms.TextBox();
            this.textBoxPreview = new System.Windows.Forms.TextBox();
            this.buttonRun = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInfo.Location = new System.Drawing.Point(12, 1);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(648, 21);
            this.labelInfo.TabIndex = 6;
            this.labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxPlaceholders
            // 
            this.textBoxPlaceholders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPlaceholders.Location = new System.Drawing.Point(12, 25);
            this.textBoxPlaceholders.Name = "textBoxPlaceholders";
            this.textBoxPlaceholders.Size = new System.Drawing.Size(595, 20);
            this.textBoxPlaceholders.TabIndex = 1;
            this.textBoxPlaceholders.TextChanged += new System.EventHandler(this.textBoxPlaceholders_TextChanged);
            this.textBoxPlaceholders.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxPlaceholders_KeyDown);
            // 
            // textBoxPreview
            // 
			this.textBoxPreview.Multiline = true;
            this.textBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPreview.Location = new System.Drawing.Point(12, 51);
            this.textBoxPreview.Name = "textBoxPreview";
            this.textBoxPreview.Size = new System.Drawing.Size(648, 250);
            this.textBoxPreview.TabIndex = 3;
            this.textBoxPreview.TextChanged += new System.EventHandler(this.textBoxPreview_textChanged);
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.Location = new System.Drawing.Point(613, 25);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(47, 20);
            this.buttonRun.TabIndex = 2;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = false;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // RunCommandForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(672, 322);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.textBoxPlaceholders);
            this.Controls.Add(this.textBoxPreview);
            this.KeyPreview = true;
            this.Name = "RunCommandForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Complete Snippet";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.TextBox textBoxPlaceholders;
        private System.Windows.Forms.TextBox textBoxPreview;
        private System.Windows.Forms.Button buttonRun;
    }
}
