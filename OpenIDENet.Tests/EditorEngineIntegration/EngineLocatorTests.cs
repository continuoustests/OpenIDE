using System;
using NUnit.Framework;
using OpenIDENet.EditorEngineIntegration;
using OpenIDENet.FileSystem;
using Rhino.Mocks;
using System.IO;
namespace OpenIDENet.Tests.EditorEngineIntegration
{
	[TestFixture]
	public class EngineLocatorTests
	{
		private EngineLocator _locator;
		private IFS _fs;
		private IClient _client;
		
		[SetUp]
		public void Setup()
		{
			_client = MockRepository.GenerateMock<IClient>();
			_fs = MockRepository.GenerateMock<IFS>();
			_locator = new EngineLocator(_fs);
			_locator.ClientFactory = () =>  { return _client; };
		}
		
		[Test]
		public void Should_not_match_paths_below_suggested_path()
		{
			_client.Stub(x => x.IsConnected).Return(true);
			_fs.Stub(x => x.GetFiles(Path.Combine(Path.GetTempPath(), "EditorEngine"), "*.pid")).Return(new string[] { "123.pid", "2345.pid", "8754.pid" });
			_fs.Stub(x => x.ReadLines("123.pid")).Return(new string[] { "/some/path/here/too", "234" });
			_fs.Stub(x => x.ReadLines("2345.pid")).Return(new string[] { "/some/path/on/another/planet", "876" });
			_fs.Stub(x => x.ReadLines("8754.pid")).Return(new string[] { "Other stuff", "1" });
			var instance = _locator.GetInstance("/some/path/here");
			
			Assert.That(instance, Is.Null);
		}
		
		[Test]
		public void Should_match_paths_equal_to_suggested_path()
		{
			_client.Stub(x => x.IsConnected).Return(true);
			_fs.Stub(x => x.GetFiles(Path.Combine(Path.GetTempPath(), "EditorEngine"), "*.pid")).Return(new string[] { "123.pid", "2345.pid", "8754.pid" });
			_fs.Stub(x => x.ReadLines("123.pid")).Return(new string[] { "/some/path/here/too", "234" });
			_fs.Stub(x => x.ReadLines("2345.pid")).Return(new string[] { "/some/path/here", "876" });
			_fs.Stub(x => x.ReadLines("8754.pid")).Return(new string[] { "Other stuff", "1" });
			var instance = _locator.GetInstance("/some/path/here");
			
			Assert.That(instance.File, Is.EqualTo("2345.pid"));
			Assert.That(instance.ProcessID, Is.EqualTo(2345));
			Assert.That(instance.Key, Is.EqualTo("/some/path/here"));
			Assert.That(instance.Port, Is.EqualTo(876));
		}
		
		[Test]
		public void Should_match_paths_at_a_lower_level_than_suggested_path()
		{
			_client.Stub(x => x.IsConnected).Return(true);
			_fs.Stub(x => x.GetFiles(Path.Combine(Path.GetTempPath(), "EditorEngine"), "*.pid")).Return(new string[] { "123.pid", "2345.pid", "8754.pid" });
			_fs.Stub(x => x.ReadLines("123.pid")).Return(new string[] { "/some/path/here/too", "234" });
			_fs.Stub(x => x.ReadLines("2345.pid")).Return(new string[] { "/some/path", "876" });
			_fs.Stub(x => x.ReadLines("8754.pid")).Return(new string[] { "Other stuff", "1" });
			var instance = _locator.GetInstance("/some/path/here");
			
			Assert.That(instance.File, Is.EqualTo("2345.pid"));
			Assert.That(instance.ProcessID, Is.EqualTo(2345));
			Assert.That(instance.Key, Is.EqualTo("/some/path"));
			Assert.That(instance.Port, Is.EqualTo(876));
		}
		
		[Test]
		public void Should_matching_on_two_paths_choose_closest_path()
		{
			_client.Stub(x => x.IsConnected).Return(true);
			_fs.Stub(x => x.GetFiles(Path.Combine(Path.GetTempPath(), "EditorEngine"), "*.pid")).Return(new string[] { "123.pid", "2345.pid", "8754.pid" });
			_fs.Stub(x => x.ReadLines("123.pid")).Return(new string[] { "/some/path", "234" });
			_fs.Stub(x => x.ReadLines("2345.pid")).Return(new string[] { "/some/path/here", "876" });
			_fs.Stub(x => x.ReadLines("8754.pid")).Return(new string[] { "Other stuff", "1" });
			var instance = _locator.GetInstance("/some/path/here");
			
			Assert.That(instance.File, Is.EqualTo("2345.pid"));
			Assert.That(instance.ProcessID, Is.EqualTo(2345));
			Assert.That(instance.Key, Is.EqualTo("/some/path/here"));
			Assert.That(instance.Port, Is.EqualTo(876));
		}
		
		[Test]
		public void Should_not_pick_instances_where_it_cannot_connect_to_host()
		{
			_fs.Stub(x => x.GetFiles(Path.Combine(Path.GetTempPath(), "EditorEngine"), "*.pid")).Return(new string[] { "123.pid" });
			_fs.Stub(x => x.ReadLines("123.pid")).Return(new string[] { "/some/path/here", "234" });
			var instance = _locator.GetInstance("/some/path/here");
			
			Assert.That(instance, Is.Null);
		}
		
		[Test]
		public void Should_delete_files_where_it_cannot_connect_to_host()
		{
			_fs.Stub(x => x.GetFiles(Path.Combine(Path.GetTempPath(), "EditorEngine"), "*.pid")).Return(new string[] { "123.pid" });
			_fs.Stub(x => x.ReadLines("123.pid")).Return(new string[] { "/some/path/here", "234" });
			_locator.GetInstance("/some/path/here");
			
			_fs.AssertWasCalled(x => x.DeleteFile("123.pid"));
		}
	}
}

