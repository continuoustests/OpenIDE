#!/bin/bash
#if [ "foo" = "foo" ]; then
#	echo expression evaluated as true
#fi
			
#!/bin/bash 

if [ "$1" = "" ]; then
	echo "goto"
	exit
fi
notify-send "\"I am here " + $1 + "\""
