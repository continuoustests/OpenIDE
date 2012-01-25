using System;
namespace OpenIDENet.UI
{
    partial class RunCommandForm
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
            this.labelDescription = new System.Windows.Forms.Label();
            this.buttonRun = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.Location = new System.Drawing.Point(12, 1);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(648, 21);
            this.labelInfo.TabIndex = 6;
            this.labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSearch.Location = new System.Drawing.Point(12, 25);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(595, 20);
            this.textBoxSearch.TabIndex = 1;
            this.textBoxSearch.TextChanged += new System.EventHandler(this.textBoxSearch_TextChanged);
            this.textBoxSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSearch_KeyDown);
            // 
            // informationList
            // 
            this.informationList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.informationList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.informationList.HideSelection = false;
            this.informationList.Location = new System.Drawing.Point(12, 51);
            this.informationList.MultiSelect = false;
            this.informationList.Name = "informationList";
            this.informationList.Size = new System.Drawing.Size(648, 166);
            this.informationList.TabIndex = 3;
            this.informationList.UseCompatibleStateImageBehavior = false;
            this.informationList.View = System.Windows.Forms.View.Details;
            this.informationList.SelectedIndexChanged += new System.EventHandler(this.informationList_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Command Options";
            this.columnHeader1.Width = 621;
            // 
            // labelDescription
            // 
            this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom 
            | System.Windows.Forms.AnchorStyles.Left 
            | System.Windows.Forms.AnchorStyles.Right));
            this.labelDescription.Location = new System.Drawing.Point(13, 224);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(647, 89);
            this.labelDescription.TabIndex = 4;
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top));
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
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.textBoxSearch);
            this.Controls.Add(this.informationList);
            this.KeyPreview = true;
            this.Name = "RunCommandForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Run Command";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.ListView informationList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Button buttonRun;
    }
}
