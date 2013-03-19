#!/usr/bin/env python
import sys
import os
import copy
import subprocess

# Script parameters
#	Param 1: Script run location
#	Param 2: global profile name
#	Param 3: local profile name
#	Param 4-: Any passed argument
#
# When calling oi use the --profile=PROFILE_NAME and 
# --global-profile=PROFILE_NAME argument to ensure calling scripts
# with the right profile.
#
# To post back oi commands print command prefixed by command| to standard output
# To post a comment print to std output prefixed by comment|
# To post an error print to std output prefixed by error|

def runProcess(exe,workingDir):    
	p = subprocess.Popen(exe, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, cwd=workingDir)
	while(True):
		retcode = p.poll() #returns None while subprocess is running
		line = p.stdout.readline().decode(encoding='windows-1252').strip('\n')
		if line != "":
			return line
		if(retcode is not None):
			break
	return None

def printProcess(exe,workingDir):    
	p = subprocess.Popen(exe, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, cwd=workingDir)
	while(True):
		retcode = p.poll() #returns None while subprocess is running
		line = p.stdout.readline().decode(encoding='windows-1252').strip('\n')
		if line != "":
			sys.stdout.write(line + "\n")
		if(retcode is not None):
			break

def printDefinitions():
	# Definition format usually represented as a single line:

	# Script description|
	# command1|"Command1 description"
	# 	param|"Param description" end
	# end
	# command2|"Command2 description"
	# 	param|"Param description" end
	# end
	print("Run package manager from AutoTest.Net folder")

def main(argv):
	if len(argv) > 1:
		if argv[2] == 'get-command-definitions':
			printDefinitions()
			return

	path = runProcess("oi conf read rootpoint", argv[1])
	params = ""
	for param in argv[4:]:
		params = params + " " + param

	if os.name == "posix":
		printProcess("mono --debug \"" + path + "/PackageManager/oipckmngr/bin/AutoTest.Net/oipckmngr.exe\"" + params, argv[1])
	else:
		printProcess(path + "\\PackageManager\\oipckmngr\\bin\\AutoTest.Net\\oipckmngr.exe" + params, argv[1])

if __name__ == "__main__":
	main(sys.argv)
