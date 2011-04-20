#!/usr/bin/env ruby

working_directory = ARGV[0]

%x[./AutoTest.Net/AutoTest.WinForms.exe "#{working_directory}"]
