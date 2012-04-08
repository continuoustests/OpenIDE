#!/bin/bash

if [ "$1" = "reactive-script-reacts-to" ]; then
	# Write one event pr line that this script will react to
	echo "codemodel filesystem-change*.go*"
	exit
fi

# Write scirpt code here. First parameter contains the event
if [[ "$1" == *.swp ]]; then
	exit
fi

iconOK="`dirname \"$0\"`/../graphics/circleWIN.png"
iconFAIL="`dirname \"$0\"`/../graphics/circleFAIL.png"
file=$(echo "$1"|cut -d' ' -f 3)
dirlocation=`dirname "$file"`
cd "$dirlocation"
output=$(go build)
if [ "$output" = "" ]; then
	testoutput=`go test|sed '{s/---//g}'`
	failed=false
	if [[ "$testoutput" == *FAIL* ]]; then
		failed=true
	fi
	if [ "$testoutput" == "" ]; then
		failed=true
		testoutput="Tests did not compile"
	fi
	
	if [ failed ]; then
		notify-send --icon="$iconFAIL" "Tests Failed" "$testoutput"
	else
		notify-send --icon="$iconOK" "Build And Tests Succeeded" "$testoutput"
	fi
else
	notify-send --icon="$iconFAIL" "Build Failed" "$output"
fi


