using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Crawlers;
using CSharp.Crawlers.TypeResolvers;
using CSharp.Crawlers.TypeResolvers.CodeEngine;
using NUnit.Framework;

namespace CSharp.Tests.Crawlers.TypeResolvers
{
    [TestFixture]
    public class CodeModelResultParserTests
    {
        [Test]
        public void Should_parse_out_various_code_references()
        {
            var parser = new CodeEngineResultParser();
            var refs = parser.ParseRefs(getResult());
            Assert.That(refs[0].File.File, Is.EqualTo(@"C:\Users\ack\storage\src\OpenIDE\Languages\CSharp\CSharp\IOutputWriter.cs"));
            Assert.That(refs[0] as Class, Is.Not.Null);
            Assert.That(refs[0].Parent, Is.EqualTo("CSharp"));
            Assert.That(refs[0].Signature, Is.EqualTo("CSharp.OutputWriter"));
            Assert.That(refs[0].Name, Is.EqualTo("OutputWriter"));
            Assert.That(refs[0].Scope, Is.EqualTo("public"));
            Assert.That(refs[0].Line, Is.EqualTo(44));
            Assert.That(refs[0].Column, Is.EqualTo(15));

            Assert.That(refs[1].File.File, Is.EqualTo(@"C:\Users\ack\storage\src\OpenIDE\Languages\CSharp\CSharp\IOutputWriter.cs"));
            Assert.That(refs[1] as Interface, Is.Not.Null);
            Assert.That(refs[1].Name, Is.EqualTo("IOutputWriter"));

            Assert.That(refs[2].File.File, Is.EqualTo("System.Xml"));
            Assert.That(refs[2] as EnumType, Is.Not.Null);
            Assert.That(refs[2].Name, Is.EqualTo("QueryOutputWriter"));

            Assert.That(refs[3].File.File, Is.EqualTo("System.Xml"));
            Assert.That(refs[3] as Struct, Is.Not.Null);
            Assert.That(refs[3].Name, Is.EqualTo("StructOutputWriter"));
        }

        private string getResult() {
            return 
                @"file|C:\Users\ack\storage\src\OpenIDE\Languages\CSharp\CSharp\IOutputWriter.cs" + Environment.NewLine +
                "C#|signature|CSharp|CSharp.OutputWriter|OutputWriter|class|public|44|15|{\"bases\":{\"CSharp.IOutputWriter\":\"\"}}" + Environment.NewLine +
                "C#|signature|CSharp|CSharp.IOutputWriter|IOutputWriter|interface|public|9|19|" + Environment.NewLine +
                @"file|System.Xml" + Environment.NewLine +
                "C#|signature|System.Xml|System.Xml.QueryOutputWriter|QueryOutputWriter|enum|public|0|0|" + Environment.NewLine +
                "C#|signature|System.Xml|System.Xml.StructOutputWriter|StructOutputWriter|struct|public|0|0|";
        }
    }
}
