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
			Assert.That(Package.Read("invalid json!"), Is.Null);
		}

		[Test]
		public void When_parsing_minimum_valid_options_parsing_validates() {
			var package = Package.Read(
				new Package("language", "Name", "v1.1", "MyDescription")
					.AddPreInstallAction("action")
					.Write());
			Assert.That(package, Is.Not.Null);
			Assert.That(package.IsValid(), Is.True);

			package = Package.Read(
				new Package("language", "Name", "v1.1", "MyDescription")
					.AddPostInstallAction("action")
					.Write());
			Assert.That(package, Is.Not.Null);
			Assert.That(package.IsValid(), Is.True);

			package = Package.Read(
				new Package("language", "Name", "v1.1", "MyDescription")
					.Write());
			Assert.That(package, Is.Not.Null);
			Assert.That(package.IsValid(), Is.True);
		}

		[Test]
		public void Can_parse_dependencies() {
			var package = Package.Read(
				new Package("language", "Name", "v1.1", "MyDescription")
					.AddPreInstallAction("action")
					.AddDependency("dependency", "v1.0")
					.Write());
			Assert.That(package.Dependencies.Count, Is.EqualTo(1));
		}

		[Test]
		public void When_parsing_package_without_minimum_nessesary_options_parsing_fails() {
			Assert.That(
				Package.Read(
					new Package("language", "", "v1.1", "MyDescription")
						.AddPreInstallAction("action1")
						.Write()),
				Is.Null);
			
			Assert.That(
				Package.Read(
					new Package("language", "MyPackage", "v1.1", "")
						.AddPreInstallAction("action1")
						.Write()),
				Is.Null);

			Assert.That(
				Package.Read(
					new Package("language", "MyPackage", "", "MyDescription")
						.AddPreInstallAction("action1")
						.Write()),
				Is.Null);

			Assert.That(
				Package.Read(
					new Package("not-valid", "MyPackage", "v1.1", "MyDescription")
						.Write()),
				Is.Null);
		}

		[Test]
		public void Can_write_package() {
			var package = 
				new Package("language", "MyPackage", "v1.1", "My Description")
					.AddPreInstallAction("action1")
					.AddPreInstallAction("action2")
					.AddDependency("Dep1", "v1")
					.AddDependency("Dep2", "v1")
					.AddPostInstallAction("action3")
					.AddPostInstallAction("action4");

			Assert.That(
				package.Write(),
				Is.EqualTo(
					"{" + NL +
					"\t\"target\": \"language\"," + NL +
					"\t\"id\": \"MyPackage\"," + NL +
					"\t\"version\": \"v1.1\"," + NL +
					"\t\"description\": \"My Description\"," + NL +
					"\t\"dependencies\":" + NL +
					"\t\t[" + NL +
					"\t\t\t{ \"id\": \"Dep1\", \"version\": \"v1\" }," + NL +
					"\t\t\t{ \"id\": \"Dep2\", \"version\": \"v1\" }" + NL +
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