#!/usr/bin/env ruby

if ARGV[0] == 'get_file_extension'
	puts '.cs'
	exit
end

classname = ARGV[0]
namespace = ARGV[1]
parameterfile = ARGV[2]

puts "using System;
using NUnit.Framework;

namespace #{namespace}
{
	[TestFixture]
	public class #{classname}
	{
		private ClassToTest _toTest;
		
		[SetUp]
		public void Setup()
		{
			_toTest = new ClassToTest();
		}
		
		[Test]
		public void Test()
		{
		}
	}
}"
