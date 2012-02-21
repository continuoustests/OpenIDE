using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenIDE.Core.Caching;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Caching.Search;
using System.IO;

namespace OpenIDE.CodeEngine.Core.Tests.Caching
{
    [TestFixture]
    public class HierarchyBuilderTests
    {
        [Test]
        public void When_given_a_directory_it_will_pull_out_files_and_directories_contained_by_it()
        {
            var cache = new TypeCache();
            cache.Add(toFile("/Some/Path/File1.cs"));
            cache.Add(toFile("/Some/Path/File2.cs"));
            cache.Add(toFile("/Some/Path/In/AnotherpathPlace/File2.cs"));
            cache.Add(toFile("/Some/Path/In/AnotherpathPlace/File3.cs"));

            var verifier = new ResultVerifier(cache.GetFilesInDirectory(to("/Some/Path")));
            verifier.VerifyCount(3);
            verifier.Verify(0, FileFindResultType.File, to("/Some/Path/File1.cs"));
            verifier.Verify(1, FileFindResultType.File, to("/Some/Path/File2.cs"));
            verifier.Verify(2, FileFindResultType.Directory, to("/Some/Path/In"));
        }

        [Test]
        public void When_given_a_project_it_will_pull_out_files_and_directories_contained_by_it()
        {
            var cache = new TypeCache();
            var project = new Project(to("/Some/Path/MyProject.csproj"));
            cache.Add(toFile("/Some/Path/File1.cs", project.File));
            cache.Add(toFile("/Some/Path/File2.cs", project.File));
            cache.Add(toFile("/Some/Path/In/AnotherpathPlace/File2.cs", project.File));
            cache.Add(toFile("/Some/Path/In/AnotherpathPlace/File3.cs", project.File));
            cache.Add(project);

            var verifier = new ResultVerifier(cache.GetFilesInProject(to("/Some/Path/MyProject.csproj")));
            verifier.VerifyCount(3);
            verifier.Verify(0, FileFindResultType.File, to("/Some/Path/File1.cs"), to("/Some/Path/MyProject.csproj"));
            verifier.Verify(1, FileFindResultType.File, to("/Some/Path/File2.cs"), to("/Some/Path/MyProject.csproj"));
            verifier.Verify(2, FileFindResultType.DirectoryInProject, to("/Some/Path/In"), to("/Some/Path/MyProject.csproj"));
        }

        [Test]
        public void When_given_a_project_and_a_directory_it_will_pull_out_files_and_directories_contained_by_it()
        {
            var cache = new TypeCache();
            var project = new Project(to("/Some/Path/MyProject.csproj"));
            cache.Add(toFile("/Some/Path/File1.cs", project.File));
            cache.Add(toFile("/Some/Path/File2.cs", project.File));
            cache.Add(toFile("/Some/Path/In/AnotherpathPlace/File2.cs", project.File));
            cache.Add(toFile("/Some/Path/In/AnotherpathPlace/File3.cs", project.File));
            cache.Add(project);
            cache.Add(toFile("/Some/Path/In/FileNotInProject.cs"));

            var verifier = new ResultVerifier(cache.GetFilesInProject(to("/Some/Path/MyProject.csproj"), to("/Some/Path/In")));
            verifier.VerifyCount(1);
            verifier.Verify(0, FileFindResultType.DirectoryInProject, to("/Some/Path/In/AnotherpathPlace"), to("/Some/Path/MyProject.csproj"));
        }

        private ProjectFile toFile(string path)
        {
            return new ProjectFile(to(path), null);
        }
		
		private ProjectFile toFile(string path, string project)
        {
            return new ProjectFile(to(path), project);
        }

		private string to(string path)
		{
			return path.Replace('/', Path.DirectorySeparatorChar);
		}
    }
}
