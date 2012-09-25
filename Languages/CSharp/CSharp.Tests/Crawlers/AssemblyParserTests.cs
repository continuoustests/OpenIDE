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
        [Test]
        public void Can_load_types_from_gac_assembly() {
            var cache = new Fake_CacheBuilder();
            var parser = new AssemblyParser(cache);
            parser.Parse("System.Configuration");
            Assert.That(cache.Classes.FirstOrDefault(x => x.Name == "bleh"), Is.Not.Null);
        }
    }
}
