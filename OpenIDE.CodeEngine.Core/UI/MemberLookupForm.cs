using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenIDE.CodeEngine.Core.Caching;

namespace OpenIDE.CodeEngine.Core.UI
{
    public partial class MemberLookupForm : Form
    {
		class Member
		{
			public string Name { get; private set; }
			public string Description { get; private set; }

			public Member(string member)
			{
				var split = member.IndexOf("|");
				if (split == -1)
					return;
				Name = member.Substring(0, split);
				Description = 
					member.Substring(
							split + 1,
							member.Length - (split + 1))
						.Replace("[[newline]]", Environment.NewLine)
                        .Replace("\t", "    ");
			}
		}

		private List<Member> _members;

        public MemberLookupForm(string[] members)
        {
            InitializeComponent();
            initControls();
			Refresh();
			_members = members
				.Select(x => new Member(x))
				.OrderBy(x => x.Name).ToList();
			listMembers();
        }

        void initControls()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                labelComment.Location = new System.Drawing.Point(12, 360);
            else
                labelComment.Location = new System.Drawing.Point(12, 373);

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                informationList.Size = new System.Drawing.Size(526, 291);
            else
                informationList.Size = new System.Drawing.Size(526, 304);
        }

		void HandleHandleFormClosing (object sender, FormClosingEventArgs e)
        {
			Visible = false;
			Dispose();
        }

		private void listMembers()
		{
			informationList.Items.Clear();
			var pattern = textBoxSearch.Text;
			if (pattern.Trim() == "")
				_members.ForEach(x => addItem(x));
			else
				_members
					.Where(x => 
						x.Name.ToLower().Contains(pattern.ToLower()) ||
						x.Description.ToLower().Contains(pattern.ToLower()))
					.OrderBy(x => new SearchSorter().Sort(x.Name, x.Description, pattern)).ToList()
					.ForEach(x => addItem(x));
			if (informationList.Items.Count == 0)
				return;
			if (informationList.SelectedItems.Count != 1)
				informationList.Items[0].Selected = true;
			displayCurrentComment();
		}

		private void addItem(Member member)
		{
			var item = informationList.Items.Add(member.Name);
			item.Tag = member;
		}

        void textBoxSearch_TextChanged(object sender, System.EventArgs e)
        {
			listMembers();
        }

        void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
			if (e.KeyCode == Keys.Enter)
			{
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
				displayCurrentComment();
			}
			else if (e.KeyCode == Keys.Up)
			{
				if (informationList.Items.Count == 0)
					return;
				if (informationList.SelectedItems.Count != 1)
					return;
				if (informationList.SelectedItems[0].Index != 0)
					informationList.Items[informationList.SelectedItems[0].Index - 1].Selected = true;
				displayCurrentComment();
			}
			else if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
        }

		private void displayCurrentComment()
		{
			if (informationList.SelectedItems.Count != 1)
				return;
			var member = (Member)informationList.SelectedItems[0].Tag;
			labelComment.Text = member.Description;
		}
    }
}
