#!/bin/bash

echo "initialized"
while IFS='|' read -ra COMMAND; do
	if [ "${COMMAND[0]}" == "shutdown" ] ; then
		break
	fi
	if [ "${COMMAND[0]}" == "get-tests" ] ; then
		echo "A inconclusive test"
	fi
	if [ "${COMMAND[0]}" == "test" ] ; then
		case "${COMMAND[1]}" in
			"A inconclusive test")
				;;
		esac
	fi
	echo "end-of-conversation"
done