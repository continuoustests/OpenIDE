using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Crawlers.TypeResolvers;
using NUnit.Framework;

namespace CSharp.Tests.Crawlers.TypeResolvers
{
    [TestFixture]
    public class CodeModelResultParserTests
    {
        [Test]
        public void Should_parse_out_various_code_references()
        {
            var parser = new CodeModelResultParser();
            var refs = parser.ParseRefs("");
            Assert.That(refs.Count, Is.EqualTo(1));
        }
    }
}
