#!/usr/bin/env ruby
# First parameter is the execution location of this script instance

if ARGV[1] == "get-command-definitions"
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
