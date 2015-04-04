using System;
using NUnit.Framework;
using OpenIDE.Core.EditorEngineIntegration;
using OpenIDE.Core.FileSystem;
using Rhino.Mocks;
using System.IO;
using System.Linq;
namespace OpenIDE.Tests.EditorEngineIntegration
{
	[TestFixture]
	public class EngineLocatorTests
	{
		private EngineLocator _locator;
		private IFS _fs;
		private IClient _client;
        private string _filepattern;
		
		[SetUp]
		public void Setup()
		{
            int a = 3;
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace(Path.DirectorySeparatorChar.ToString(), "-");
            _filepattern = string.Format("*.EditorEngine.{0}.pid", user);
			_client = MockRepository.GenerateMock<IClient>();
			_fs = MockRepository.GenerateMock<IFS>();
			_locator = new EngineLocator(_fs);
			_locator.ClientFactory = () =>  { return _client; };
		}
		
		[Test]
		public void Should_not_match_paths_below_suggested_path()
		{
			_fs.Stub(x => x.DirectoryExists(FS.GetTempPath())).Return(true);
			_client.Stub(x => x.IsConnected).Return(true);
			_fs.Stub(x => x.GetFiles(FS.GetTempPath(), _filepattern)).Return(toFiles(new string[] { "123.EditorEngine.user.pid", "2345.EditorEngine.user.pid", "8754.EditorEngine.user.pid" }));
			_fs.Stub(x => x.ReadLines(toFile("123.EditorEngine.user.pid"))).Return(new string[] { "/some/path/here/too", "234" });
			_fs.Stub(x => x.ReadLines(toFile("2345.EditorEngine.user.pid"))).Return(new string[] { "/some/path/on/another/planet", "876" });
			_fs.Stub(x => x.ReadLines(toFile("8754.EditorEngine.user.pid"))).Return(new string[] { "Other stuff", "1" });
			var instance = _locator.GetInstance("/some/path/here");
			
			Assert.That(instance, Is.Null);
		}
		
		[Test]
		public void Should_match_paths_equal_to_suggested_path()
		{
			_fs.Stub(x => x.DirectoryExists(FS.GetTempPath())).Return(true);
			_client.Stub(x => x.IsConnected).Return(true);
			_fs.Stub(x => x.GetFiles(FS.GetTempPath(), _filepattern)).Return(toFiles(new string[] { "123.EditorEngine.user.pid", "2345.EditorEngine.user.pid", "8754.EditorEngine.user.pid" }));
			_fs.Stub(x => x.ReadLines(toFile("123.EditorEngine.user.pid"))).Return(new string[] { "/some/path/here/too", "234" });
			_fs.Stub(x => x.ReadLines(toFile("2345.EditorEngine.user.pid"))).Return(new string[] { "/some/path/here", "876" });
			_fs.Stub(x => x.ReadLines(toFile("8754.EditorEngine.user.pid"))).Return(new string[] { "Other stuff", "1" });
			var instance = _locator.GetInstance("/some/path/here");
			
			Assert.That(instance.File, Is.EqualTo(toFile("2345.EditorEngine.user.pid")));
			Assert.That(instance.ProcessID, Is.EqualTo(2345));
			Assert.That(instance.Key, Is.EqualTo("/some/path/here"));
			Assert.That(instance.Port, Is.EqualTo(876));
		}
		
		[Test]
		public void Should_match_paths_at_a_lower_level_than_suggested_path()
		{
			_fs.Stub(x => x.DirectoryExists(FS.GetTempPath())).Return(true);
			_client.Stub(x => x.IsConnected).Return(true);
			_fs.Stub(x => x.GetFiles(FS.GetTempPath(), _filepattern)).Return(toFiles(new string[] { "123.EditorEngine.user.pid", "2345.EditorEngine.user.pid", "8754.EditorEngine.user.pid" }));
			_fs.Stub(x => x.ReadLines(toFile("123.EditorEngine.user.pid"))).Return(new string[] { "/some/path/here/too", "234" });
			_fs.Stub(x => x.ReadLines(toFile("2345.EditorEngine.user.pid"))).Return(new string[] { "/some/path", "876" });
			_fs.Stub(x => x.ReadLines(toFile("8754.EditorEngine.user.pid"))).Return(new string[] { "Other stuff", "1" });
			var instance = _locator.GetInstance("/some/path/here");
			
			Assert.That(instance.File, Is.EqualTo(toFile("2345.EditorEngine.user.pid")));
			Assert.That(instance.ProcessID, Is.EqualTo(2345));
			Assert.That(instance.Key, Is.EqualTo("/some/path"));
			Assert.That(instance.Port, Is.EqualTo(876));
		}
		
		[Test]
		public void Should_matching_on_two_paths_choose_closest_path()
		{
			_fs.Stub(x => x.DirectoryExists(FS.GetTempPath())).Return(true);
			_client.Stub(x => x.IsConnected).Return(true);
			_fs.Stub(x => x.GetFiles(FS.GetTempPath(), _filepattern)).Return(toFiles(new string[] { "123.EditorEngine.user.pid", "2345.EditorEngine.user.pid", "8754.EditorEngine.user.pid" }));
			_fs.Stub(x => x.ReadLines(toFile("123.EditorEngine.user.pid"))).Return(new string[] { "/some/path", "234" });
			_fs.Stub(x => x.ReadLines(toFile("2345.EditorEngine.user.pid"))).Return(new string[] { "/some/path/here", "876" });
			_fs.Stub(x => x.ReadLines(toFile("8754.EditorEngine.user.pid"))).Return(new string[] { "Other stuff", "1" });
			var instance = _locator.GetInstance("/some/path/here");
			
			Assert.That(instance.File, Is.EqualTo(toFile("2345.EditorEngine.user.pid")));
			Assert.That(instance.ProcessID, Is.EqualTo(2345));
			Assert.That(instance.Key, Is.EqualTo("/some/path/here"));
			Assert.That(instance.Port, Is.EqualTo(876));
		}
		
		[Test]
		public void Should_not_pick_instances_where_it_cannot_connect_to_host()
		{
			_fs.Stub(x => x.DirectoryExists(FS.GetTempPath())).Return(true);
			_fs.Stub(x => x.GetFiles(FS.GetTempPath(), _filepattern)).Return(toFiles(new string[] { "123.EditorEngine.user.pid" }));
			_fs.Stub(x => x.ReadLines(toFile("123.EditorEngine.user.pid"))).Return(new string[] { "/some/path/here", "234" });
			var instance = _locator.GetInstance("/some/path/here");
			
			Assert.That(instance, Is.Null);
		}
		
		[Test]
		public void Should_delete_files_where_it_cannot_connect_to_host()
		{
			_fs.Stub(x => x.DirectoryExists(FS.GetTempPath())).Return(true);
			_fs.Stub(x => x.GetFiles(FS.GetTempPath(), _filepattern)).Return(toFiles(new string[] { "123.EditorEngine.user.pid" }));
			_fs.Stub(x => x.ReadLines(toFile("123.EditorEngine.user.pid"))).Return(new string[] { "/some/path/here", "234" });
			_locator.GetInstance("/some/path/here");
			
			_fs.AssertWasCalled(x => x.DeleteFile(toFile("123.EditorEngine.user.pid")));
		}
		
		[Test]
		public void Should_not_find_editor_if_editor_engine_temp_directory_does_not_exist()
		{
			_locator.GetInstance("/some/path/here");
			
			_fs.AssertWasNotCalled(x => x.GetFiles(Path.Combine(FS.GetTempPath(), "EditorEngine"), "*.pid"));
		}

        private string[] toFiles(string[] files)
        {
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace(Path.DirectorySeparatorChar.ToString(), "-");
            return files.Select(x => x.Replace(".user.", user)).ToArray();
        }

        private string toFile(string file)
        {
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace(Path.DirectorySeparatorChar.ToString(), "-");
            return file.Replace(".user.", user);
        }
	}
}

