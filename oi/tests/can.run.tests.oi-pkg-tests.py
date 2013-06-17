#!/usr/bin/env python
import sys
import os
sys.path.append(os.path.dirname(__file__))
import tests

def getTests():
	return {
		"When given a passing test it should output passed":
			canPassTest,
		"When given a failing test it should output failed":
			canFailTest,
		"When given a inconclusive test it should output question marks":
			canMarkTestAsInconclusuve
	}

def canPassTest():
	root = os.path.dirname(__file__)
	tests.out("command|packagetest " + os.path.join(root, "pkgtest.passing.test.py"))
	tests.assertOn(tests.hasOutput("PASSED A passing test"))

def canFailTest():
	root = os.path.dirname(__file__)
	tests.out("command|packagetest " + os.path.join(root, "pkgtest.failed.test.py"))
	tests.assertOn(tests.hasOutput("FAILED A failing test"))

def canMarkTestAsInconclusuve():
	root = os.path.dirname(__file__)
	tests.out("command|packagetest " + os.path.join(root, "pkgtest.inconclusive.test.py"))
	tests.assertOn(tests.hasOutput("?????? A inconclusive test"))

if __name__ == "__main__":
	tests.main("initialized", getTests)