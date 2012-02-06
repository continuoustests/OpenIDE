using System;
using NUnit.Framework;
using OpenIDE.CommandBuilding;
using System.IO;

namespace OpenIDE.Tests.CommandBuilding
{
	[TestFixture]
	public class PathAutoCompletionTests
	{
		[Test]
        public void Autocompleting_nothing_gives_nothing()
		{
            Auto.Completing("").Using("")
                .ResultsIn("");
		}

        [Test]
        public void Autocompleting_file_returns_file()
        {
            Auto.Completing("").Using("file1.txt")
                .ResultsIn("file1.txt");
        }

        [Test]
        public void Autocompleting_partially_typed_file_returns_file()
        {
            Auto.Completing("fi").Using("file1.txt")
                .ResultsIn("le1.txt");
        }

        [Test]
        public void Autocompleting_existing_path_returns_addition_to_path()
        {
            Auto.Completing("/home/person/").Using("file1.txt")
                .ResultsIn("file1.txt");
        }

        [Test]
        public void Autocompleting_path_with_spaces_path_returns_addition_to_path()
        {
            Auto.Completing("\"/home/person/this is").Using("this is sparta")
                .ResultsIn(" sparta");
        }

        class Auto
        {
            private string _statement;
            private string _command;

            public static Auto Completing(string statement)
            {
                return new Auto(
                    statement
                        .Replace('/', Path.DirectorySeparatorChar));
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
                    new PathAutoCompletion().AutoComplete(_command, _statement),
                    Is.EqualTo(completion));
            }
        }
	}
}