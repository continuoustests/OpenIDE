#!/usr/bin/env python
import sys
import os
sys.path.append(os.path.dirname(__file__))
import tests

def getTests():
	return {
		"Can create and load local profile":
			canCreateLocalProfile,
		"Can create and load global profile":
			canCreateGlobalProfile
	}

def canCreateLocalProfile():
	tests.out("command|profile init new-local-profile")
	tests.out("command|profile load new-local-profile")
	tests.out("command|profile list")
	tests.assertOn(tests.hasOutput("Active local profile:  new-local-profile"))

def canCreateGlobalProfile():
	tests.out("command|profile init new-global-profile -g")
	tests.out("command|profile load new-global-profile -g")
	tests.out("command|profile list")
	tests.assertOn(tests.hasOutput("Active global profile: new-global-profile"))

if __name__ == "__main__":
	tests.main("initialized", getTests)