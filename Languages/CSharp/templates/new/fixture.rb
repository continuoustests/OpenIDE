#!/usr/bin/env ruby

if ARGV[0] == 'get_file_extension'
	puts '.cs'
	exit
end

if ARGV[0] == 'get_position'
	puts '11|4'
	exit
end

if ARGV[0] == 'get_definition'
	puts "Creates an empty NUnit test fixture for C#"
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
		[Test]
		public void Test()
		{
		}
	}
}"
