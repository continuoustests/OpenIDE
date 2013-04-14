using System;
using NUnit.Framework;
using OpenIDE.CodeEngine.Core.ReactiveScripts;

namespace OpenIDE.CodeEngine.Core.Tests.ReactiveScripts
{
	[TestFixture]
	public class ScriptTouchHandlerTests
	{
		[Test]
		public void When_processing_an_invalid_message_none_is_reported()
		{
			eventProduces("not a valid message", ScriptTouchEvents.None);
		}
		
		[Test]
		public void When_processing_a_file_added_message_inside_a_rscript_folder_it_reports_add()
		{
			eventProduces(
				"codemodel raw-filesystem-change-filecreated \"/this/is/rscript/myNewScript.sh\"",
				ScriptTouchEvents.Added);
		}
		
		[Test]
		public void When_processing_a_file_updated_message_inside_a_rscript_folder_it_reports_update()
		{
			eventProduces(
				"codemodel raw-filesystem-change-filechanged \"/this/is/rscript/myExistingScript.sh\"",
				ScriptTouchEvents.Changed);
		}
		
		[Test]
		public void When_processing_a_file_delete_message_inside_a_rscript_folder_it_reports_removed()
		{
			eventProduces(
				"codemodel raw-filesystem-change-filedeleted \"/this/is/rscript/myExistingScript.sh\"",
				ScriptTouchEvents.Removed);
		}

		[Test]
		public void When_processing_a_file_updated_message_outside_a_rscript_folder_it_reports_none()
		{
			eventProduces(
				"codemodel raw-filesystem-change-filechanged \"/this/is/myExistingScript.sh\"",
				ScriptTouchEvents.None);
		}

		[Test]
		public void Can_pull_path_from_message()
		{
			Assert.That(
				new ScriptTouchHandler(
						new System.Collections.Generic.List<string>(new[] {"/this/is/rscript"}))
					.GetPath("codemodel raw-filesystem-change-filedeleted \"/this/is/rscript/myExistingScript.sh\""),
				Is.EqualTo("/this/is/rscript/myExistingScript.sh"));
		}

		private void eventProduces(string message, ScriptTouchEvents result) {
			Assert.That(
				new ScriptTouchHandler(
						new System.Collections.Generic.List<string>(new[] {"/this/is/rscript"}))
					.Handle(message),
				Is.EqualTo(result));
		}
	}
}