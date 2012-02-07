#!/usr/bin/env ruby
require 'rbconfig'

WINDOWS = RbConfig::CONFIG['host_os'] =~ /mswin|mingw/

working_directory = ARGV[0]
default_language = ""
if ARGV.length > 1
	default_language = ARGV[1]
end
enabled_languages = ""
if ARGV.length > 2
	enabled_languages = ARGV[2]
end

t1 = Thread.new do
	# Or if you want to use AutoTest.Net
	if WINDOWS
		%x[bin/AutoTest.Net/AutoTest.WinForms.exe "#{working_directory}"]
	else
		%x[mono ./bin/AutoTest.Net/AutoTest.WinForms.exe "#{working_directory}"]
	end
	#if WINDOWS
	#	%x[bin/ContinuousTests/ContinuousTests.exe "#{working_directory}"]
	#else
	#	%x[mono ./bin/ContinuousTests/ContinuousTests.exe "#{working_directory}"]
	#end
end

sleep 5
t1.kill

