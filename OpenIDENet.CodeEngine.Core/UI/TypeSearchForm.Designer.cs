using System;
namespace OpenIDENet.CodeEngine.Core.UI
{
    partial class TypeSearchForm
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
			this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.informationList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
			
			//
			// labelInfo
			//
			this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInfo.Location = new System.Drawing.Point(12, 2);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(760, 21);
            this.labelInfo.TabIndex = 3;
			this.labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textBoxSearch
			//
			this.textBoxSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSearch.Location = new System.Drawing.Point(12, 25);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(760, 45);
            this.textBoxSearch.TabIndex = 1;
			this.textBoxSearch.TextChanged += HandleTextBoxSearchhandleTextChanged;
			this.textBoxSearch.KeyDown += HandleTextBoxSearchhandleKeyDown;
            // 
            // informationList
            // 
            this.informationList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom)));
            this.informationList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.informationList.Location = new System.Drawing.Point(12, 57);
            this.informationList.Name = "informationList";
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				this.informationList.Size = new System.Drawing.Size(760, 291);
			else
            	this.informationList.Size = new System.Drawing.Size(760, 304);
            this.informationList.TabIndex = 2;
            this.informationList.UseCompatibleStateImageBehavior = false;
            this.informationList.View = System.Windows.Forms.View.Details;
			this.informationList.HideSelection = false;
			this.informationList.MultiSelect = false;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Type";
            this.columnHeader1.Width = 723;
            // 
            // TypeSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 362);
			this.Controls.Add(this.labelInfo);
			this.Controls.Add(this.textBoxSearch);
            this.Controls.Add(this.informationList);
            this.KeyPreview = true;
            this.Name = "TypeSearchForm";
            this.Text = "Type Search";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();
			this.FormClosing += HandleHandleFormClosing;

        }
        #endregion

		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.ListView informationList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}