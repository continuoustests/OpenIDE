using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSharp.Crawlers;
using CSharp.Projects;

namespace CSharp.Tests.Crawlers
{
    [TestFixture]
    public class ReferenceCacheTests
    {
        private IReferenceCache _cache;

        [SetUp]
        public void Setup() {
            _cache = new ReferenceCache();
        }

        [Test]
        public void Can_add_using() {
            canAdd(() => _cache.AddUsing(new Using(null, "System", 1, 1)), "System");
        }

        [Test]
        public void Can_add_using_alias() {
            canAdd(
                () => _cache.AddUsingAlias(new UsingAlias(null, "MyAlias", "", 1, 1)),
                "MyAlias");
        }

        [Test]
        public void Can_add_project() {
            canAddContainer(
                () => _cache.AddProject(new Project("project")),
                "project");
        }

        [Test]
        public void Can_add_file() {
            canAddContainer(
                () => _cache.AddFile(new FileRef("file", null)),
                "file");
        }

        [Test]
        public void Can_add_namespace() {
            canAdd(
                () => _cache.AddNamespace(new Namespce(null, "ns", 1, 1)),
                "ns");
        }

        [Test]
        public void Can_add_class() {
            canAdd(
                () => _cache.AddClass(new Class(null, "", "class", "", 1, 1)),
                "class");
        }

        [Test]
        public void Can_add_interface() {
            canAdd(
                () => _cache.AddInterface(new Interface(null, "", "interface", "", 1, 1)),
                "interface");
        }

        [Test]
        public void Can_add_structs() {
            canAdd(
                () => _cache.AddStruct(new Struct(null, "", "struct", "", 1, 1)),
                "struct");
        }

        [Test]
        public void Can_add_enum() {
            canAdd(
                () => _cache.AddEnum(new EnumType(null, "", "enum", "", 1, 1)),
                "enum");
        }

        [Test]
        public void Can_add_field() {
            canAdd(
                () => _cache.AddField(new Field(null, "", "field", "", 1, 1, "")),
                "field");
        }

        [Test]
        public void Can_add_method() {
            canAdd(
                () => _cache.AddMethod(new Method(null, "", "method", "", 1, 1, "", new Parameter[] {})),
                "method");
        }

        [Test]
        public void Can_add_variable() {
            canAdd(
                () => _cache.AddVariable(new Variable(null, "", "variable", "", 1, 1, "")),
                "variable");
        }

        [Test]
        public void not_implemented_functions() {
            throw new Exception("Need to: support all indexes required by output writer, also need merging");
        }

        private void canAddContainer(Func<long> cacheAdd, string name) {
            Assert.That(
                nameFromContainer(cacheAdd()),
                Is.EqualTo(name));
        }

        private void canAdd(Func<long> cacheAdd, string name) {
            Assert.That(
                nameFromCodeRef(cacheAdd()),
                Is.EqualTo(name));
        }

        private string nameFromContainer(long id) {
            var item = _cache.ContainerFromID(id);
            if (item == null)
                return null;
            return item.Signature;
        }

        private string nameFromCodeRef(long id) {
            var item = _cache.CodeRefFromID(id);
            if (item == null)
                return null;
            return item.Name;
        }
    }
}
