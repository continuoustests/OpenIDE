using System;
using NUnit.Framework;
using OpenIDE.Core.UI;
namespace OpenIDE.Core.Tests.UI
{
	[TestFixture]
	public class SnippetReplaceControllerTests
	{
		private SnippetReplaceController _controller;
		
		[SetUp]
		public void Setup()
		{
			_controller = new SnippetReplaceController(
				new[]
					{
						"{nr1}",
						"{nr2}",
						"{nr3}",
						"{nr4}"
					},
				getContent());
		}
		
		[Test]
		public void On_init_it_will_show_place_holder_and_original_text()
		{
			Assert.That(_controller.CurrentPlaceholder, Is.EqualTo("{nr1}"));
			Assert.That(_controller.ModifiedSnippet, Is.EqualTo(getContent()));
		}
		
		[Test]
		public void When_setting_content_to_nothing_it_will_have_initial_state()
		{
			_controller.SetContent("", 0);
			Assert.That(_controller.CurrentPlaceholder, Is.EqualTo("{nr1}"));
			Assert.That(_controller.ModifiedSnippet, Is.EqualTo(getContent()));
		}

		[Test]
		public void When_setting_content_to_space_after_first_word_it_will_use_second_as_current()
		{
			_controller.SetContent("FirstWord ", 10);
			Assert.That(_controller.CurrentPlaceholder, Is.EqualTo("{nr2}"));
			Assert.That(
				_controller.ModifiedSnippet,
				Is.EqualTo(
					getContent()
						.Replace("{nr1}", "FirstWord")));
		}

		[Test]
		public void When_setting_content_to_space_after_first_quoted_word_it_will_use_first_as_current()
		{
			_controller.SetContent("\"FirstWord ", 11);
			Assert.That(_controller.CurrentPlaceholder, Is.EqualTo("{nr1}"));
			Assert.That(
				_controller.ModifiedSnippet,
				Is.EqualTo(
					getContent()
						.Replace("{nr1}", "FirstWord")));
		}

		[Test]
		public void When_setting_content_to_more_words_than_placeholders_notify_about_it()
		{
			_controller.SetContent("FirstWord second third fourth fifth", 35);
			Assert.That(_controller.CurrentPlaceholder, Is.EqualTo("There are no more placeholders?! Stop writing!"));
		}

		[Test]
		public void When_writing_inside_first_word_it_will_replace_first_placeholder()
		{
			_controller.SetContent("FirstWord", 9);
			Assert.That(_controller.CurrentPlaceholder, Is.EqualTo("{nr1}"));
			Assert.That(
				_controller.ModifiedSnippet,
				Is.EqualTo(
					getContent()
						.Replace("{nr1}", "FirstWord")));
		}
		
		[Test]
		public void When_writing_inside_second_word_it_will_replace_first_and_second_placeholder()
		{
			_controller.SetContent("FirstWord SecondWord", 20);
			Assert.That(_controller.CurrentPlaceholder, Is.EqualTo("{nr2}"));
			Assert.That(
				_controller.ModifiedSnippet,
				Is.EqualTo(
					getContent()
						.Replace("{nr1}", "FirstWord")
						.Replace("{nr2}", "SecondWord")));
		}
		
		[Test]
		public void When_writing_inside_first_word_but_string_contains_two_words_it_will_replace_first_and_second_placeholder_and_set_first_as_active()
		{
			_controller.SetContent("FirstWord SecondWord", 5);
			Assert.That(_controller.CurrentPlaceholder, Is.EqualTo("{nr1}"));
			Assert.That(
				_controller.ModifiedSnippet,
				Is.EqualTo(
					getContent()
						.Replace("{nr1}", "FirstWord")
						.Replace("{nr2}", "SecondWord")));
		}
		
		[Test]
		public void When_rewriting_original_content_it_will_affect_placeholders()
		{
			var newContent = getContent().Replace("{nr2}", "bleh");
			_controller.SetModifiedContent(newContent);
			_controller.SetContent("FirstWord SecondWord", 20);
			Assert.That(_controller.CurrentPlaceholder, Is.EqualTo("{nr3}"));
			Assert.That(
				_controller.ModifiedSnippet,
				Is.EqualTo(
					newContent
						.Replace("{nr1}", "FirstWord")
						.Replace("{nr3}", "SecondWord")));
		}

		[Test]
		public void When_rewriting_original_content_it_will_affect_current_placeholder()
		{
			var newContent = getContent().Replace("{nr1}", "bleh");
			_controller.SetModifiedContent(newContent);
			Assert.That(_controller.CurrentPlaceholder, Is.EqualTo("{nr2}"));
		}

		private string getContent()
		{
			return "it will replace {nr1} with" + Environment.NewLine +
				"and {nr2} and {nr3} with stuff" + Environment.NewLine +
				"{4} with something";
		}
	}
}
