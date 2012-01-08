using System;
using System.Linq;
using NUnit.Framework;
using OpenIDENet.Core.Language;

namespace OpenIDENet.Core.Tests.Language
{
	[TestFixture]
	public class UsageParserTests
	{
		[Test]
		public void When_no_valid_usage_string_return_0_elements()
		{
			var cmd = parse("");
			Assert.That(cmd.Length, Is.EqualTo(0));
		}

		[Test]
		public void When_given_a_single_usage_command_it_will_parse_it()
		{
			var cmd = parse("command|\"command description\" end");
			Assert.That(cmd.Length, Is.EqualTo(1));
			Assert.That(cmd[0].Name, Is.EqualTo("command"));
			Assert.That(cmd[0].Description, Is.EqualTo("command description"));
		}

		[Test]
		public void When_given_a_two_usage_commands_it_will_parse_them()
		{
			var cmd = parse("command|\"command description\" end command2|\"It's description\" end");
			Assert.That(cmd.Length, Is.EqualTo(2));
			Assert.That(cmd[0].Name, Is.EqualTo("command"));
			Assert.That(cmd[0].Description, Is.EqualTo("command description"));
			Assert.That(cmd[1].Name, Is.EqualTo("command2"));
			Assert.That(cmd[1].Description, Is.EqualTo("It's description"));
		}

		[Test]
		public void When_given_a_command_with_a_sub_usage_commands_it_will_parse_them()
		{
			var cmd = parse("command|\"command description\" sub_command|\"sub description\" end end");
			Assert.That(cmd.Length, Is.EqualTo(1));
			Assert.That(cmd[0].Name, Is.EqualTo("command"));
			Assert.That(cmd[0].Description, Is.EqualTo("command description"));
			Assert.That(cmd[0].Parameters.ElementAt(0).Name, Is.EqualTo("sub_command"));
			Assert.That(cmd[0].Parameters.ElementAt(0).Description, Is.EqualTo("sub description"));
		}
		
		[Test]
		public void When_given_a_list_of_commands_with_a_sub_usage_commands_it_will_parse_all()
		{
			var cmd = parse("command|\"command description\" sub_command|\"sub description\" end end " + 
							"newcommand|\"description\" " +
								"first_sub|\"first desc\" " +
									"second_level_sub1|\"second level desc\" end " +
								"end " +
								"second_sub|\"second sub desc\" end" +
							" end");
			Assert.That(cmd[0].Name, Is.EqualTo("command"));
			Assert.That(cmd[0].Description, Is.EqualTo("command description"));
			Assert.That(cmd[0].Parameters.ElementAt(0).Name, Is.EqualTo("sub_command"));
			Assert.That(cmd[0].Parameters.ElementAt(0).Description, Is.EqualTo("sub description"));

			Assert.That(cmd[1].Name, Is.EqualTo("newcommand"));
			Assert.That(cmd[1].Description, Is.EqualTo("description"));
			Assert.That(cmd[1].Parameters.ElementAt(0).Name, Is.EqualTo("first_sub"));
			Assert.That(cmd[1].Parameters.ElementAt(0).Description, Is.EqualTo("first desc"));
			Assert.That(cmd[1].Parameters.ElementAt(0).Parameters.ElementAt(0).Name, Is.EqualTo("second_level_sub1"));
			Assert.That(cmd[1].Parameters.ElementAt(0).Parameters.ElementAt(0).Description, Is.EqualTo("second level desc"));
			Assert.That(cmd[1].Parameters.ElementAt(1).Name, Is.EqualTo("second_sub"));
			Assert.That(cmd[1].Parameters.ElementAt(1).Description, Is.EqualTo("second sub desc"));
		}
		
		[Test]
		public void When_given_a_usage_statement_with_newline_and_tab_it_will_trim_them()
		{
			var cmd = parse("command|\"command description\"" + Environment.NewLine + 
							"end" + Environment.NewLine +
							"\t\t\t\t\t\t\tcommand2|\"It's description\" " + Environment.NewLine +
							"end");
			Assert.That(cmd.Length, Is.EqualTo(2));
			Assert.That(cmd[0].Name, Is.EqualTo("command"));
			Assert.That(cmd[0].Description, Is.EqualTo("command description"));
			Assert.That(cmd[1].Name, Is.EqualTo("command2"));
			Assert.That(cmd[1].Description, Is.EqualTo("It's description"));
		}

		[Test]
		public void When_given_a_command_with_an_optional_sub_usage_commands_it_will_parse_it()
		{
			var cmd = parse("command|\"command description\" [sub_command]|\"sub description\" end end");
			Assert.That(cmd.Length, Is.EqualTo(1));
			Assert.That(cmd[0].Name, Is.EqualTo("command"));
			Assert.That(cmd[0].Description, Is.EqualTo("command description"));
			Assert.That(cmd[0].Required, Is.True);
			Assert.That(cmd[0].Parameters.ElementAt(0).Name, Is.EqualTo("sub_command"));
			Assert.That(cmd[0].Parameters.ElementAt(0).Description, Is.EqualTo("sub description"));
			Assert.That(cmd[0].Parameters.ElementAt(0).Required, Is.False);
		}
		
		[Test]
		public void Should_parse_csharp_output()
		{
			var cmd = parse("create|\"Uses the create template to create what everproject related specified by the template\" library|\"Creates a new C# library project\" ITEM_NAME|\"The name of the Project/Item to create\" end  end  service|\"Creates a new C# windows service project\" ITEM_NAME|\"The name of the Project/Item to create\" end  end  end  addfile|\"Adds a file to the closest project\"FILE_TO_ADD|\"Relative or full path to the file to add\" end end  deletefile|\"Removes a file from the closest project and deletes it\"FILE_TO_DELETE|\"Relative or full path to the file to delete\" end end  dereference|\"Dereferences a project/assembly from given project\"REFERENCE|\"Path to the reference to remove\"PROJECT|\"Project to remove the reference from\" end end end  new|\"Uses the new template to create what ever specified by the template\" class|\"Creates an empty C# class\" FILE|\"Path to the file to be create\" end  end  fixture|\"Creates an empty NUnit test fixture for C#\" FILE|\"Path to the file to be create\" end  end  interface|\"Creates an new C# interface\" FILE|\"Path to the file to be create\" end  end  end  reference|\"Adds a reference to a project file\"REFERENCE|\"The path to the project or assembly to be referenced\"PROJECT|\"The path to the project to add the reference to\" end end end  removefile|\"Removes a file from the closest project\"FILE_TO_REMOVE|\"Relative or full path to the file to remove\" end end");

			Assert.That(cmd.Length, Is.EqualTo(7));
		}

		private BaseCommandHandlerParameter[] parse(string usage)
		{
			return new UsageParser(usage).Parse();
		}
	}
}
