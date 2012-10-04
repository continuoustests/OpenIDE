using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Crawlers.TypeResolvers;
using NUnit.Framework;

namespace CSharp.Tests.Crawlers.TypeResolvers
{
    [TestFixture]
    public class CodeModelTypeResolverTests
    {
        [Test]
        public void test()
        {
            var model = new CodeModelTypeResolver(@"C:\Users\ack\storage\src\openide");
            var refs = model.MatchTypeName("CodeModelTypeResolverTests", new[] { "CSharp.Tests.Crawlers.TypeResolvers" });
        }
    }
}
