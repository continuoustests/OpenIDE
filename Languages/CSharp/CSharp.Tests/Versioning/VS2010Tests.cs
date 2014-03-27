using System;
using NUnit.Framework;
using CSharp.Versioning;
using System.IO;
namespace CSharp.Tests.Versioning
{
	[TestFixture]
	public class VS2010Tests
	{
		[Test]
		public void Should_recognize_vs_2010_project()
		{
			var file = Path.GetTempFileName();
			File.WriteAllText(file, "<ProductVersion>9.0.21022</ProductVersion>");
			var ver = new VS2010();
			Assert.That(ver.IsValid(file), Is.True);
			File.Delete(file);
		}
		
		[Test]
		public void Should_not_recognize_non_existent_file()
		{
			var file = Path.GetTempFileName() + ".doesnot.exist";
			var ver = new VS2010();
			Assert.That(ver.IsValid(file), Is.False);
		}
	}
}

