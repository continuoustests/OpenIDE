using System;
using NUnit.Framework;
using OpenIDENet.Bootstrapping;

namespace OpenIDENet.Tests.Bootstrapping
{
	[TestFixture]
	public class AppSettingsTests
	{
		private AppSettings _appSettings;
		
		[SetUp]
		public void Setup()
		{
			_appSettings = new AppSettings();
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
	}
}
