#!/usr/bin/env ruby

working_directory = ARGV[0]

t1 = Thread.new do
     %x[mono ./CodeEngine/OpenIDENet.CodeEngine.exe "#{working_directory}"]
end
t2 = Thread.new do
     %x[mono ./ContinuousTests/ContinuousTests.exe "#{working_directory}"]
end
#%x[./AutoTest.Net/AutoTest.WinForms.exe "#{working_directory}"]

sleep 5
t1.kill
t2.kill

