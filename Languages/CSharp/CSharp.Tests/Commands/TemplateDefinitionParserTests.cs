using System;
using System.Linq;
using NUnit.Framework;
using CSharp.Commands;

namespace CSharp.Tests.Commands
{
	[TestFixture]
	public class TemplateDefinitionParserTests
	{
		private TemplateDefinitionParser _parser;
		
		[SetUp]
		public void Setup()
		{
			_parser = new TemplateDefinitionParser();
		}
		
		[Test]
		public void When_only_containing_descriptions_it_should_parse_description_only()
		{
			var parameters = _parser.Parse("name", "This is a description");
			Assert.That(parameters.Name, Is.EqualTo("name"));
			Assert.That(parameters.Description, Is.EqualTo("This is a description"));
		}

		[Test]
		public void When_passing_more_parameters_it_will_return_these_as_sub_parameters()
		{
			var parameters = _parser.Parse(
				"name",
				"This is a description=>FirstParam|First description||[Second]|Second description");

			Assert.That(parameters.Name, Is.EqualTo("name"));
			Assert.That(parameters.Description, Is.EqualTo("This is a description"));

			Assert.That(parameters.Parameters.ElementAt(0).Name, Is.EqualTo("FirstParam"));
			Assert.That(parameters.Parameters.ElementAt(0).Description, Is.EqualTo("First description"));
			Assert.That(parameters.Parameters.ElementAt(0).Required, Is.True);
			Assert.That(parameters.Parameters.ElementAt(0).Parameters.ElementAt(0).Name, Is.EqualTo("Second"));
			Assert.That(parameters.Parameters.ElementAt(0).Parameters.ElementAt(0).Description, Is.EqualTo("Second description"));
			Assert.That(parameters.Parameters.ElementAt(0).Parameters.ElementAt(0).Required, Is.False);
		}
	}
}
