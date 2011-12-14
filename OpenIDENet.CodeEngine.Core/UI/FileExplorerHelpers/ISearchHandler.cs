using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenIDENet.CodeEngine.Core.Crawlers;

namespace OpenIDENet.CodeEngine.Core.UI.FileExplorerHelpers
{
	interface ISearchHandler
	{
		void ListFromSearch(string expression);
		string GetDescription(TreeNode node);
		void OneUp(TreeNode node);
		void BeforeExpand(TreeNode node);
		FilePosition PositionFromnode(TreeNode node);
	}
}
