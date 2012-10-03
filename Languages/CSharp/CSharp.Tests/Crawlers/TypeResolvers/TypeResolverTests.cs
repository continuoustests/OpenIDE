using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Crawlers.TypeResolvers;
using NUnit.Framework;

namespace CSharp.Tests.Crawlers.TypeResolvers
{
    [TestFixture]
    public class TypeResolverTests
    {
        [Test]
        public void Resolves_all_missing_types() {
            var resolver = new TypeResolver(null);
            resolver.ResolveAllUnresolved(null);
        }
    }
}
