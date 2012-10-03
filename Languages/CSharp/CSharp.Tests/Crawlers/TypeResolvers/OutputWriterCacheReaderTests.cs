using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSharp.Crawlers.TypeResolvers;
using CSharp.Projects;
using CSharp.Crawlers;

namespace CSharp.Tests.Crawlers.TypeResolvers
{
    [TestFixture]
    public class OutputWriterCacheReaderTests
    {
        private Fake_CacheBuilder _cache;
        private OutputWriterCacheReader _resolver;

        [SetUp]
        public void Setup() {
            _cache = new Fake_CacheBuilder();
            _resolver = new OutputWriterCacheReader(_cache);
            buildCache();
        }

        [Test]
        public void Can_resolve_regular_class_from_file() {
            var resolvedWith = "";
            var file = new FileRef("File1", new Project("Project1"));
            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(3, 1), "FirstClass", "Project1",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("Project1.FirstClass"));
        }

        [Test]
        public void When_not_able_to_resolve_file_it_will_not_be_in_the_list_of_resolved_types()
        {
            var resolvedWith = "not_set";
            var file = new FileRef("File1", new Project("Project1"));
            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(3, 1), "WillNotExist", "Project1",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("not_set"));
        }

        [Test]
        public void Will_resolve_based_on_available_usings()
        {
            var resolvedWith = "not_set";
            var file = new FileRef("File1", new Project("Project1"));
            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(3, 1), "ThirdClass", "Project1.SecondNamespace",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("not_set"));

            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(3, 1), "SecondClass", "Project1.FirstNamespace",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("Project1.FirstNamespace.SecondClass"));
        }

        private void buildCache() {
            var project1 = new Project("Project1");
            _cache.WriteProject(project1);
            var file = new FileRef("File1", project1);
            _cache.WriteFile(file);
            _cache.WriteUsing(new Using(file, "Project1.FirstNamespace", 1, 1));
            _cache.WriteNamespace(new Namespce(file, "Project1", 2, 1));
            _cache.WriteClass(
                new Class(file, "Project1", "FirstClass", "public", 5, 1));

            var file2 = new FileRef("File2", project1);
            _cache.WriteFile(file2);
            _cache.WriteNamespace(new Namespce(file2, "Project1.FirstNamespace", 1, 1));
            _cache.WriteClass(
                new Class(file2, "Project1.FirstNamespace", "SecondClass", "public", 2, 1));
            _cache.WriteNamespace(new Namespce(file2, "Project1.SecondNamespace", 10, 1));
            _cache.WriteClass(
                new Class(file2, "Project1.SecondNamespace", "ThirdClass", "public", 12, 2));

            var project2 = new Project("Project2");
            _cache.WriteProject(project2);
        }
    }
}
