#!/usr/bin/env python
import sys
import os
sys.path.append(os.path.dirname(__file__))
import tests

def getTests():
	return {
		"When adding a simple setting we can read it out":
			canReadout,
		"When finding a oicfgoptions file it is listed by the list command":
			canFindCfgOption
	}

def writeToFile(file, content):
	f = open(file, "w")
	try:
		f.write(content)
	finally:
		f.close()

def canReadout():
	tests.out("command|conf test.setting=34")
	tests.out("command|conf read test.setting")
	tests.assertOn(tests.hasOutput("34"))

def canFindCfgOption():
	directory = os.path.join(sys.argv[1], ".OpenIDE")
	file = os.path.join(directory, "test.oicfgoptions")
	writeToFile(file, "custom.setting|This is a custom setting")
	tests.out("command|conf list")
	expected = "custom.setting                          // This is a custom setting"
	tests.assertOn(tests.hasOutput(expected))

if __name__ == "__main__":
	tests.main("initialized", getTests)