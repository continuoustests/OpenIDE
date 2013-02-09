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

	echo "Run package manager from AutoTest.Net folder"
	exit
fi

cd $1
path=`oi configure read rootpoint`
mono --debug $path/PackageManager/oipckmngr/bin/AutoTest.Net/oipckmngr.exe "${@:4}"
