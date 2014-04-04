using System;
using NUnit.Framework;

namespace OpenIDE.Core.Packaging.Tests
{
	[TestFixture]
	public class PackageTests
	{
		private string NL = Environment.NewLine;

		[Test]
		public void When_parsing_an_invalid_package_description_it_returns_null() {
			Assert.That(Package.Read("invalid json!", "myfile"), Is.Null);
		}

		[Test]
		public void Can_read_package_file() {
			var package = Package.Read(
				new Package(new[] { "linux" }, "language", "id", "v1.1", "cmd", "name", "MyDescription")
					.AddPreInstallAction("action")
					.Write(),
				"myfile");
			Assert.That(package.File, Is.EqualTo("myfile"));
		}

		[Test]
		public void Can_read_package_signature() {
			var package = Package.Read(
				new Package(new[] { "linux" }, "language", "id", "v1.1", "cmd", "name", "MyDescription")
					.AddPreInstallAction("action")
					.Write(),
				"myfile");
			Assert.That(package.Signature, Is.EqualTo("id-v1.1"));
		}

		[Test]
		public void When_parsing_minimum_valid_options_parsing_validates() {
			var package = Package.Read(
				new Package(new[] { "linux" }, "language", "id", "v1.1", "cmd", "name", "MyDescription")
					.AddPreInstallAction("action")
					.Write(),
				"myfile");
			Assert.That(package, Is.Not.Null);
			Assert.That(package.IsValid(), Is.True);

			package = Package.Read(
				new Package(new[] { "linux" }, "language", "id", "v1.1", "cmd", "name", "MyDescription")
					.AddPostInstallAction("action")
					.Write(),
				"myfile");
			Assert.That(package, Is.Not.Null);
			Assert.That(package.IsValid(), Is.True);

			package = Package.Read(
				new Package(new[] { "linux" }, "language", "id", "v1.1", "cmd", "name", "MyDescription")
					.Write(),
				"myfile");
			Assert.That(package, Is.Not.Null);
			Assert.That(package.IsValid(), Is.True);

			package = Package.Read(
				new Package(new[] { "linux" }, "language-script", "id", "cmd", "v1.1", "name", "MyDescription") {
						Language = "C#"
					}
					.Write(),
				"myfile");
			Assert.That(package, Is.Not.Null);
			Assert.That(package.IsValid(), Is.True);

			package = Package.Read(
				new Package(new[] { "linux" }, "language-rscript", "id", "cmd", "v1.1", "name", "MyDescription") {
						Language = "C#"
					}
					.Write(),
				"myfile");
			Assert.That(package, Is.Not.Null);
			Assert.That(package.IsValid(), Is.True);
		}

		[Test]
		public void Can_parse_dependencies() {
			var package = Package.Read(
				new Package(new[] { "linux" }, "language", "id", "v1.1", "cmd", "name", "MyDescription")
					.AddPreInstallAction("action")
					.AddDependency("dependency", new[] { "v1.0", "v1.1", "v1.2" })
					.Write(),
				"myfile");
			Assert.That(package.Dependencies.Count, Is.EqualTo(1));
		}

		[Test]
		public void When_parsing_package_without_minimum_nessesary_options_parsing_fails() {
			Assert.That(
				Package.Read(
					new Package(new[] { "linux" }, "language", "", "v1.1", "cmd", "name", "MyDescription")
						.AddPreInstallAction("action1")
						.Write(),
				"myfile"),
				Is.Null);
			
			Assert.That(
				Package.Read(
					new Package(new[] { "linux" }, "language", "MyPackage", "v1.1", "cmd", "name", "")
						.AddPreInstallAction("action1")
						.Write(),
					"myfile"),
				Is.Null);

			Assert.That(
				Package.Read(
					new Package(new[] { "linux" }, "language", "MyPackage", "", "cmd", "name", "MyDescription")
						.AddPreInstallAction("action1")
						.Write(),
					"myfile"),
				Is.Null);

			Assert.That(
				Package.Read(
					new Package(new[] { "linux" }, "not-valid", "MyPackage", "v1.1", "cmd", "name", "MyDescription")
						.Write(),
					"myfile"),
				Is.Null);

			Assert.That(
				Package.Read(
					new Package(new[] { "linux" }, "language-rscript", "id", "v1.1", "cmd", "name", "MyDescription")
					.Write(),
				"myfile"),
			Is.Null);

			Assert.That(
				Package.Read(
					new Package(new[] { "linux" }, "language", "MyPackage", "v1.1", "", "name", "MyDescription")
					.AddPreInstallAction("action1")
					.Write(),
				"myfile"),
			Is.Null);

			Assert.That(
				Package.Read(
				new Package(new[] { "linux" }, "language", "id", "v1.1", "cmd", "", "MyDescription")
					.AddPreInstallAction("action")
					.AddDependency("dependency", new[] { "v1.0", "v1.1", "v1.2" })
					.Write(),
				"myfile"),
			Is.Null);

			Assert.That(
				Package.Read(
				new Package(new[] { "linux" }, "language", "id", "v1.1", "cmd", "This name is more than 50 chars. And that's too long", "MyDescription")
					.Write(),
				"myfile"),
			Is.Null);
		}

		[Test]
		public void Can_write_package() {
			var package = 
				new Package(new[] { "linux" }, "language", "MyPackage", "v1.1", "cmd", "My package", "My Description")
					.AddPreInstallAction("action1")
					.AddPreInstallAction("action2")
					.AddDependency("Dep1", new[] {"v1","v2"})
					.AddDependency("Dep2", new[] {"v1"})
					.AddPostInstallAction("action3")
					.AddPostInstallAction("action4");

			Assert.That(
				package.Write(),
				Is.EqualTo(
					"{" + NL +
					"\t\"os\":" + NL +
					"\t\t[" + NL +
					"\t\t\t\"linux\"" + NL +
					"\t\t]," + NL +
					"\t\"target\": \"language\"," + NL +
					"\t\"id\": \"MyPackage\"," + NL +
					"\t\"version\": \"v1.1\"," + NL +
					"\t\"command\": \"cmd\"," + NL +
					"\t\"name\": \"My package\"," + NL +
					"\t\"description\": \"My Description\"," + NL +
					"\t\"dependencies\":" + NL +
					"\t\t[" + NL +
					"\t\t\t{" + NL +
					"\t\t\t\t\"id\": \"Dep1\"," + NL +
					"\t\t\t\t\"versions\":" + NL +
					"\t\t\t\t[" + NL +
					"\t\t\t\t\t\"v1\"," + NL +
					"\t\t\t\t\t\"v2\"" + NL +
					"\t\t\t\t]" + NL +
					"\t\t\t}," + NL +
					"\t\t\t{" + NL +
					"\t\t\t\t\"id\": \"Dep2\"," + NL +
					"\t\t\t\t\"versions\":" + NL +
					"\t\t\t\t[" + NL +
					"\t\t\t\t\t\"v1\"" + NL +
					"\t\t\t\t]" + NL +
					"\t\t\t}" + NL +
					"\t\t]," + NL +
					"\t\"pre-install-actions\":" + NL +
					"\t\t[" + NL +
					"\t\t\t\"action1\"," + NL +
					"\t\t\t\"action2\"" + NL +
					"\t\t]," + NL +
					"\t\"post-install-actions\":" + NL +
					"\t\t[" + NL +
					"\t\t\t\"action3\"," + NL +
					"\t\t\t\"action4\"" + NL +
					"\t\t]" + NL +
					"}"));
		}
	}
}