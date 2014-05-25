using System;
using System.Text;
using NUnit.Framework;
using OpenIDE.Core.Packaging;

namespace OpenIDE.Core.Tests.Packaging
{
    [TestFixture]
    public class MetaPackageTests
    {
        [Test]
        public void Can_read_meta_package()
        {
            var package = new MetaPackage((file) => {
                    return getPackage();
                },
                "/some/file"
            );

            Assert.That(package.File, Is.EqualTo("/some/file"));
            Assert.That(package.OS[0], Is.EqualTo("linux"));
            Assert.That(package.OS[1], Is.EqualTo("osx"));
            Assert.That(package.Id, Is.EqualTo("pack"));
            Assert.That(package.Version, Is.EqualTo("v1.0"));
            Assert.That(package.Name, Is.EqualTo("Pack"));
            Assert.That(package.Description, Is.EqualTo("Pack desc"));
            Assert.That(package.Packages[0].Id, Is.EqualTo("package1"));
            Assert.That(package.Packages[0].Version, Is.EqualTo("v1.0"));
            Assert.That(package.Packages[1].Id, Is.EqualTo("package2"));
            Assert.That(package.Packages[1].Version, Is.EqualTo("any"));
        }

        [Test]
        public void When_reading_an_invalid_package_it_returns_null()
        {
            Assert.That(MetaPackage.Read("/this/path/is/not/very/likely/to/exist.meta"), Is.Null);
        }

        private string getPackage() {
            var sb = new StringBuilder();
            Action<string> w = (s) => sb.AppendLine(s);
            w("{");
            w("    \"os\": [\"linux\",\"osx\"],");
            w("    \"id\": \"pack\",");
            w("    \"version\": \"v1.0\",");
            w("    \"name\": \"Pack\",");
            w("    \"description\": \"Pack desc\",");
            w("    \"packages\": [");
            w("         {");
            w("             \"id\": \"package1\",");
            w("             \"version\": \"v1.0\"");
            w("         },");
            w("         {");
            w("             \"id\": \"package2\",");
            w("             \"version\": \"any\"");
            w("         }");
            w("    ]");
            w("}");
            return sb.ToString();
        }
    }
}
