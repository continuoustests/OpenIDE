using System;
using NUnit.Framework;
using OpenIDE.CommandBuilding;

namespace OpenIDE.Tests.CommandBuilding
{
	[TestFixture]
	public class CommandAutoCompletionTests
	{	
		[Test]
		public void Autocompleting_nothing_gives_nothing()
		{
            Auto.Completing("").Using("")
                .ResultsIn("");
		}

        [Test]
        public void Autocompleting_simple_command_autocompletes_whole_word()
        {
            Auto.Completing("").Using("editor")
                .ResultsIn("editor");
        }

        [Test]
        public void Autocompleting_partially_typed_command_autocompletes_remaining_characters()
        {
            Auto.Completing("edi").Using("editor")
                .ResultsIn("tor");
        }

        [Test]
        public void Autocompleting_second_command_autocompletes_whole_word()
        {
            Auto.Completing("editor ").Using("editor new")
                .ResultsIn("new");
        }

        [Test]
        public void Autocompleting_partially_typed_second_command_autocompletes_remaining_characters()
        {
            Auto.Completing("editor n").Using("editor new")
                .ResultsIn("ew");
        }

        class Auto
        {
            private string _statement;
            private string _command;

            public static Auto Completing(string statement)
            {
                return new Auto(statement);
            }

            public Auto(string statement)
            {
                _statement = statement;
            }

            public Auto Using(string command)
            {
                _command = command;
                return this;
            }

            public void ResultsIn(string completion)
            {
                Assert.That(
                    new CommandAutoCompletion().AutoComplete(_command, _statement),
                    Is.EqualTo(completion));
            }
        }
	}
}