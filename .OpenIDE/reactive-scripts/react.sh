#!/bin/bash
#if [ "foo" = "foo" ]; then
#	echo expression evaluated as true
#fi
			
#!/bin/bash 

if [ "$1" = "reactive-script-reacts-to" ]; then
	echo "goto"
	exit
fi
notify-send "\"I am here " + $1 + "\""
