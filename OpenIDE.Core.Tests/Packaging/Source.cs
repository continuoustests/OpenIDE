using System;
using System.Text;
using NUnit.Framework;
using OpenIDE.Core.Packaging;

namespace OpenIDE.Core.Tests.Packaging
{
	[TestFixture]
	public class SourceTests
	{
		[Test]
		public void When_reading_invalid_json_it_will_respond_with_null() {
			Source.ReadFilesUsing((f) => "not valid json!!");
			var source = Source.Read("/path/to/source1.source");
			Assert.That(source, Is.Null);
		}

		[Test]
		public void Can_read_source_file() {
			Source.ReadFilesUsing((f) => getSourceContents());
			var source = Source.Read("/path/to/source1.source");
			Assert.That(source.Path , Is.EqualTo("/path/to/source1.source"));
			Assert.That(source.Name, Is.EqualTo("source1"));
			Assert.That(source.Base, Is.EqualTo("/origin/"));
			Assert.That(source.Origin, Is.EqualTo("/origin/list.source"));
			Assert.That(source.Packages.Count, Is.EqualTo(2));
			Assert.That(source.Packages[0].ID, Is.EqualTo("package1"));
			Assert.That(source.Packages[0].OS.Length, Is.EqualTo(2));
			Assert.That(source.Packages[0].OS[0], Is.EqualTo("linux"));
			Assert.That(source.Packages[0].OS[1], Is.EqualTo("osx"));
			Assert.That(source.Packages[0].Version, Is.EqualTo("v1.0"));
			Assert.That(source.Packages[0].Name, Is.EqualTo("Package 1"));
			Assert.That(source.Packages[0].Description, Is.EqualTo("Package descr"));
			Assert.That(source.Packages[0].Package, Is.EqualTo("/origin/package1-v1.0.oipkg"));
			Assert.That(source.Packages[1].ID, Is.EqualTo("package2"));
			Assert.That(source.Packages[1].OS.Length, Is.EqualTo(1));
			Assert.That(source.Packages[1].OS[0], Is.EqualTo("windows"));
			Assert.That(source.Packages[1].Version, Is.EqualTo("v1.1"));
			Assert.That(source.Packages[1].Name, Is.EqualTo("Package 2"));
			Assert.That(source.Packages[1].Description, Is.EqualTo("Package descr2"));
			Assert.That(source.Packages[1].Package, Is.EqualTo("/origin/package2-v1.1.oipkg"));
		}

		private string getSourceContents() {
			var sb = new StringBuilder();
			sb.AppendLine("{");
			sb.AppendLine("\t\"origin\": \"/origin/list.source\",");
			sb.AppendLine("\t\"base\": \"/origin/\",");
			sb.AppendLine("\t\"packages\": [");
			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\t\"id\": \"package1\",");
			sb.AppendLine("\t\t\t\"os\": [\"linux\",\"osx\"],");
			sb.AppendLine("\t\t\t\"version\": \"v1.0\",");
			sb.AppendLine("\t\t\t\"name\": \"Package 1\",");
			sb.AppendLine("\t\t\t\"description\": \"Package descr\",");
			sb.AppendLine("\t\t\t\"package\": \"package1-v1.0.oipkg\"");
			sb.AppendLine("\t\t},");
			sb.AppendLine("\t\t{");
			sb.AppendLine("\t\t\t\"id\": \"package2\",");
			sb.AppendLine("\t\t\t\"os\": [\"windows\"],");
			sb.AppendLine("\t\t\t\"version\": \"v1.1\",");
			sb.AppendLine("\t\t\t\"name\": \"Package 2\",");
			sb.AppendLine("\t\t\t\"description\": \"Package descr2\",");
			sb.AppendLine("\t\t\t\"package\": \"package2-v1.1.oipkg\"");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t]");
			sb.AppendLine("}");
			return sb.ToString();
		}
	}
}