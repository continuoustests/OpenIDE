using System;
using OpenIDE.Core.Language;
using OpenIDE.Core.Definitions;

namespace OpenIDE.Core.Commands
{
	public class UsagePrinter
	{
		public static void PrintDefinition(DefinitionCacheItem item) {
			int level = 0;
			PrintDefinition(item, ref level);
		}
		
		public static void PrintDefinition(DefinitionCacheItem item, ref int level) {
			level++;
			var name = item.Name;
			if (!item.Required)
				name = "[" + name + "]";
			Console.WriteLine("{0}{1} : {2}", "".PadLeft(level, '\t'), name, item.Description);
			foreach (var child in item.Parameters)
				PrintDefinition(child, ref level);
			level--;
		}
	}
}