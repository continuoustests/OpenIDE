#!/usr/bin/env ruby

if ARGV[0] == "get-command-definitions"
	# Definition format usually represented as a single line:

	# Script description|
	# command1|"Command1 description"
	# 	param|"Param description" end
	# end
	# command2|"Command2 description"
	# 	param|"Param description" end
	# end

	puts "Script description"
	exit
end
