#!/bin/bash 

# Script parameters
#	Param 1: Script run location
#	Param 2: global profile name
#	Param 3: local profile name
#	Param 4-: Any passed argument
#
# When calling oi use the --profile=PROFILE_NAME and 
# --global-profile=PROFILE_NAME argument to ensure calling scripts
# with the right profile.
#
# To post back oi commands print command prefixed by command| to standard output
# To post a comment print to std output prefixed by comment|
# To post an error print to std output prefixed by error|

if [ "$2" = "get-command-definitions" ]; then
	# Definition format usually represented as a single line:

	# Script description|
	# command1|"Command1 description"
	# 	param|"Param description" end
	# end
	# command2|"Command2 description"
	# 	param|"Param description" end
	# end

	echo "Php plugin dev options|"
	echo "crawl|\"Runs crawl-source on the plugin directory\" end "
	echo "new|\"Runs php language script new\" end "
	echo "new-definitions|\"Gets definitions for the new command\" end "
	exit
fi

if [ "$4" == "crawl" ]; then
	./Languages/php/php.php crawl-source Languages/php
fi
if [ "$4" == "new" ]; then
	location=$(pwd)/Languages/php/php-files/scripts
	./Languages/php/php-files/scripts/new.php "${location}" default default "${@:5}" 
fi
if [ "$4" == "new-definitions" ]; then
	./Languages/php/php-files/scripts/new.php get-command-definitions
fi
