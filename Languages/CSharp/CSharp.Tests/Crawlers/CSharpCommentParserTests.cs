using System;
using NUnit.Framework;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using CSharp.Crawlers;
using CSharp.Projects;

namespace CSharp.Tests.Crawlers
{
	[TestFixture]
	public class CSharpCommentParserTests
	{
		private ICSharpParser _parser;
		private Fake_CacheBuilder _cache;
		
		[SetUp]
		public void Setup()
		{
			_cache = new Fake_CacheBuilder();
            _parser = new NRefactoryParser()
			//_parser = new CSharpFileParser()
				.SetOutputWriter(_cache);
			_parser.ParseFile(new FileRef("file1", null), () => { return getContent(); });
		}
		
		[Test]
		public void Should_not_parse_content_of_multiline_comments()
		{
			Assert.That(_cache.Classes.FirstOrDefault(x => x.Name.Equals("CSharpComments")), Is.Null);
		}
		
		[Test]
		public void Should_look_behind_comments()
		{
			Assert.That(_cache.Classes.FirstOrDefault(x => x.Name.Equals("InComment")), Is.Null);
			Assert.That(_cache.Classes.FirstOrDefault(x => x.Name.Equals("BehindComment")), Is.Not.Null);
		}
		
		[Test]
		public void Should_look_in_front_of_comments()
		{
			Assert.That(_cache.Classes.FirstOrDefault(x => x.Name.Equals("InFronOfCOmment")), Is.Not.Null);
			Assert.That(_cache.Classes.FirstOrDefault(x => x.Name.Equals("ClassBehind")), Is.Null);
		}
		
		private string getContent()
		{
			return File.ReadAllText(Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestResources"), "CSharpComments.txt"));
		}
	}

	class Fake_CacheBuilder : IOutputWriter 
	{
        public List<Project> Projects { get; private set; }
        public List<Using> Usings { get; private set; }
        public List<UsingAlias> UsingAliases { get; private set; }
        public List<FileRef> Files { get; private set; }
        public List<Namespce> Namespaces { get; private set; }
        public List<Class> Classes { get; private set; }
        public List<Interface> Interfaces { get; private set; }
        public List<Struct> Structs { get; private set; }
        public List<EnumType> Enums { get; private set; }
        public List<Method> Methods { get; private set; }
        public List<Field> Fields { get; private set; }
        public List<Parameter> Parameters { get; private set; }
        public List<Variable> Variables { get; private set; }

        public Fake_CacheBuilder()
        {
            Projects = new List<Project>();
            Usings = new List<Using>();
            UsingAliases = new List<UsingAlias>();
            Files = new List<FileRef>();
            Namespaces = new List<Namespce>();
            Classes = new List<Class>();
            Interfaces = new List<Interface>();
            Structs = new List<Struct>();
            Enums = new List<EnumType>();
            Methods = new List<Method>();
            Fields = new List<Field>();
            Parameters = new List<Parameter>();
            Variables = new List<Variable>();
        }

		public bool ProjectExists(Project project)
		{
			return true;
		}

        public void SetTypeVisibility(bool visibility) {
        }

        public void WriteUsing(Using usng)
        {
            Usings.Add(usng);
        }

        public void WriteProject(Project project)
		{
            Projects.Add(project);
		}
		
		public Project GetProject(string fullpath)
		{
			return Projects.Where(x => x.File == fullpath).FirstOrDefault();
		}
		
		public bool FileExists(string file)
		{
			return true;
		}

        public void WriteFile(FileRef file)
		{
			Files.Add(file);
		}

        public void WriteUsingAlias(UsingAlias alias)
        {
            UsingAliases.Add(alias);
        }

        public void WriteNamespace(Namespce ns)
		{
			Namespaces.Add(ns);
		}

        public void WriteClass(Class cls)
		{
			Classes.Add(cls);
		}

        public void WriteInterface(Interface iface)
		{
			Interfaces.Add(iface);
		}

        public void WriteStruct(Struct str)
		{
			Structs.Add(str);
		}

        public void WriteEnum(EnumType enu)
		{
			Enums.Add(enu);
		}

        public void WriteMethod(Method method)
        {
            Methods.Add(method);
        }

        public void WriteField(Field field)
        {
            Fields.Add(field);
        }

        public void WriteError(string description)
		{
		}

        public void WriteToOutput()
        {
        }

        public void BuildTypeIndex() {
        }

        public List<string> TypeIndex = new List<string>();
        public bool ContainsType(string fullname) {
            return TypeIndex.Contains(fullname);
        }

        public string FirstMatchingTypeFromName(string name) {
            return null;
        }
	}
}

