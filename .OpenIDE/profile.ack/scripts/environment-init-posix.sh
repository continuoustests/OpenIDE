#!/bin/bash 

if [ "$2" = "get-command-definitions" ]; then
	echo "Initializing a freshly installed system|"
	echo "[[environment]]|\"\" "
	echo "	init|\"Initializes a freshly installed system\" end "
	echo "end "
	exit
fi

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
"$DIR/environment-init-posix-files/environment-init-generic.py"