using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenIDENet.Arguments;
using OpenIDENet.CommandBuilding;
using OpenIDENet.Languages;

namespace OpenIDENet.Tests.CommandBuilding
{
	public abstract class CommandBuilderTests
	{
		protected CommandBuilder _builder;

		[SetUp]
		public void Setup()
		{
			var commands = new List<CommandHandlerParameter>();
			commands.Add(new CommandHandlerParameter(
				SupportedLanguage.CSharp,
				CommandType.FileCommand,
				"Option1",
				"Opt1 desc"));
			commands[0].Add("opt1sub1", "desc");

			commands.Add(new CommandHandlerParameter(
				SupportedLanguage.CSharp,
				CommandType.FileCommand,
				"Option2",
				"Opt2 desc"));
			commands[1].Add("opt2sub1", "desc");
			commands[1].Add("opt2sub2", "desc");

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
		public void current_is_null() {
			Assert.That(_builder.Current, Is.Null);
		}

		[Test]
		public void path_is_null() {
			Assert.That(_builder.Path, Is.Null);
		}

		[Test]
		public void available_commands_are_first_level_of_commands() {
			Assert.That(_builder.AvailableCommands.Length, Is.EqualTo(2));
			Assert.That(_builder.AvailableCommands[0].Name, Is.EqualTo("Option1"));
			Assert.That(_builder.AvailableCommands[1].Name, Is.EqualTo("Option2"));
		}
	}

	[TestFixture]
	public class When_moving_to_invalid_option : CommandBuilderTests
	{
		protected override void initialize() {
		}

		[Test]
		[ExpectedException(typeof(InvalidCommandException))]
		public void it_throws_and_exception() {
			_builder.Select("InvalidOption");
		}
	}

	[TestFixture]
	public class When_moving_to_option2 : CommandBuilderTests
	{
		protected override void initialize() {
			_builder.Select("Option2");
		}

		[Test]
		public void current_is_null() {
			Assert.That(_builder.Current.Name, Is.EqualTo("Option2"));
		}

		[Test]
		public void path_is_null() {
			Assert.That(_builder.Path, Is.EqualTo("/Option2"));
		}

		[Test]
		public void available_commands_are_first_level_of_commands() {
			Assert.That(_builder.AvailableCommands.Length, Is.EqualTo(2));
			Assert.That(_builder.AvailableCommands[0].Name, Is.EqualTo("opt2sub1"));
			Assert.That(_builder.AvailableCommands[1].Name, Is.EqualTo("opt2sub2"));
		}
	}
}
