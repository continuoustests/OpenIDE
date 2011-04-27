#!/usr/bin/env ruby

working_directory = ARGV[0]

Thread.new do
     %x[mono ./CodeEngine/OpenIDENet.CodeEngine.exe "#{working_directory}"]
end
Thread.new do
     %x[mono ./ContinuousTests/ContinuousTests.exe "#{working_directory}"]
end
#%x[./AutoTest.Net/AutoTest.WinForms.exe "#{working_directory}"]
