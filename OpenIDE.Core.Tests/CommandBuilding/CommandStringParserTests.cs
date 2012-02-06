using System;
using System.Linq;
using NUnit.Framework;
using OpenIDE.Core.CommandBuilding;
using System.Collections.Generic;

namespace OpenIDE.Tests.Core.CommandBuilding
{
	[TestFixture]
	public class CommandStringParserTests
	{
		private CommandStringParser _parser;
		
		[SetUp]
		public void Setup()
		{
			_parser = new CommandStringParser();
		}
		
		[Test]
		public void Empty_argument_string_returns_empty_list()
		{
            Assert.That(_parser.Parse("").Count(), Is.EqualTo(0));
		}

        [Test]
        public void Simple_argument_string_returns_list_of_arguments()
        {
            assertArguments(
                _parser.Parse("new class").ToArray(),
                "new", "class");
        }

        [Test]
        public void Folder_argument_string_returns_list_of_arguments()
        {
            assertArguments(
                _parser.Parse("new class /home/my/location").ToArray(),
                "new", "class", "/home/my/location");
        }

        [Test]
        public void Windows_folder_argument_string_returns_list_of_arguments()
        {
            assertArguments(
                _parser.Parse("new class C:\\my\\folder\\myfile.txt").ToArray(),
                "new", "class", "C:\\my\\folder\\myfile.txt");
        }

        [Test]
        public void Quoted_folder_with_space_argument_string_returns_list_of_arguments()
        {
            assertArguments(
                _parser.Parse("new class \"/home/folder/with some sub folder\"").ToArray(),
                "new", "class", "/home/folder/with some sub folder");
        }

        [Test]
        public void Windows_quoted_folder_with_space_argument_string_returns_list_of_arguments()
        {
            assertArguments(
                _parser.Parse("new class \"C:\\my\\folder with spaces\\myfile.txt\"").ToArray(),
                "new", "class", "C:\\my\\folder with spaces\\myfile.txt");
        }

		[Test]
		public void Can_parse_string_with_different_separator()
		{
			assertArguments(
				new CommandStringParser(',')
					.Parse("one,after ,the	,other, and with space inside them").ToArray(),
				"one", "after", "the", "other", "and with space inside them");
		}

        private void assertArguments(string[] args, params string[] argList)
        {
            Assert.That(args.Length, Is.EqualTo(argList.Length));
            for (int i = 0; i < args.Length; i++)
                Assert.That(args[i], Is.EqualTo(argList[i]));
        }
	}
}
