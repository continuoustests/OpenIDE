using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using OpenIDE.CodeEngine.Core.Commands;

namespace OpenIDE.CodeEngine.Core.Tests.Commands
{
	[TestFixture]
	public class QueryArgumentParserTests
	{
		private QueryArgumentParser _parser;
		
		[SetUp]
		public void Setup()
		{
			_parser = new QueryArgumentParser();
		}
		
		[Test]
		public void When_passed_an_invalid_query_it_null()
		{
			Assert.That(_parser.Parse(""), Is.Null);
		}

		[Test]
		public void When_given_a_simple_json_it_will_parse_it()
		{
			Assert.That(
				arg(_parser.Parse("level=class"), "level"),
				Is.EqualTo("class"));
		}

		[Test]
		public void When_given_multiple_properties_it_will_parse_them()
		{
			var args = _parser.Parse("level=class,name=it's name");
			Assert.That(arg(args, "level"), Is.EqualTo("class"));
			Assert.That(arg(args, "name"), Is.EqualTo("it's name"));
		}
		
		[Test]
		public void When_given_a_json_with_special_characters_it_is_escaped_by_quotes()
		{
			Assert.That(
				arg(_parser.Parse("level = \"class , with = some funny chars\""), "level"),
				Is.EqualTo("class , with = some funny chars"));
		}

		[Test]
		public void When_given_a_json_with_escaped_quotes_in_quotes()
		{
			Assert.That(
				arg(_parser.Parse("level = \"class \\\"\""), "level"),
				Is.EqualTo("class \""));
		}

		private string arg(List<KeyValuePair<string,string>> list, string name)
		{
			return list.FirstOrDefault(x => x.Key.Equals(name)).Value;
		}
	}
}
