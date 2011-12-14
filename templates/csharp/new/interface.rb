#!/usr/bin/env ruby

if ARGV[0] == 'get_file_extension'
	puts '.cs'
	exit
end

if ARGV[0] == 'get_position'
	puts '6|3'
	exit
end

if ARGV[0] == 'get_definition'
	puts "Creates an new C# interface"
	exit
end

interfaceName = ARGV[0]
namespace = ARGV[1]
parameterfile = ARGV[2]

puts "using System;

namespace #{namespace}
{
	interface #{interfaceName}
	{
	}
}"
