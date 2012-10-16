using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Responses;
using NUnit.Framework;
using CSharp.Projects;
using CSharp.Crawlers;

namespace CSharp.Tests
{
    [TestFixture]
    public class OutputWriterTests
    {
        [Test]
        public void Should_cache_added_items() {
            var cache = new OutputWriter(new NullResponseWriter());

            cache.WriteProject(new Project(""));
            var file = new FileRef("", null);
            cache.WriteFile(file);
            cache.WriteUsing(new Using(file, "", 0, 1));
            cache.WriteNamespace(new Namespce(file, "", 0, 1));
            cache.WriteClass(new Class(file, "", "", "", 0, 0));
            cache.WriteInterface(new Interface(file, "", "", "", 0, 0));
            cache.WriteStruct(new Struct(file, "", "", "", 0, 0));
            cache.WriteEnum(new EnumType(file, "", "", "", 0, 0));
            cache.WriteField(new Field(file, "", "", "", 0, 0, ""));
            cache.WriteMethod(
                new Method(file, "", "", "", 0, 0, "", new Parameter[] { }));

            Assert.That(cache.Projects.Count, Is.EqualTo(1));
            Assert.That(cache.Files.Count, Is.EqualTo(1));
            Assert.That(cache.Usings.Count, Is.EqualTo(1));
            Assert.That(cache.Namespaces.Count, Is.EqualTo(1));
            Assert.That(cache.Classes.Count, Is.EqualTo(1));
            Assert.That(cache.Interfaces.Count, Is.EqualTo(1));
            Assert.That(cache.Structs.Count, Is.EqualTo(1));
            Assert.That(cache.Enums.Count, Is.EqualTo(1));
            Assert.That(cache.Fields.Count, Is.EqualTo(1));
            Assert.That(cache.Methods.Count, Is.EqualTo(1));
        }
    }
}
