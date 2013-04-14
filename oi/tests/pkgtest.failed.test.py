#!/usr/bin/env python
import sys
import os
sys.path.append(os.path.dirname(__file__))
import tests

def getTests():
	return {
		"A failing test":
			failingTest
	}

def failingTest():
	tests.assertOn(False)

if __name__ == "__main__":
	tests.main("initialized", getTests)