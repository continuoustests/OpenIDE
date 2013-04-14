#!/usr/bin/env python
import os
import sys

def out(msg):
	sys.stdout.write(msg + "\n")
	sys.stdout.flush()

def assertOn(expression):
	if expression == True:
		out("passed")
	else:
		out("failed")

def hasOutput(value):
	out("hasoutput|" + value)
	line = sys.stdin.readline().rstrip()
	return line == "true"

def hasEvent(value):
	out("hasevent|" + value)
	line = sys.stdin.readline().rstrip()
	return line == "true"

def get(value):
	out("get|" + value)
	return sys.stdin.readline().rstrip()

def main(init, getTests):
	out(init)
	while True:
		line = sys.stdin.readline().rstrip()
		if line == "shutdown":
			break
		if line == "get-tests":
			tests = getTests()
			for test in tests:
				out(test)

		options = line.split('|')
		if options[0] == "test":
			test = getTests()[options[1]]
			test()

		out("end-of-conversation")