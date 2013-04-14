using System;
using NUnit.Framework;
using CSharp.Crawlers;
using CSharp.Projects;
using CSharp.Responses;
using CSharp.Crawlers.TypeResolvers;
using System.Text;

namespace CSharp.Tests.Crawlers.TypeResolvers
{
	[TestFixture]
	public class FileContextAnalyzerTests
	{
        private OutputWriter _cache;
        private OutputWriter _globalCache;
		private string _file = "file";
        private string _fileContent;
		private FileContextAnalyzer _analyzer;

		[SetUp]
		public void Setup()
		{
            
            createGlobalCache();
			createLocalCache();
            _analyzer = new FileContextAnalyzer(
                _globalCache,
                _cache);
		}

		[Test]
		public void Can_get_signatur_from_method_variable()
		{
			Assert.That(
                signatureFromPosition(9, 24), // le.WriteLine(str;
                Is.EqualTo("System.String MyNS.MyClass.MyMethod(System.String).str"));
		}

        [Test]
        public void Can_get_signatur_from_method_variable_with_trailing_method_call()
        {
            Assert.That(
                signatureFromPosition(11, 30), // eLine(bleh.ToString(
                Is.EqualTo("System.Int32 MyNS.MyClass.MyMethod(System.String).bleh"));
        }

        [Test]
        public void Can_get_signatur_from_method_parameter()
        {
            Assert.That(
                signatureFromPosition(9, 29), // le.WriteLine(str + word);
                Is.EqualTo("System.String MyNS.MyClass.MyMethod(System.String).word"));
        }

        [Test]
        public void Can_get_signatur_from_field()
        {
            Assert.That(
                signatureFromPosition(12, 15), // if (_isValid)");
                Is.EqualTo("System.Bool MyNS.MyClass._isValid"));
        }

        [Test]
        public void Have_to_do_this() 
        {
            throw new Exception("The namespace field in the types have to be full signature for anything to work here");
        }

        private string signatureFromPosition(int line, int column) {
            var name =
                new TypeUnderPositionResolver()
                    .GetTypeName(_file, _fileContent, line, column);
            return 
                _analyzer.GetSignatureFromNameAndPosition(_file, name, line, column);
        }

        private void createGlobalCache() {
            _globalCache = new OutputWriter(new NullResponseWriter());
            _globalCache
                .WriteClass(createSystemClass("String"));
            _globalCache
                .WriteClass(createSystemClass("Boolean"));
        }

        private Class createSystemClass(string name) {
            return 
                new Class(
                    new FileRef("mscorlib.dll", null),
                    "System",
                    name,
                    "public",
                    0,
                    0);
        }

		private void createLocalCache() {
			var sb = new StringBuilder();
            Action<string> a = (s) => sb.AppendLine(s);
			a("using System;");
			a("");
			a("namespace MyNS");
			a("{");
			a("	public class MyClass");
			a("	{");
			a("		private void MyMethod(string word) {");
			a("			var str = \"Hello World\";");
			a("			Console.WriteLine(str + word);");
            a("         var bleh = 15;");
            a("         Console.WriteLine(bleh.ToString());");
            a("         if (_isValid)");
            a("             Console.WriteLine(\"meh\");");
			a("		}");
            a("     ");
            a("     private bool _isValid = false;");
			a("	}");
			a("}");
            _fileContent = sb.ToString();
			_cache = new OutputWriter(new NullResponseWriter());
			_cache.SetTypeVisibility(true);
			var parser = new NRefactoryParser()
                .ParseLocalVariables();
			parser.SetOutputWriter(_cache);
			parser.ParseFile(new FileRef("MyFile", null), () => _fileContent);
            _cache.BuildTypeIndex();
            new TypeResolver(new OutputWriterCacheReader(_cache, _globalCache))
                .ResolveAllUnresolved(_cache);
		}
	}
}