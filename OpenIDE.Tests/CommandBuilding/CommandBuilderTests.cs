using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using OpenIDE.Arguments;
using OpenIDE.Core.Definitions;
using OpenIDE.CommandBuilding;
using System.Text;

namespace OpenIDE.Tests.CommandBuilding
{
	public abstract class CommandBuilderTests
	{
		protected CommandBuilder _builder;
		protected Func<List<DefinitionCacheItem>,DefinitionCacheItem,DefinitionCacheItem> _parameterAppender = (parameters, parameter) => {
			parameters.Add(parameter);
			return parameter;
		};


		[SetUp]
		public void Setup()
		{
			var type = DefinitionCacheItemType.Script;
			var commands = new List<DefinitionCacheItem>();
			commands.Add(new DefinitionCacheItem(_parameterAppender) {
					Type = type, Location = "", Updated = DateTime.Now, Required = true, Name = "Option1", Description = "Opt1 desc"
				});
			commands[0].Append(type, "", DateTime.Now, false, true, "opt1sub1", "desc");

			commands.Add(new DefinitionCacheItem(_parameterAppender) {
					Type = type, Location = "", Updated = DateTime.Now, Required = true, Name = "Option2", Description = "Opt2 desc"
				});
			commands[1].Append(type, "", DateTime.Now, false, true, "opt2sub1", "desc");
			commands[1].Append(type, "", DateTime.Now, false, true, "opt2sub2", "desc");

			_builder = new CommandBuilder(commands);
			initialize();
		}

		protected abstract void initialize();
	}

	[TestFixture]
	public class When_constructing_the_command_builder : CommandBuilderTests
	{
		protected override void initialize() { }

		[Test]
		public void available_commands_are_all_commands() {
			Assert.That(_builder.AvailableCommands.Length, Is.EqualTo(3));
			Assert.That(_builder.AvailableCommands[0], Is.EqualTo("/Option1/opt1sub1"));
            Assert.That(_builder.AvailableCommands[1], Is.EqualTo("/Option2/opt2sub1"));
            Assert.That(_builder.AvailableCommands[2], Is.EqualTo("/Option2/opt2sub2"));
		}
	}

	[TestFixture]
	public class When_navigating_to_a_command : CommandBuilderTests
	{
		protected override void initialize() {
			_builder.NavigateTo("/Option2");
		}

		[Test]
		public void its_commands_are_available() {
            Assert.That(_builder.AvailableCommands.Length, Is.EqualTo(2));
            Assert.That(_builder.AvailableCommands[0], Is.EqualTo("/Option2/opt2sub1"));
            Assert.That(_builder.AvailableCommands[1], Is.EqualTo("/Option2/opt2sub2"));
		}
	}

    [TestFixture]
    public class When_navigating_to_invalid_command : CommandBuilderTests
    {
        protected override void initialize() {
            _builder.NavigateTo("/Option2/InvalidCommand");
        }

        [Test]
        public void the_closest_match_will_be_shown() {
            Assert.That(_builder.AvailableCommands.Length, Is.EqualTo(2));
            Assert.That(_builder.AvailableCommands[0], Is.EqualTo("/Option2/opt2sub1"));
            Assert.That(_builder.AvailableCommands[1], Is.EqualTo("/Option2/opt2sub2"));
        }
    }

    [TestFixture]
    public class When_describing_a_valid_path : CommandBuilderTests
    {
        protected override void initialize() {}

        [Test]
        public void the_closest_match_will_be_shown()
        {
            var result = new StringBuilder();
            result.AppendLine("Opt2 desc");
            result.AppendLine("Option2");
            result.AppendLine("    opt2sub1 : desc");
            Assert.That(_builder.Describe("/Option2/opt2sub1"), Is.EqualTo(result.ToString()));
        }
    }

    [TestFixture]
    public class When_describing_an_invalidvalid_path : CommandBuilderTests
    {
        protected override void initialize() { }

        [Test]
        public void the_closest_match_will_be_shown()
        {
            Assert.That(_builder.Describe("/Option2/opt2sub1_invalid"), Is.EqualTo(""));
        }
    }
}
