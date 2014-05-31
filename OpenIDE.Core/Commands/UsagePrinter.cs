using System;
using System.Linq;
using System.Collections.Generic;
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
			PrintSingleDefinition(item, level);
			foreach (var child in item.Parameters)
				PrintDefinition(child, ref level);
			level--;
		}


		public static void PrintSingleDefinition(DefinitionCacheItem item)
		{
			PrintSingleDefinition(item, 0);
		}

		public static void PrintSingleDefinition(DefinitionCacheItem item, int level)
		{
			var name = item.Name;
			if (!item.Required)
				name = "[" + name + "]";
			Console.WriteLine("{0}{1} - {2}", "".PadLeft(level, '\t'), name, item.Description);
		}

		public static void PrintDefinitionsAligned(IEnumerable<DefinitionCacheItem> items)
		{
			if (items.Count() == 0)
				return;
			var maxLength = items.Max(x => x.Name.Length);
			foreach (var item in items) {
				var name = item.Name;
				if (!item.Required)
					name = "[" + name + "]";
				name = name.PadRight(maxLength, ' ');
				Console.WriteLine("{0} - {1}", name, item.Description);
			}
		}

		public static void PrintDefinitionNames(IEnumerable<DefinitionCacheItem> items)
		{
			if (items.Count() == 0)
				return;
			foreach (var item in items) {
				var name = item.Name;
				if (!item.Required)
					name = "[" + name + "]";
				Console.WriteLine("{0}", name);
			}
		}
	}
}