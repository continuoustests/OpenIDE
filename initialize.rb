#!/usr/bin/env ruby

working_directory = ARGV[0]

%x[mono ./ContinuousTests/ContinuousTests.exe "#{working_directory}"]
#%x[./AutoTest.Net/AutoTest.WinForms.exe "#{working_directory}"]
