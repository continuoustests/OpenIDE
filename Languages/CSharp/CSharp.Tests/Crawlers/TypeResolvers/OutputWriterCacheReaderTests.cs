using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Responses;
using NUnit.Framework;
using CSharp.Crawlers.TypeResolvers;
using CSharp.Projects;
using CSharp.Crawlers;

namespace CSharp.Tests.Crawlers.TypeResolvers
{
    [TestFixture]
    public class OutputWriterCacheReaderTests
    {
        private OutputWriter _cache;
        private OutputWriter _globalCache;
        private OutputWriterCacheReader _resolver;

        [SetUp]
        public void Setup() {
            _globalCache = new OutputWriter(new NullResponseWriter());
            _cache = new OutputWriter(new NullResponseWriter());
            _resolver = new OutputWriterCacheReader(_cache, _globalCache);
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
                        file, new Point(3, 1), "ThirdClass", "Project1",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("Project1.SecondNamespace.ThirdClass"));

            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(3, 1), "SecondClass", "Project1",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("Project1.FirstNamespace.SecondClass"));
        }

        [Test]
        public void Will_resolve_based_on_local_variables() {
            var resolvedWith = "not_set";
            var file = new FileRef("File1", new Project("Project1"));
            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(12, 4), "str", "Project1.FirstClass.myMethod",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("System.String"));
        }
        
        [Test]
        public void Will_resolve_variable_assignment_expression_local_scope() {
            var resolvedWith = "not_set";
            var file = new FileRef("File1", new Project("Project1"));
            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(14, 4), "str.ToString()", "Project1.FirstClass.myMethod",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("System.String"));
        }

        [Test]
        public void Will_resolve_variable_assignment_expression_local_scope_using_this()
        {
            var resolvedWith = "not_set";
            var file = new FileRef("File1", new Project("Project1"));
            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(14, 4), "this.str.ToString()", "Project1.FirstClass.myMethod",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("System.String"));
        }

        [Test]
        public void Will_resolve_variable_assignment_expression_from_private_scope()
        {
            var resolvedWith = "not_set";
            var file = new FileRef("File1", new Project("Project1"));
            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(14, 4), "FCls.Count", "Project1.FirstClass.myMethod",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("System.Int32"));
        }

        [Test]
        public void Will_not_resolve_variable_assignment_expression_from_non_static_type_members()
        {
            var resolvedWith = "not_set";
            var file = new FileRef("File1", new Project("Project1"));
            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(14, 4), "SecondClass.Count", "Project1.FirstClass.myMethod",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("not_set"));
        }

        [Test]
        public void Will_resolve_variable_assignment_expression_from_static_type_members()
        {
            var resolvedWith = "not_set";
            var file = new FileRef("File1", new Project("Project1"));
            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(14, 4), "SecondClass.NAME", "Project1.FirstClass.myMethod",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("System.String"));
        }

        [Test]
        public void Will_resolve_variable_assignment_expression_from_enum()
        {
            var resolvedWith = "not_set";
            var file = new FileRef("File1", new Project("Project1"));
            _resolver
                .ResolveMatchingType(
                    new PartialType(
                        file, new Point(14, 4), "System.More.FunnyBool.True", "Project1.FirstClass.myMethod",
                        (s) => resolvedWith = s));
            Assert.That(resolvedWith, Is.EqualTo("System.Int32"));
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
            _cache.WriteField(
                new Field(file, "Project1.FirstClass", "FCls", "public", 6, 2, "SecondClass"));
            _cache.WriteMethod(
                new Method(file, "Project1.FirstClass", "myMethod", "private", 7, 5, "System.Void", new Parameter[]{}));
            _cache.WriteVariable(
                new Variable(file, "Project1.FirstClass.myMethod", "str", "local", 9, 3, "System.String"));

            var file2 = new FileRef("File2", project1);
            _cache.WriteFile(file2);
            _cache.WriteNamespace(new Namespce(file2, "Project1.SecondNamespace", 10, 1));
            _cache.WriteClass(
                new Class(file2, "Project1.SecondNamespace", "ThirdClass", "public", 12, 2));
            _cache.WriteClass(
                new Class(file2, "Project1.SecondNamespace", "SecondClass", "public", 2, 1));

            _cache.WriteNamespace(new Namespce(file2, "Project1.FirstNamespace", 1, 1));
            _cache.WriteClass(
                new Class(file2, "Project1.FirstNamespace", "SecondClass", "public", 2, 1));
            _cache.WriteField(
                new Field(file, "Project1.FirstNamespace.SecondClass", "Count", "public", 3, 2, "System.Int32"));

            var staticField = 
                new Field(file, "Project1.FirstNamespace.SecondClass", "NAME", "public", 3, 2, "System.String");
            staticField.AddModifiers(new[]{ "static" });
            _cache.WriteField(staticField);

            var project2 = new Project("Project2");
            _cache.WriteProject(project2);

            var system = new FileRef("mscorlib", null);
            _cache.WriteFile(system);
            _cache.WriteNamespace(new Namespce(system, "System",0,0));
            _cache.WriteNamespace(new Namespce(system, "System.More", 0, 0));
            _cache.WriteClass(
                new Class(system, "System", "String", "public", 0, 0));
            _cache.WriteMethod(
                new Method(system, "System.Object", "ToString", "public", 0, 0, "System.String", new Parameter[] {}));
            _cache.WriteEnum(
                new EnumType(system, "System.More", "FunnyBool", "public", 0, 0));
            _cache.WriteField(
                new Field(system, "System.More.FunnyBool", "True", "public", 0, 0, "System.Int32").AddModifiers(new[] { "static" }));


            _cache.BuildTypeIndex();
        }
    }
}
