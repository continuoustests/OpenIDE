#!/bin/bash 

if [ "$1" = "reactive-script-reacts-to" ]; then
	echo "goto*.cs|*"
	exit
fi
notify-send "I am here "$1""
