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
			canFindCfgOption,
		"Can read config file location":
			canFindLocalCfgFile,
		"Can read config point":
			canReadCfgPoint,
		"Can read root point":
			canReadRootPoint,
		"Can remove setting":
			canRemoveSetting,
		"Can append to setting":
			canAppendToSetting,
		"Can remove from setting":
			canRemoveFromSetting,
		"Can write and read global setting":
			canWriteReadGlobalSetting,
		"Can read globale config file location":
			canReadGlobalCfgFile,
		"Can read global config point":
			canReadGlobalCfgPoint,
		"Can read global root point":
			canReadGlobalRootPoint,
		"Can remove global setting":
			canRemoveGlobalSetting
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

def canFindLocalCfgFile():
	cfgfile = os.path.join(os.path.join(sys.argv[1], ".OpenIDE"), "oi.config")
	tests.out("command|conf read cfgfile")
	tests.assertOn(tests.hasOutput(cfgfile))

def canReadCfgPoint():
	cfgfile = os.path.join(sys.argv[1], ".OpenIDE")
	tests.out("command|conf read cfgpoint")
	tests.assertOn(tests.hasOutput(cfgfile))

def canReadRootPoint():
	root = sys.argv[1]
	tests.out("command|conf read rootpoint")
	tests.assertOn(tests.hasOutput(root))

def canRemoveSetting():
	tests.out("command|conf test.removedsetting=34")
	tests.out("command|conf test.removedsetting -d")
	tests.out("command|conf read")
	tests.assertOn(tests.hasOutput("test.removedsetting=34") == False)

def canAppendToSetting():
	tests.out("command|conf test.appendsetting=hei")
	tests.out("command|conf test.appendsetting+=ja")
	tests.out("command|conf read")
	tests.assertOn(tests.hasOutput("test.appendsetting=hei,ja"))

def canRemoveFromSetting():
	tests.out("command|conf test.reducedsetting=hei,men,ja")
	tests.out("command|conf test.reducedsetting-=men")
	tests.out("command|conf read")
	tests.assertOn(tests.hasOutput("test.reducedsetting=hei,ja"))

def canWriteReadGlobalSetting():
	# Clean up in case of previous failing test
	tests.out("command|conf test.globalsetting -g -d")
	tests.out("command|conf test.globalsetting=20 -g")
	tests.out("command|conf read -g")
	tests.assertOn(tests.hasOutput("test.globalsetting=20"))
	tests.out("command|conf test.globalsetting -g -d")

def canReadGlobalCfgFile():
	root = tests.get("applocation")
	file = os.path.join(os.path.join(root, ".OpenIDE"), "oi.config")
	tests.out("command|conf read cfgfile -g")
	tests.assertOn(tests.hasOutput(file))

def canReadGlobalCfgPoint():
	cfgfile = os.path.join(tests.get("applocation"), ".OpenIDE")
	tests.out("command|conf read cfgpoint -g")
	tests.assertOn(tests.hasOutput(cfgfile))

def canReadGlobalRootPoint():
	root = tests.get("applocation")
	tests.out("command|conf read rootpoint -g")
	tests.assertOn(tests.hasOutput(root))

def canRemoveGlobalSetting():
	tests.out("command|conf test.globalremovedsetting=34 -g")
	tests.out("command|conf test.globalremovedsetting -g -d")
	tests.out("command|conf read -g")
	tests.assertOn(tests.hasOutput("test.globalremovedsetting=34") == False)

if __name__ == "__main__":
	tests.main("initialized", getTests)