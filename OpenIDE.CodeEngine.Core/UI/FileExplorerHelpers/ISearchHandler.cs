using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenIDE.Core.Caching;

namespace OpenIDE.CodeEngine.Core.UI.FileExplorerHelpers
{
	public interface ISearchHandler
	{
		void ListFromSearch(string expression);
		string GetDescription(TreeNode node);
		void OneUp(TreeNode node);
		void BeforeExpand(TreeNode node);
		FilePosition PositionFromnode(TreeNode node);
        void Run(TreeNode node);
	}
}
