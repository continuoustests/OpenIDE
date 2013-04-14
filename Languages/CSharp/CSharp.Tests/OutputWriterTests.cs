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
                new Method(file, "", "", "", 0, 0, "", new Parameter[] { new Parameter(file, "", "", "", 0, 0, "") }));
            cache.WriteVariable(new Variable(file, "", "", "", 0, 0, ""));


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
            Assert.That(cache.Parameters.Count, Is.EqualTo(1));
            Assert.That(cache.Variables.Count, Is.EqualTo(1));
        }

        [Test]
        public void When_merging_it_will_replace_info_for_the_entire_file() 
        {
            var cache = new OutputWriter(new NullResponseWriter());
            var file = new FileRef("file1", null);
            cache.WriteFile(file);
            cache.WriteUsing(new Using(file, "", 0, 1));
            cache.WriteNamespace(new Namespce(file, "", 0, 1));
            cache.WriteClass(new Class(file, "", "", "", 0, 0));
            cache.WriteInterface(new Interface(file, "", "", "", 0, 0));
            cache.WriteStruct(new Struct(file, "", "", "", 0, 0));
            cache.WriteEnum(new EnumType(file, "", "", "", 0, 0));
            cache.WriteField(new Field(file, "", "", "", 0, 0, ""));
            cache.WriteMethod(
                new Method(file, "", "", "", 0, 0, "", new Parameter[] { new Parameter(file, "", "", "", 0, 0, "") }));
            cache.WriteVariable(new Variable(file, "", "", "", 0, 0, ""));

            var cacheToMerge = new OutputWriter(new NullResponseWriter());
            file = new FileRef("file1", null);
            cacheToMerge.WriteFile(file);
            cacheToMerge.WriteUsing(new Using(file, "", 0, 1));
            cacheToMerge.WriteUsing(new Using(file, "", 0, 1));
            cacheToMerge.WriteNamespace(new Namespce(file, "", 0, 1));
            cacheToMerge.WriteNamespace(new Namespce(file, "", 0, 1));
            cacheToMerge.WriteClass(new Class(file, "", "", "", 0, 0));
            cacheToMerge.WriteClass(new Class(file, "", "", "", 0, 0));
            cacheToMerge.WriteInterface(new Interface(file, "", "", "", 0, 0));
            cacheToMerge.WriteInterface(new Interface(file, "", "", "", 0, 0));
            cacheToMerge.WriteStruct(new Struct(file, "", "", "", 0, 0));
            cacheToMerge.WriteStruct(new Struct(file, "", "", "", 0, 0));
            cacheToMerge.WriteEnum(new EnumType(file, "", "", "", 0, 0));
            cacheToMerge.WriteEnum(new EnumType(file, "", "", "", 0, 0));
            cacheToMerge.WriteField(new Field(file, "", "", "", 0, 0, ""));
            cacheToMerge.WriteField(new Field(file, "", "", "", 0, 0, ""));
            cacheToMerge.WriteMethod(
                new Method(file, "", "", "", 0, 0, "", new Parameter[] { new Parameter(file, "", "", "", 0, 0, "") }));
            cacheToMerge.WriteMethod(
                new Method(file, "", "", "", 0, 0, "", new Parameter[] { new Parameter(file, "", "", "", 0, 0, "") }));
            cacheToMerge.WriteVariable(new Variable(file, "", "", "", 0, 0, ""));
            cacheToMerge.WriteVariable(new Variable(file, "", "", "", 0, 0, ""));

            cache.MergeWith(cacheToMerge);

            Assert.That(cache.Projects.Count, Is.EqualTo(0));
            Assert.That(cache.Files.Count, Is.EqualTo(1));
            Assert.That(cache.Usings.Count, Is.EqualTo(2));
            Assert.That(cache.Namespaces.Count, Is.EqualTo(2));
            Assert.That(cache.Classes.Count, Is.EqualTo(2));
            Assert.That(cache.Interfaces.Count, Is.EqualTo(2));
            Assert.That(cache.Structs.Count, Is.EqualTo(2));
            Assert.That(cache.Enums.Count, Is.EqualTo(2));
            Assert.That(cache.Fields.Count, Is.EqualTo(2));
            Assert.That(cache.Methods.Count, Is.EqualTo(2));
            Assert.That(cache.Parameters.Count, Is.EqualTo(2));
            Assert.That(cache.Variables.Count, Is.EqualTo(2));
        }
    }
}
