using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSharp.Crawlers;

namespace CSharp.Tests.Crawlers
{
    [TestFixture]
    public class AssemblyParserTests
    {
        private Fake_CacheBuilder _cache;
        private AssemblyParser _parser;

        [SetUp]
        public void Setup() {
            _cache = new Fake_CacheBuilder();
            _parser = new AssemblyParser(_cache);
        }

        [Test]
        public void Can_load_types_from_gac_assembly() {
            _parser.Parse("mscorlib");
            Assert.That(
                _cache.Classes.FirstOrDefault(x => x.Name == "List`1"),
                Is.Not.Null);
        }
    }
}
