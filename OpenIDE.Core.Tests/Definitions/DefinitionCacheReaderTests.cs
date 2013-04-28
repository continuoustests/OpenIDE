using System;
using System.Text;
using NUnit.Framework;
using OpenIDE.Core.Definitions;

namespace OpenIDE.Core.Tests.Definitions
{
	[TestFixture]
	public class DefinitionCacheReaderTests
	{
		[Test]
		public void When_reading_an_invalid_file_it_will_return_null() {
			var reader = 
				new DefinitionCacheReader()
					.ReadUsing((file) => "invalid content.");
			Assert.That(reader.Read("/file"), Is.Null);
		}

		[Test]
		public void Can_read_definition_cache_file() {
			var reader = 
				new DefinitionCacheReader()
					.ReadUsing((file) => fileContent());
			var cache = reader.Read("/file");
			Assert.That(cache.Definitions.Length, Is.EqualTo(1));
			Assert.That(cache.Definitions[0].Type, Is.EqualTo(DefinitionCacheItemType.Language));
			Assert.That(cache.Definitions[0].Location, Is.EqualTo("/my/script"));
			Assert.That(cache.Definitions[0].Updated, Is.EqualTo(new DateTime(2013,1,1,2,3,1)));
			Assert.That(cache.Definitions[0].Required, Is.True);
			Assert.That(cache.Definitions[0].Name, Is.EqualTo("mycmd"));
			Assert.That(cache.Definitions[0].Description, Is.EqualTo("My command does my stuff."));
			Assert.That(cache.Definitions[0].Parameters.Count, Is.EqualTo(1));
			Assert.That(cache.Definitions[0].Parameters[0].Type, Is.EqualTo(DefinitionCacheItemType.Language));
			Assert.That(cache.Definitions[0].Parameters[0].Location, Is.EqualTo("/my/script"));
			Assert.That(cache.Definitions[0].Parameters[0].Updated, Is.EqualTo(new DateTime(2013,1,1,2,3,1)));
			Assert.That(cache.Definitions[0].Parameters[0].Required, Is.True);
			Assert.That(cache.Definitions[0].Parameters[0].Name, Is.EqualTo("FILE"));
			Assert.That(cache.Definitions[0].Parameters[0].Description, Is.EqualTo("another param"));
			Assert.That(cache.Definitions[0].Parameters[0].Parameters.Count, Is.EqualTo(1));
			Assert.That(cache.Definitions[0].Parameters[0].Parameters[0].Type, Is.EqualTo(DefinitionCacheItemType.Language));
			Assert.That(cache.Definitions[0].Parameters[0].Parameters[0].Location, Is.EqualTo("/my/script"));
			Assert.That(cache.Definitions[0].Parameters[0].Parameters[0].Updated, Is.EqualTo(new DateTime(2013,1,1,2,3,1)));
			Assert.That(cache.Definitions[0].Parameters[0].Parameters[0].Required, Is.False);
			Assert.That(cache.Definitions[0].Parameters[0].Parameters[0].Name, Is.EqualTo("optional"));
			Assert.That(cache.Definitions[0].Parameters[0].Parameters[0].Description, Is.EqualTo("This one is optional"));
			Assert.That(cache.Definitions[0].Parameters[0].Parameters[0].Parameters.Count, Is.EqualTo(0));
		}

		private string fileContent() {
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
			append(sb, "              'type': 'language',");
			append(sb, "              'location': '/my/script',");
			append(sb, "              'updated': '2013.01.01 02:03:01'");
			append(sb, "            }");
			append(sb, "          ],");
			append(sb, "          'cmd': 'FILE',");
			append(sb, "          'description': 'another param',");
			append(sb, "          'type': 'language',");
			append(sb, "          'location': '/my/script',");
			append(sb, "          'updated': '2013.01.01 02:03:01'");
			append(sb, "        }");
			append(sb, "      ],");
			append(sb, "      'cmd': 'mycmd',");
			append(sb, "      'description': 'My command does my stuff.',");
			append(sb, "      'type': 'language',");
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