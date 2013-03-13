#!/bin/bash

# Parameter 1: Current directory for test oi session

echo "initialized"
#echo "initialized|"
while IFS='|' read -ra COMMAND; do
	if [ "${COMMAND[0]}" == "shutdown" ] ; then
		break
	fi
	if [ "${COMMAND[0]}" == "get-tests" ] ; then
		echo "Should be able to run command then verify the output (global profiles)"
		echo "This is a nonsense test that does not complete"
		echo "This is another one that fails"
	fi
	if [ "${COMMAND[0]}" == "test" ] ; then
		case "${COMMAND[1]}" in
			"Should be able to run command then verify the output (global profiles)")
				echo "command|profile list"
				echo "hasoutput|Global profiles:"
				IFS='|' read -ra VALUE
				if [ "${VALUE}" != "true" ] ; then
					echo "failed|"
				else
					echo "passed|"
				fi
				;;
			"This is a nonsense test that does not complete")
				;;
			"This is another one that fails")
				echo "failed|This test is not supposed to work"
				;;				
		esac
	fi
	echo "end-of-conversation"
done