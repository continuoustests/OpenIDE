#!/usr/bin/env python
import sys
import os
sys.path.append(os.path.dirname(__file__))
import tests

def getTests():
	return {
		"When test produces events we should be able to assert on them":
			canAssertOnEvents
	}

def canAssertOnEvents():
	file = os.path.join(sys.argv[1], "bleh.txt")
	tests.out("command|touch bleh.txt")
	tests.out("command|conf read test.setting")
	event = "codemodel raw-filesystem-change-filecreated \"" + file + "\""
	tests.assertOn(tests.hasEvent(event))

if __name__ == "__main__":
	tests.main("initialized|editor", getTests)