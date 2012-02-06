using System;
using System.IO;
using System.Reflection;
using OpenIDE.Core.Config;
using NUnit.Framework;

namespace OpenIDE.Core.Tests.Config
{
	[TestFixture]
	public class ConfigurationTests
	{
		private Configuration _config;

		[SetUp]
		public void Setup()
		{
			var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_config = new Configuration(basePath, false);
			var file = Configuration.GetConfigFile(basePath);
			if (file != null)
				File.Delete(file);
			var dir = Path.Combine(basePath, ".OpenIDE");
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			File.WriteAllText(Path.Combine(dir, "oi.config"), "");
		}

		[TearDown]
		public void Teardown()
		{
			File.Delete(_config.ConfigurationFile);
		}

		[Test]
		public void Can_write_default_language()
		{
			_config.Write("default-language=C#");
			configIs("default-language=C#");
			_config.Delete("default-language");
			configIs("");
		}

		[Test]
		public void When_given_add_operator_it_will_add_to_setting()
		{
			_config.Write("enabled-languages=C#");
			_config.Write("enabled-languages+=js");
			configIs("enabled-languages=C#,js");
		}

		[Test]
		public void When_given_remove_operator_it_will_remove_given_setting()
		{
			_config.Write("enabled-languages=Cobol,C#,js");
			_config.Write("enabled-languages-=C#");
			configIs("enabled-languages=Cobol,js");
		}
		
		[Test]
		public void When_given_removing_all_items_it_will_remove_the_setting()
		{
			_config.Write("enabled-languages=C#,js");
			_config.Write("enabled-languages-=C#");
			_config.Write("enabled-languages-=js");
			configIs("");
		}
		
		[Test]
		public void When_adding_to_non_existing_setting_it_will_act_as_new()
		{
			_config.Write("enabled-languages+=C#");
			configIs("enabled-languages=C#");
		}

		[Test]
		public void When_given_removing_element_from_non_existing_setting_it_will_do_nothing()
		{
			_config.Write("enabled-languages-=C#");
			configIs("");
		}

		private void configIs(string content)
		{
			var verified = content + Environment.NewLine;
			if (content == "")
				verified = "";
			Assert.That(
				File.ReadAllText(_config.ConfigurationFile),
				Is.EqualTo(verified));
		}
	}
}
