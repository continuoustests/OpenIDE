using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Crawlers.TypeResolvers;
using CSharp.Tests.Crawlers.TypeResolvers.CodeEngine;
using NUnit.Framework;
using CSharp.Responses;

namespace CSharp.Tests.Crawlers.TypeResolvers
{
    [TestFixture]
    public class EnclosingSignatureFromPositionTests
    {
        [Test]
        public void When_outside_a_namespace_it_returns_null() {
            var signature = 
                new EnclosingSignatureFromPosition(new OutputWriter(new NullResponseWriter()), (filePath) => getFileContent(filePath), (filepath) => {}, (f) => null)
                    .GetSignature("file1", 2, 1);
            Assert.That(signature, Is.Null);
        }

        [Test]
        public void When_the_closest_code_item_is_a_namespace_it_will_return_the_namespace() {
            var signature =
                new EnclosingSignatureFromPosition(new OutputWriter(new NullResponseWriter()), (filePath) => getFileContent(filePath), (filepath) => { }, (f) => null)
                    .GetSignature("file1", 4, 1);
            Assert.That(signature, Is.EqualTo("MyNamespace"));
        }

        [Test]
        public void When_the_closest_code_item_is_a_class_it_will_return_the_signature() {
            var signature =
                new EnclosingSignatureFromPosition(new OutputWriter(new NullResponseWriter()), (filePath) => getFileContent(filePath), (filepath) => { }, (f) => "")
                    .GetSignature("file1", 6, 1);
            Assert.That(signature, Is.EqualTo("MyNamespace.TestClass"));
        }

        [Test]
        public void When_the_closest_code_is_a_field_it_will_return_the_class() {
            var signature =
                new EnclosingSignatureFromPosition(new OutputWriter(new NullResponseWriter()), (filePath) => getFileContent(filePath), (filepath) => { }, (f) => null)
                    .GetSignature("file1", 7, 1);
            Assert.That(signature, Is.EqualTo("MyNamespace.TestClass"));
        }

        [Test]
        public void When_the_closest_code_item_is_a_method_it_will_return_signature() {
            var signature =
                new EnclosingSignatureFromPosition(new OutputWriter(new NullResponseWriter()), (filePath) => getFileContent(filePath), (filepath) => { }, (f) => null)
                    .GetSignature("file1", 10, 1);
            Assert.That(signature, Is.EqualTo("MyNamespace.MyStruct MyNamespace.TestClass.MyMethod(System.Int32)"));
        }

        [Test]
        public void When_outside_of_the_method_it_will_pick_containing_class() {
            var signature =
                new EnclosingSignatureFromPosition(new OutputWriter(new NullResponseWriter()), (filePath) => getFileContent(filePath), (filepath) => { }, (f) => null)
                    .GetSignature("file1", 12, 1);
            Assert.That(signature, Is.EqualTo("MyNamespace.TestClass"));
        }

        [Test]
        public void When_file_is_changed_it_will_use_the_dirty_buffer() {
            var signature =
                new EnclosingSignatureFromPosition(new OutputWriter(new NullResponseWriter()), (filePath) => getFileContent(filePath), (filepath) => { }, (f) => "file1|DirtyFile")
                    .GetSignature("file1", 6, 1);
            Assert.That(signature, Is.EqualTo("MyNamespace.ITestClass"));
        }

        private string getFileContent(string filePath) {
            if (filePath == "file1")
                return getDiskFileContent();
            else
                return getDirtyFileContent();
        }

        private string getDiskFileContent() {
            return
                line("using System;") +
                line("") +
                line("namespace MyNamespace") +
                line("{") +
                line("\tpublic class TestClass : ITestClass") +
                line("\t{") +
                line("\t\tpublic string MyProp { get; set; }") +
                line("") +
                line("\t\tpublic MyStruct MyMethod(int number) {") +
                line("\t\t\tConsole.WriteLine(\"\")") +
                line("\t\t}") +
                line("\t}") +
                line("") +
                line("\tpublic interface ITestClass") +
                line("\t{") +
                line("\t\tstring MyMethod();") +
                line("\t}") +
                line("") +
                line("\tpublic struct MyStruct") +
                line("\t{") +
                line("\t\tpublic string MyThing;") +
                line("\t}") +
                line("") +
                line("\tpublic enum MyEnum") +
                line("\t{") +
                line("\t\tNothing,") +
                line("\t\tSomething") +
                line("\t}") +
                line("}");
        }

        private string getDirtyFileContent() {
            return
                line("using System;") +
                line("") +
                line("namespace MyNamespace") +
                line("{") +
                line("\tpublic interface ITestClass") +
                line("\t{") +
                line("\t\tstring MyMethod();") +
                line("\t}") +
                line("}");
        }

        private string line(string content) {
            return content + Environment.NewLine;
        }
    }
}
