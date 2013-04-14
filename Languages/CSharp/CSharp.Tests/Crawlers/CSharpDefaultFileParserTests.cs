using System;
using CSharp.Responses;
using NUnit.Framework;
using System.Reflection;
using System.IO;
using System.Linq;
using CSharp.Crawlers;
using CSharp.Projects;

namespace CSharp.Tests.Crawlers
{
	[TestFixture]
	public class CSharpDefaultFileParserTests
	{
		private ICSharpParser _parser;
		private OutputWriter _cache;
		
		[SetUp]
		public void Setup()
		{
			_cache = new OutputWriter(new NullResponseWriter());
			_parser = new NRefactoryParser()
			//_parser = new CSharpFileParser()
				.SetOutputWriter(_cache)
                .ParseLocalVariables();
			_parser.ParseFile(new FileRef("file1", null), 
                () => { return getContent(); });
		}
		
		[Test]
		public void Should_find_file()
		{
			Assert.That(_cache.Files.Count, Is.EqualTo(1));
            Assert.That(_cache.Files[0].File, Is.EqualTo("file1"));
		}
		
		[Test]
		public void Should_find_basic_namespace()
		{
			var cache = new OutputWriter(new NullResponseWriter());
            _parser = new NRefactoryParser()
				.SetOutputWriter(cache);
			_parser.ParseFile(new FileRef("TestFile", null), () =>
				{
					return "namespace MyFirstNS {}";
				});
			var ns = cache.Namespaces.ElementAt(0);
            Assert.That(ns.File.File, Is.EqualTo("TestFile"));
			Assert.That(ns.Signature, Is.EqualTo("MyFirstNS"));
			Assert.That(ns.Name, Is.EqualTo("MyFirstNS"));
			Assert.That(ns.Line, Is.EqualTo(1));
			Assert.That(ns.Column, Is.EqualTo(11));
			Assert.That(ns.EndLine, Is.EqualTo(1));
			Assert.That(ns.EndColumn, Is.EqualTo(23));
		}

		[Test]
		public void Should_find_namespace()
		{
			var ns = _cache.Namespaces.Where(x => x.Name.Equals("MyNamespace1")).FirstOrDefault();
            Assert.That(ns.File.File, Is.EqualTo("file1"));
			Assert.That(ns.Signature, Is.EqualTo("MyNamespace1"));
			Assert.That(ns.Name, Is.EqualTo("MyNamespace1"));
			Assert.That(ns.Line, Is.EqualTo(3));
			Assert.That(ns.Column, Is.EqualTo(11));
		}
		
		[Test]
		public void Should_find_class()
		{
			var cls = _cache.Classes.Where(x => x.Name.Equals("AVerySimpleClass")).FirstOrDefault();
            Assert.That(cls.File.File, Is.EqualTo("file1"));
			Assert.That(cls.Signature, Is.EqualTo("MyNamespace1.AVerySimpleClass"));
			Assert.That(cls.Parent, Is.EqualTo("MyNamespace1"));
			Assert.That(cls.Name, Is.EqualTo("AVerySimpleClass"));
            Assert.That(cls.Scope, Is.EqualTo("private"));
			Assert.That(cls.Line, Is.EqualTo(5));
			Assert.That(cls.Column, Is.EqualTo(8));
			Assert.That(cls.EndLine, Is.EqualTo(7));
			Assert.That(cls.EndColumn, Is.EqualTo(3));
		}
		
		[Test]
		public void Should_find_inherited_class()
		{
			var cls = _cache.Classes.Where(x => x.Name.Equals("MyClass1")).FirstOrDefault();
            Assert.That(cls.File.File, Is.EqualTo("file1"));
			Assert.That(cls.Signature, Is.EqualTo("MyNamespace1.MyClass1"));
			Assert.That(cls.Parent, Is.EqualTo("MyNamespace1"));
			Assert.That(cls.Name, Is.EqualTo("MyClass1"));
            Assert.That(cls.Scope, Is.EqualTo("internal"));
			Assert.That(cls.Line, Is.EqualTo(9));
			Assert.That(cls.Column, Is.EqualTo(17));
            Assert.That(cls.JSON, Is.EqualTo("{\"bases\":{\"MyClass1Base\":\"\"}}"));
		}
		
		[Test]
		public void Should_find_multiline_namespace()
		{
			var ns = _cache.Namespaces.Where(x => x.Name.Equals("MyNamespace2")).FirstOrDefault();
            Assert.That(ns.File.File, Is.EqualTo("file1"));
			Assert.That(ns.Signature, Is.EqualTo("MyNamespace2"));
			Assert.That(ns.Name, Is.EqualTo("MyNamespace2"));
			Assert.That(ns.Line, Is.EqualTo(15));
			Assert.That(ns.Column, Is.EqualTo(1));
		}
		
		[Test]
		public void Should_find_multiline_classes()
		{
			var cls = _cache.Classes.Where(x => x.Name.Equals("MyClass2")).FirstOrDefault();
            Assert.That(cls.File.File, Is.EqualTo("file1"));
			Assert.That(cls.Signature, Is.EqualTo("MyNamespace2.MyClass2"));
			Assert.That(cls.Parent, Is.EqualTo("MyNamespace2"));
			Assert.That(cls.Name, Is.EqualTo("MyClass2"));
            Assert.That(cls.Scope, Is.EqualTo("public"));
			Assert.That(cls.Line, Is.EqualTo(19));
			Assert.That(cls.Column, Is.EqualTo(2));
		}
		
		[Test]
		public void Should_find_single_line_namespace()
		{
			var ns = _cache.Namespaces.Where(x => x.Name.Equals("MyNamespace3")).FirstOrDefault();
            Assert.That(ns.File.File, Is.EqualTo("file1"));
			Assert.That(ns.Signature, Is.EqualTo("MyNamespace3"));
			Assert.That(ns.Name, Is.EqualTo("MyNamespace3"));
			Assert.That(ns.Line, Is.EqualTo(24));
			Assert.That(ns.Column, Is.EqualTo(11));
		}
		
		[Test]
		public void Should_find_single_line_classes()
		{
			var cls = _cache.Classes.Where(x => x.Name.Equals("MyClass3")).FirstOrDefault();
            Assert.That(cls.File.File, Is.EqualTo("file1"));
			Assert.That(cls.Signature, Is.EqualTo("MyNamespace3.MyClass3"));
			Assert.That(cls.Parent, Is.EqualTo("MyNamespace3"));
			Assert.That(cls.Name, Is.EqualTo("MyClass3"));
			Assert.That(cls.Line, Is.EqualTo(24));
			Assert.That(cls.Column, Is.EqualTo(41));
		}
		
		[Test]
		public void Should_find_bizarro_namespace()
		{
			var ns = _cache.Namespaces.Where(x => x.Name.Equals("MyNamespace4")).FirstOrDefault();
            Assert.That(ns.File.File, Is.EqualTo("file1"));
			Assert.That(ns.Signature, Is.EqualTo("MyNamespace4"));
			Assert.That(ns.Name, Is.EqualTo("MyNamespace4"));
			Assert.That(ns.Line, Is.EqualTo(30));
			Assert.That(ns.Column, Is.EqualTo(5));
		}
		
		[Test]
		public void Should_find_bizarro_classes()
		{
			var cls = _cache.Classes.Where(x => x.Name.Equals("MyClass4")).FirstOrDefault();
            Assert.That(cls.File.File, Is.EqualTo("file1"));
			Assert.That(cls.Signature, Is.EqualTo("MyNamespace4.MyClass4"));
			Assert.That(cls.Parent, Is.EqualTo("MyNamespace4"));
			Assert.That(cls.Name, Is.EqualTo("MyClass4"));
			Assert.That(cls.Line, Is.EqualTo(39));
			Assert.That(cls.Column, Is.EqualTo(6));
		}
		
		[Test]
		public void Should_find_struct()
		{
			var str = _cache.Structs.Where(x => x.Name.Equals("MyStruct1")).FirstOrDefault();
			Assert.That(str.File.File, Is.EqualTo("file1"));
			Assert.That(str.Signature, Is.EqualTo("MyNamespace5.MyStruct1"));
			Assert.That(str.Parent, Is.EqualTo("MyNamespace5"));
			Assert.That(str.Name, Is.EqualTo("MyStruct1"));
            Assert.That(str.Scope, Is.EqualTo("public"));
			Assert.That(str.Line, Is.EqualTo(45));
			Assert.That(str.Column, Is.EqualTo(16));
            Assert.That(str.EndLine, Is.EqualTo(47));
            Assert.That(str.EndColumn, Is.EqualTo(3));
		}
		
		[Test]
		public void Should_find_enum()
		{
			var str = _cache.Enums.Where(x => x.Name.Equals("MyEnum1")).FirstOrDefault();
            Assert.That(str.File.File, Is.EqualTo("file1"));
			Assert.That(str.Signature, Is.EqualTo("MyNamespace5.MyEnum1"));
			Assert.That(str.Parent, Is.EqualTo("MyNamespace5"));
			Assert.That(str.Name, Is.EqualTo("MyEnum1"));
            Assert.That(str.Scope, Is.EqualTo("internal"));
			Assert.That(str.Line, Is.EqualTo(49));
			Assert.That(str.Column, Is.EqualTo(16));
            Assert.That(str.EndLine, Is.EqualTo(52));
            Assert.That(str.EndColumn, Is.EqualTo(3));
		}
		
		[Test]
		public void Should_find_interface()
		{
			var iface = _cache.Interfaces.Where(x => x.Name.Equals("MyInterface1")).FirstOrDefault();
            Assert.That(iface.File.File, Is.EqualTo("file1"));
			Assert.That(iface.Signature, Is.EqualTo("MyNamespace5.MyInterface1"));
			Assert.That(iface.Parent, Is.EqualTo("MyNamespace5"));
			Assert.That(iface.Name, Is.EqualTo("MyInterface1"));
            Assert.That(iface.Scope, Is.EqualTo("public"));
			Assert.That(iface.Line, Is.EqualTo(54));
			Assert.That(iface.Column, Is.EqualTo(19));
            Assert.That(iface.EndLine, Is.EqualTo(56));
            Assert.That(iface.EndColumn, Is.EqualTo(3));
		}

        [Test]
        public void Should_find_abstract_class()
        {
            var cls = _cache.Classes.Where(x => x.Name.Equals("AnAbstractClass")).FirstOrDefault();
            Assert.That(cls.File.File, Is.EqualTo("file1"));
            Assert.That(cls.Signature, Is.EqualTo("MyNamespace5.AnAbstractClass"));
            Assert.That(cls.Parent, Is.EqualTo("MyNamespace5"));
            Assert.That(cls.Name, Is.EqualTo("AnAbstractClass"));
            Assert.That(cls.Scope, Is.EqualTo("private"));
            Assert.That(cls.JSON, Is.EqualTo("{\"abstract\":\"1\"}"));
        }

        [Test]
        public void Should_find_sealed_class()
        {
            var cls = _cache.Classes.Where(x => x.Name.Equals("ASealedClass")).FirstOrDefault();
            Assert.That(cls.JSON, Is.EqualTo("{\"sealed\":\"1\",\"partial\":\"1\"}"));
        }

        [Test]
        public void Should_find_static_class()
        {
            var cls = _cache.Classes.Where(x => x.Name.Equals("AStaticClass")).FirstOrDefault();
            Assert.That(cls.JSON, Is.EqualTo("{\"static\":\"1\"}"));
        }

        [Test]
        public void Should_find_using()
        {
            var usng = _cache.Usings.Where(x => x.Name.Equals("System.Core")).FirstOrDefault();
            Assert.That(usng.File.File, Is.EqualTo("file1"));
            Assert.That(usng.Name, Is.EqualTo("System.Core"));
            Assert.That(usng.Line, Is.EqualTo(1));
            Assert.That(usng.Column, Is.EqualTo(7));
            Assert.That(usng.EndLine, Is.EqualTo(1));
            Assert.That(usng.EndColumn, Is.EqualTo(19));
        }
		
        [Test]
        public void Should_find_methods()
        {
            var usng = _cache.Methods.Where(x => x.Name.Equals("get")).FirstOrDefault();
            Assert.That(usng.File.File, Is.EqualTo("file1"));
            Assert.That(usng.Parent, Is.EqualTo("MyNamespace5.Program"));
            Assert.That(usng.Name, Is.EqualTo("get"));
            Assert.That(usng.Signature, Is.EqualTo("System.Void MyNamespace5.Program.get(System.Int32,ASealedClass)"));
            Assert.That(usng.Scope, Is.EqualTo("public"));
            Assert.That(usng.Line, Is.EqualTo(82));
            Assert.That(usng.Column, Is.EqualTo(28));
            Assert.That(usng.EndLine, Is.EqualTo(87));
            Assert.That(usng.EndColumn, Is.EqualTo(10));
            Assert.That(
                usng.JSON,
                Is.EqualTo("{\"static\":\"1\",\"attributes\":{\"CategoryAttribute\":\"hello,15\"},\"parameters\":{\"number\":\"System.Int32\",\"cls\":\"ASealedClass\"}}"));
        }

        [Test]
        public void Should_find_protected_methods()
        {
            var usng = _cache.Methods.Where(x => x.Name.Equals("doIt")).FirstOrDefault();
            Assert.That(usng.File.File, Is.EqualTo("file1"));
            Assert.That(usng.Parent, Is.EqualTo("MyNamespace5.Program"));
            Assert.That(usng.Signature, Is.EqualTo("System.String MyNamespace5.Program.doIt()"));
            Assert.That(usng.Scope, Is.EqualTo("protected"));
            Assert.That(usng.JSON, Is.EqualTo("{\"virtual\":\"1\"}"));
        }

        [Test]
        public void Should_find_properties()
        {
            var usng = _cache.Fields.Where(x => x.Name.Equals("MYThing")).FirstOrDefault();
            Assert.That(usng.File.File, Is.EqualTo("file1"));
            Assert.That(usng.Parent, Is.EqualTo("MyNamespace5.Program"));
            Assert.That(usng.Name, Is.EqualTo("MYThing"));
            Assert.That(usng.Signature, Is.EqualTo("System.Int32 MyNamespace5.Program.MYThing"));
            Assert.That(usng.Scope, Is.EqualTo("private"));
            Assert.That(usng.Line, Is.EqualTo(77));
            Assert.That(usng.Column, Is.EqualTo(22));
            Assert.That(usng.EndLine, Is.EqualTo(77));
            Assert.That(usng.EndColumn, Is.EqualTo(43));
            Assert.That(usng.JSON, Is.EqualTo("{\"static\":\"1\"}"));
        }

        [Test]
        public void Should_find_fields()
        {
            var usng = _cache.Fields.Where(x => x.Name.Equals("_field")).FirstOrDefault();
            Assert.That(usng.File.File, Is.EqualTo("file1"));
            Assert.That(usng.Parent, Is.EqualTo("MyNamespace5.Program"));
            Assert.That(usng.Name, Is.EqualTo("_field"));
            Assert.That(usng.Signature, Is.EqualTo("System.String MyNamespace5.Program._field"));
            Assert.That(usng.Scope, Is.EqualTo("public"));
            Assert.That(usng.Line, Is.EqualTo(79));
            Assert.That(usng.Column, Is.EqualTo(23));
            Assert.That(usng.EndLine, Is.EqualTo(79));
            Assert.That(usng.EndColumn, Is.EqualTo(37));
            Assert.That(usng.JSON, Is.EqualTo("{\"const\":\"1\"}"));
        }

        [Test]
        public void Should_find_class_attributes()
        {
            var cls = _cache.Classes.Where(x => x.Name.Equals("Program")).FirstOrDefault();
            Assert.That(cls.JSON, Is.EqualTo("{\"attributes\":{\"TestFixtureAttribute\":\"\"}}"));
        }

        [Test]
        public void Should_find_method_parameters()
        {
        	var parameter = _cache.Parameters.Where(x => x.Name.Equals("cls")).FirstOrDefault();
            Assert.That(parameter.File.File, Is.EqualTo("file1"));
            Assert.That(parameter.Parent, Is.EqualTo("MyNamespace5.Program.get(System.Int32,ASealedClass)"));
            Assert.That(parameter.Name, Is.EqualTo("cls"));
            Assert.That(parameter.Signature, Is.EqualTo("ASealedClass MyNamespace5.Program.get(System.Int32,ASealedClass).cls"));
            Assert.That(parameter.Scope, Is.EqualTo("parameter"));
            Assert.That(parameter.Line, Is.EqualTo(82));
            Assert.That(parameter.Column, Is.EqualTo(57));
            Assert.That(parameter.EndLine, Is.EqualTo(82));
            Assert.That(parameter.EndColumn, Is.EqualTo(60));
            Assert.That(parameter.JSON, Is.EqualTo(""));
        }
		
		[Test]
        public void Should_find_type_declared_variables_in_method()
        {
            var variable = _cache.Variables.Where(x => x.Name.Equals("myInt")).FirstOrDefault();
            Assert.That(variable.File.File, Is.EqualTo("file1"));
            Assert.That(variable.Parent, Is.EqualTo("MyNamespace5.Program.get(System.Int32,ASealedClass)"));
            Assert.That(variable.Name, Is.EqualTo("myInt"));
            Assert.That(variable.Signature, Is.EqualTo("System.Int32 MyNamespace5.Program.get(System.Int32,ASealedClass).myInt"));
            Assert.That(variable.Scope, Is.EqualTo("local"));
            Assert.That(variable.Line, Is.EqualTo(84));
            Assert.That(variable.Column, Is.EqualTo(8));
            Assert.That(variable.EndLine, Is.EqualTo(84));
            Assert.That(variable.EndColumn, Is.EqualTo(17));
            Assert.That(variable.JSON, Is.EqualTo(""));
        }

        [Test]
        public void Should_find_var_declared_variables_in_method()
        {
            var variable = _cache.Variables.Where(x => x.Name.Equals("bleh")).FirstOrDefault();
            Assert.That(variable.File.File, Is.EqualTo("file1"));
            Assert.That(variable.Parent, Is.EqualTo("MyNamespace5.Program.get(System.Int32,ASealedClass)"));
            Assert.That(variable.Name, Is.EqualTo("bleh"));
            Assert.That(variable.Signature, Is.EqualTo("cls.ToString() MyNamespace5.Program.get(System.Int32,ASealedClass).bleh"));
            Assert.That(variable.Scope, Is.EqualTo("local"));
            Assert.That(variable.Line, Is.EqualTo(85));
            Assert.That(variable.Column, Is.EqualTo(14));
            Assert.That(variable.JSON, Is.EqualTo(""));
        }

        [Test]
        public void Should_find_enum_member()
        {
            var str = _cache.Fields.Where(x => x.Name.Equals("Something")).FirstOrDefault();
            Assert.That(str.File.File, Is.EqualTo("file1"));
            Assert.That(str.Signature, Is.EqualTo("System.Int32 MyNamespace5.MyEnum1.Something"));
            Assert.That(str.Parent, Is.EqualTo("MyNamespace5.MyEnum1"));
            Assert.That(str.Name, Is.EqualTo("Something"));
            Assert.That(str.Scope, Is.EqualTo("public"));
            Assert.That(str.Line, Is.EqualTo(51));
            Assert.That(str.Column, Is.EqualTo(3));
        }

		private string getContent()
		{
			return File.ReadAllText(
				Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestResources"),
				"DefaultCSharp.txt"));
		}
	}
}

