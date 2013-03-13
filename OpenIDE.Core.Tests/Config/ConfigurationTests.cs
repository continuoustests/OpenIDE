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
			if (File.Exists(_config.ConfigurationFile))
				File.Delete(_config.ConfigurationFile);
		}

		[Test]
		public void Can_write_default_language()
		{
			_config.Write("default.language=C#");
			configIs("default.language=C#");
			_config.Delete("default.language");
			configIs("");
		}

		[Test]
		public void When_given_add_operator_it_will_add_to_setting()
		{
			_config.Write("enabled.languages=C#");
			_config.Write("enabled.languages+=js");
			configIs("enabled.languages=C#,js");
		}

		[Test]
		public void When_given_remove_operator_it_will_remove_given_setting()
		{
			_config.Write("enabled.languages=Cobol,C#,js");
			_config.Write("enabled.languages-=C#");
			configIs("enabled.languages=Cobol,js");
		}
		
		[Test]
		public void When_given_removing_all_items_it_will_remove_the_setting()
		{
			_config.Write("enabled.languages=C#,js");
			_config.Write("enabled.languages-=C#");
			_config.Write("enabled.languages-=js");
			configIs("");
		}
		
		[Test]
		public void When_adding_to_non_existing_setting_it_will_act_as_new()
		{
			_config.Write("enabled.languages+=C#");
			configIs("enabled.languages=C#");
		}

		[Test]
		public void When_given_removing_element_from_non_existing_setting_it_will_do_nothing()
		{
			_config.Write("enabled.languages-=C#");
			configIs("");
		}

		[Test]
		public void When_given_an_unexisting_setting_we_get_null_returned() 
		{
			var setting = _config.Get("mytest.setting");
			Assert.That(setting, Is.Null);
		}

		[Test]
		public void When_given_an_exact_setting_we_can_read_it() 
		{
			_config.Write("mytest.setting.with.more=20");
			_config.Write("mytest.setting=15");
			var setting = _config.Get("mytest.setting");
			Assert.That(setting.Key, Is.EqualTo("mytest.setting"));
			Assert.That(setting.Value, Is.EqualTo("15"));
		}
		
		[Test]
		public void When_given_settings_starting_with_a_token_we_can_fetch_them() 
		{
			_config.Write("mytest.setting.with.more=20");
			_config.Write("		 mytest.setting=15");
			var settings = _config.GetSettingsStartingWith("mytest.setting");
			Assert.That(settings.Length, Is.EqualTo(2));
			Assert.That(settings[0].Key, Is.EqualTo("mytest.setting.with.more"));
			Assert.That(settings[0].Value, Is.EqualTo("20"));
			Assert.That(settings[1].Key, Is.EqualTo("mytest.setting"));
			Assert.That(settings[1].Value, Is.EqualTo("15"));
		}

		[Test]
		public void When_splitting_values_of_a_setting_with_no_value_returns_an_empty_list()
		{
			Assert.That(
				new ConfigurationSetting("key", null).SplitBy(",").Length,
				Is.EqualTo(0));
		}

		[Test]
		public void When_splitting_values_it_will_return_array_of_values()
		{
			var values = new ConfigurationSetting("key", "1,2,3").SplitBy(",");
			Assert.That(values.Length, Is.EqualTo(3));
			Assert.That(values[0], Is.EqualTo("1"));
			Assert.That(values[1], Is.EqualTo("2"));
			Assert.That(values[2], Is.EqualTo("3"));
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
