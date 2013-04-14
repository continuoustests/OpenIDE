#!/usr/bin/env python
import sys
import os
sys.path.append(os.path.dirname(__file__))
import tests

def getTests():
	return {
		"When running the touch command a file should be created and a goto event should be emitted":
			canRunTouch
	}

def canRunTouch():
	tests.out("command|touch bleh.txt")
	result = False
	file = os.path.join(sys.argv[1], "bleh.txt")
	event = "goto \"" + file + "|0|0\""
	result = tests.hasEvent(event)
	if result:
		result = os.path.exists(file)
	tests.assertOn(result)

if __name__ == "__main__":
	tests.main("initialized|editor", getTests)