using System;
using NUnit.Framework;
using OpenIDE.Bootstrapping;
using OpenIDE.Arguments;
using OpenIDE.Core.Language;

namespace OpenIDE.Tests.Bootstrapping
{
	[TestFixture]
	public class AppSettingsTests
	{
		private AppSettings _appSettings;
		
		[SetUp]
		public void Setup()
		{
			_appSettings = new AppSettings(
				"",
				() => { return new ICommandHandler[] { new Fake_CommandHandler() }; },
				() => { return new ICommandHandler[] { new Fake_LanguageCommandHandler() }; });
		}
		
		[Test]
		public void When_no_parameters_return_what_was_passed_in()
		{
			var args = _appSettings.Parse(new[]
				{
					"arg1",
					"arg2",
					"arg3"
				});

			Assert.That(args.Length, Is.EqualTo(3));
			Assert.That(args[0], Is.EqualTo("arg1"));
			Assert.That(args[1], Is.EqualTo("arg2"));
			Assert.That(args[2], Is.EqualTo("arg3"));
		}

		[Test]
		public void When_passing_default_language_it_will_set_default_language_and_remove_from_params()
		{
			var args = _appSettings.Parse(new[]
				{
					"arg1",
					"--default-language=C#"
				});

			Assert.That(args.Length, Is.EqualTo(1));
			Assert.That(args[0], Is.EqualTo("arg1"));
			Assert.That(_appSettings.DefaultLanguage, Is.EqualTo("C#"));
		}

		[Test]
		public void When_parsing_a_command_it_should_default_to_the_default_language_if_not_in_original_commands()
		{
			var args = _appSettings.Parse(new[]
				{
					"subcommand",
					"--default-language=fake_language"
				});

			Assert.That(_appSettings.DefaultLanguage, Is.EqualTo("fake_language"));
			Assert.That(args.Length, Is.EqualTo(2));
			Assert.That(args[0], Is.EqualTo("fake_language"));
			Assert.That(args[1], Is.EqualTo("subcommand"));
		}

		[Test]
		public void When_parsing_a_command_it_should_to_original_commands()
		{
			var args = _appSettings.Parse(new[]
				{
					"mycommand",
					"subcommand",
					"--default-language=fake_language"
				});

			Assert.That(_appSettings.DefaultLanguage, Is.EqualTo("fake_language"));
			Assert.That(args.Length, Is.EqualTo(2));
			Assert.That(args[0], Is.EqualTo("mycommand"));
			Assert.That(args[1], Is.EqualTo("subcommand"));
		}
	}

	class Fake_CommandHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get { 
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"description");
				usage.Add("subcommand", "description");
				return usage;
			}
		}

		public string Command { get { return "mycommand"; } }

		public void Execute(string[] arguments)
		{
		}
	}

	class Fake_LanguageCommandHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get { 
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"description");
				usage.Add("subcommand", "description");
				return usage;
			}
		}

		public string Command { get { return "fake_language"; } }

		public void Execute(string[] arguments)
		{
		}
	}
}
