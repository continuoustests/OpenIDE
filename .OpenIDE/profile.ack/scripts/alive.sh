#!/bin/bash 
# First parameter is the execution location of this script instance

if [ "$2" = "get-command-definitions" ]; then
	# Definition format usually represented as a single line:

	# Script description|
	# command1|"Command1 description"
	# 	param|"Param description" end
	# end
	# command2|"Command2 description"
	# 	param|"Param description" end
	# end

	echo "Checks whether any of the starting processes are still alive|"
		echo "-k|\"Kills processes\""
	exit
fi

if [ "$4" = "-k" ]; then
	ps -Af|grep "OpenIDE"|awk '{print $2}'|xargs kill
else
	ps -Af|grep "OpenIDE"
fi