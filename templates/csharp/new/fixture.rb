#!/usr/bin/env ruby

if ARGV[0] == 'get_file_extension'
	puts '.cs'
	exit
end

if ARGV[0] == 'get_position'
	puts '19|4'
	exit
end

classname = ARGV[0]
classToTest = classname.gsub('Tests', '')
instanceName = "_#{classToTest[0].downcase}#{classToTest[1..(classToTest.length - 1)]}"
namespace = ARGV[1]
parameterfile = ARGV[2]

puts "using System;
using NUnit.Framework;

namespace #{namespace}
{
	[TestFixture]
	public class #{classname}
	{
		private #{classToTest} #{instanceName};
		
		[SetUp]
		public void Setup()
		{
			#{instanceName} = new #{classToTest}();
		}
		
		[Test]
		public void Test()
		{
		}
	}
}"
