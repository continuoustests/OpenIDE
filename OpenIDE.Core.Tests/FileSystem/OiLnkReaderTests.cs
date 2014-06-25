using System;
using System.Text;
using NUnit.Framework;
using OpenIDE.Core.FileSystem;

namespace OpenIDE.Core.Tests.FileSystem
{
	[TestFixture]
	public class OiLnkReaderTests
	{
		[Test]
		public void When_reading_invalid_link_file_it_returns_null() {
			Assert.That(OiLnkReader.Read("invalid json!"), Is.Null);
		}

		[Test]
		public void When_reading_file_with_handlers_it_reads_handlers() {
			var lnk = OiLnkReader.Read(getJSON());
			Assert.That(lnk.Handlers.Length, Is.EqualTo(1));
			Assert.That(lnk.Handlers[0].Arguments[0], Is.EqualTo("arg1"));
			Assert.That(lnk.Handlers[0].Arguments[1], Is.EqualTo("arg2"));
			Assert.That(lnk.Handlers[0].Responses[0], Is.EqualTo("My script"));
		}

		[Test]
		public void When_passed_a_link_it_reads_link() {
			var lnk = OiLnkReader.Read(getJSON());
			Assert.That(lnk.Preparer, Is.EqualTo(toOSPath("application-files/compile.py")));
			Assert.That(lnk.LinkCommand, Is.EqualTo(toOSPath("path/to/my/application")));
			Assert.That(lnk.LinkArguments, Is.EqualTo("{args}"));
		}

		private string toOSPath(string path) {
			if (Environment.OSVersion.Platform == PlatformID.Unix || 
				Environment.OSVersion.Platform == PlatformID.MacOSX)
			{
				return path;
			}
			return path.Replace("/", "\\");
		}

		[Test]
		public void Can_not_match_handler_to_single_not_matching_argument() {
			Assert.That(
				new OiLnkReader.Handler(new[] {"my-args"}, new string[] {})
					.Matches(new[] {"my-other-args"}),
				Is.False);
		}

		[Test]
		public void Can_match_handler_to_single_argument() {
			Assert.That(
				new OiLnkReader.Handler(new[] {"my-args"}, new string[] {})
					.Matches(new[] {"my-args"}),
				Is.True);
		}

		[Test]
		public void Can_not_match_handler_to_multiple_none_matching_arguments() {
			Assert.That(
				new OiLnkReader.Handler(new[] {"my-args", "my-second-arg"}, new string[] {})
					.Matches(new[] {"my-args", "my-other-second-arg"}),
				Is.False);
		}

		[Test]
		public void Can_match_handler_to_multiple_matching_arguments() {
			Assert.That(
				new OiLnkReader.Handler(new[] {"my-args", "my-second-arg"}, new string[] {})
					.Matches(new[] {"my-args", "my-second-arg"}),
				Is.True);
		}

		[Test]
		public void Can_not_match_handler_to_multiple_matching_arguments_when_passed_argument_count_is_higher_than_handler_argument_count() {
			Assert.That(
				new OiLnkReader.Handler(new[] {"my-args", "my-second-arg"}, new string[] {})
					.Matches(new[] {"my-args", "my-second-arg", "my-third"}),
				Is.False);
		}

		[Test]
		public void Can_match_handler_to_any_argument_arguments() {
			Assert.That(
				new OiLnkReader.Handler(new[] {"|ANY|", "my-second-arg"}, new string[] {})
					.Matches(new[] {"my-args", "my-second-arg"}),
				Is.True);
		}

		private string getJSON() {
			var sb = new StringBuilder();
			Action<string> a = (line) => {
				sb.AppendLine(line);
			};
			a("{");
			a("    \"#Comment\" : \"Handlers are command arguments and stdout writelines\",");
			a("	   \"handlers\" : [");
			a("        { ");
			a("            \"handler\" : {");
			a(" 		       \"arguments\" : [ \"arg1\", \"arg2\" ],");
			a(" 			   \"responses\" : [ \"My script\" ]");
			a(" 			}");
			a(" 	   }");
			a("    ],");
			a("    \"link\" : {");
			a("		   \"preparer\": \"application-files/compile.py\",");
			a("        \"executable\" : \"path/to/my/application\",");
			a(" 	   \"params\" : \"{args}\",");
			a(" 	   \"#Comment\" : \"Valid: {run-location} {global-profile} {local-profile} {args}\" ");
			a("	   }");
			a("}");
			return sb.ToString();
		}
	}
}