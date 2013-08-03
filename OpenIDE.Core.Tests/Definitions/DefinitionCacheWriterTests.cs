using System;
using System.Text;
using NUnit.Framework;
using OpenIDE.Core.Language;
using OpenIDE.Core.Definitions;

namespace OpenIDE.Core.Tests.Definitions
{
	[TestFixture]
	public class DefinitionCacheWriterTests
	{
		[Test]
		public void Can_write_definition_cache()
		{
			string actualJSON = null;
			var writer = 
				new DefinitionCacheWriter("/write/path")
					.WriteUsing((file,content) => actualJSON = content);

			var type = DefinitionCacheItemType.Script;
			var script = "/my/script";
			var time = new DateTime(2013,1,1,2,3,1);
			var cache = new DefinitionCache();
			cache
				.Add(type, script, time, true, "mycmd", "My command does my stuff.")
					.Add(type, script, time, true, "FILE", "another param")
						.Add(type, script, time, false, "optional", "This one is optional");
			
			writer.Write(cache);
			Assert.That(actualJSON, Is.EqualTo(expectedJSON()));
		}

		private string expectedJSON() {
			var sb = new StringBuilder();
			append(sb, "{");
			append(sb, "  'commands': [");;
			append(sb, "    {");
			append(sb, "      'arguments': [");
			append(sb, "        {");
			append(sb, "          'arguments': [");
			append(sb, "            {");
			append(sb, "              'arguments': [],");
			append(sb, "              'cmd': '[optional]',");
			append(sb, "              'description': 'This one is optional',");
			append(sb, "              'type': 'script',");
			append(sb, "              'location': '/my/script',");
			append(sb, "              'updated': '2013.01.01 02:03:01'");
			append(sb, "            }");
			append(sb, "          ],");
			append(sb, "          'cmd': 'FILE',");
			append(sb, "          'description': 'another param',");
			append(sb, "          'type': 'script',");
			append(sb, "          'location': '/my/script',");
			append(sb, "          'updated': '2013.01.01 02:03:01'");
			append(sb, "        }");
			append(sb, "      ],");
			append(sb, "      'cmd': 'mycmd',");
			append(sb, "      'description': 'My command does my stuff.',");
			append(sb, "      'type': 'script',");
			append(sb, "      'location': '/my/script',");
			append(sb, "      'updated': '2013.01.01 02:03:01'");
			append(sb, "    }");
			append(sb, "  ]");
			sb.Append("}");
			return sb.ToString();
		}

		private void append(StringBuilder sb, string content) {
			sb.AppendLine(
				string.Format("{0}", 
					content.Replace('\'', '\"')));
		}
	}
}