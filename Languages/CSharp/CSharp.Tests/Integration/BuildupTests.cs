using System;
using System.IO;
using System.Diagnostics;
using NUnit.Framework;

namespace CSharp.Tests.Integration
{
	[TestFixture]
	public class BuildupTests
	{
		private string _baseDir = "buildup";

		[SetUp]
		public void Setup()
		{
			if (Directory.Exists(_baseDir))
				Directory.Delete(_baseDir, true);
			if (Directory.Exists("C#"))
				Directory.Delete("C#", true);
		}

		[TearDown]
		public void Teardown()
		{
			Setup();
		}

		[Test]
		public void Run_the_full_suite_of_commands()
		{
			copyTemplates();
			createLibrary1();
		}

		private void createLibrary1()
		{
			// Create library 1
			var project = combine("library1","library1.csproj");
			run("create", "library", project);
			assertExists(project);

			// Add main.cs to library 1
			var main = combine("library1", "Main");
			run("new", "class", main);
			assertExists(main + ".cs");
			assertContains(project, "    <Compile Include=\"Main.cs\" />");

			// Add non compile file
			var nonCompile = combine("library1", "NoNCompile.txt");
			File.WriteAllText(nonCompile, "");
			run("addfile", nonCompile);
			assertContains(project, "    <None Include=\"NoNCompile.txt\" />");

			// Add using wildcard names
			var wildcard1 = combine("library1", "wildcard1.cs");
			var wildcard2 = combine("library1", "wildcard2.txt");
			var wildcardNoMatch = combine("library1", "notwildcard.cs");
			File.WriteAllText(wildcard1, "");
			File.WriteAllText(wildcard2, "");
			File.WriteAllText(wildcardNoMatch, "");
			run("addfile", combine("library1", "wildcard*"));
			assertContains(project, "    <Compile Include=\"wildcard1.cs\" />");
			assertContains(project, "    <None Include=\"wildcard2.txt\" />");
			assertNotContains(project, "    <Compile Include=\"notwildcard.cs\" />");

			// Add using recursive wildcard names
			var wildcard3 = combine("library1", "wildcardsub1.cs");
			var wildcard4 = combine("library1", "subdir", "wildcardsub2.cs");
			File.WriteAllText(wildcard3, "");
			Directory.CreateDirectory(Path.GetDirectoryName(wildcard4));
			File.WriteAllText(wildcard4, "");
			run("addfile", combine("library1", "wildcardsub*"), "--recursive");
			assertContains(project, "    <Compile Include=\"wildcardsub1.cs\" />");
			assertContains(project, "    <Compile Include=\"subdir\\wildcardsub2.cs\" />");
			assertNotContains(project, "    <Compile Include=\"notwildcard.cs\" />");

			// Create library 2
			var project2 = combine("library2", "library2.csproj");
			run("create", "library", project2);
			assertExists(project2);
			
			var projectReference = 
				"    <ProjectReference Include=\"..\\library1\\library1.csproj\">" + Environment.NewLine +
				"      <Project>{fb9bd7ea-f1f8-4f9c-8b82-17e703c0c766}</Project>" + Environment.NewLine +
				"      <Name>library1</Name>" + Environment.NewLine +
				"    </ProjectReference>";

			// Reference library 1 in library 2
			run("reference", project, project2);
			assertContains(project2, projectReference);

			var assemblyReference = "    <Reference Include=\"..\\..\\C#.exe\" />";
			// Reference assembly to library1
			run("reference", "C#.exe", project);
			assertContains(project, assemblyReference);

			// Add existing file to library 2
			var library2Main = combine("library2", "Main.cs");
			File.WriteAllText(library2Main, "");
			run("addfile", library2Main);
			assertContains(project2, "    <Compile Include=\"Main.cs\" />");

			// Remove project reference
			run("dereference", project, project2);
			assertNotContains(project2, projectReference);

			// Remove assembly reference
			run("dereference", "C#.exe", project);
			assertNotContains(project, assemblyReference);

			// Delete file
			run("deletefile", main + ".cs");
			assertNotContains(project, "    <Compile Include=\"Main.cs\" />");
			assertNotExists(main + ".cs");

			// Remove file
			run("removefile", library2Main);
			assertNotContains(project2, "    <Compile Include=\"Main.cs\" />");
			assertExists(library2Main);
		}

		private void assertContains(string file, string content)
		{
			Assert.That(File.ReadAllText(file).Contains(content), Is.True, file + " did not contain " + content);
		}

		private void assertNotContains(string file, string content)
		{
			Assert.That(File.ReadAllText(file).Contains(content), Is.False, file + " contained " + content);
		}

		private void assertExists(string path)
		{
			Assert.That(File.Exists(path), Is.True, "Could not find " + path);
		}

		private void assertNotExists(string path)
		{
			Assert.That(File.Exists(path), Is.False, "File was not deleted " + path);
		}

		private void createBaseDir()
		{
			Directory.CreateDirectory(_baseDir);
		}

		private string combine(params string[] chunks)
		{
			var path = _baseDir;
			foreach (var chunk in chunks)
				path = Path.Combine(path, chunk);
			return path;
		}

		private void copyTemplates()
		{
			// TODO Fix this later (not relative to project
			copyFolder(@"../../../templates", @"C#");
		}

		private void copyFolder(string sourceFolder, string destFolder)
		{  
			if (!Directory.Exists(destFolder))  
				Directory.CreateDirectory(destFolder);  
			string[] files = Directory.GetFiles(sourceFolder);  
			foreach (string file in files)  
			{  
				string name = Path.GetFileName(file);  
				string dest = Path.Combine(destFolder, name);  
				if (File.Exists(dest))
					File.Delete(dest);
				File.Copy(file, dest);  
			}  
			string[] folders = Directory.GetDirectories(sourceFolder);  
			foreach (string folder in folders)  
			{  
				string name = Path.GetFileName(folder);  
				string dest = Path.Combine(destFolder, name);  
				copyFolder(folder, dest);  
			}  
		}

		private void run(params string[] args)
		{
			MainClass.Main(args);
		}
	}
}
