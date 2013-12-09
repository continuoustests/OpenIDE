#!/usr/bin/env python
import os
import sys
import subprocess
sys.path.append(os.path.dirname(__file__))
import tests

def runProcess(exe):    
	p = subprocess.Popen(exe, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
	while(True):
		retcode = p.poll() # returns None while subprocess is running
		line = p.stdout.readline().decode(encoding='windows-1252').strip('\n').strip('\r')
		if line != "":
			yield line
		if(retcode is not None):
			break

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
	runTest("pkgtest.passing.test.py", "PASSED A passing test")

def canFailTest():
	runTest("pkgtest.failed.test.py", "FAILED A failing test")

def canMarkTestAsInconclusuve():
	runTest("pkgtest.inconclusive.test.py", "?????? A inconclusive test")

def runTest(testfile, expectedcoutput):
	root = os.path.dirname(__file__)
	passed = False
	for line in runProcess(["oi", "package", "test", os.path.join(root, testfile)]):
		if expectedcoutput in line:
			passed = True

	if passed == False:
		tests.out("failed")
	else:
		tests.out("passed")

if __name__ == "__main__":
	tests.main("initialized", getTests)