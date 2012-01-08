#!/usr/bin/env ruby
require 'rbconfig'

WINDOWS = RbConfig::CONFIG['host_os'] =~ /mswin|mingw/

working_directory = ARGV[0]
default_language = ""
if ARGV.length > 1
	default_language = ARGV[1]
end

t1 = Thread.new do
	if WINDOWS
		%x[CodeEngine/OpenIDENet.CodeEngine.exe "#{working_directory}" "#{default_language}"]
	else
		%x[mono ./CodeEngine/OpenIDENet.CodeEngine.exe "#{working_directory}" "#{default_language}"]
	end
end
t2 = Thread.new do
	# Or if you want to use AutoTest.Net
	# AutoTest.Net/AutoTest.WinForms.exe "#{working_directory}"
	if WINDOWS
		%x[ContinuousTests/ContinuousTests.exe "#{working_directory}"]
	else
		%x[mono ./ContinuousTests/ContinuousTests.exe "#{working_directory}"]
	end
end

sleep 5
t1.kill
t2.kill

