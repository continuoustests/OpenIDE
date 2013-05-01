#!/usr/bin/env python
import os
import sys
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

def printDefinitions():
	# Definition format usually represented as a single line:

	# Script description|
	# command1|"Command1 description"
	# 	param|"Param description" end
	# end
	# command2|"Command2 description"
	# 	param|"Param description" end
	# end
	sys.stdout.write("Rebuilds and downloads local package repository\n")

def runProcess(exe):    
    p = subprocess.Popen(exe, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    while(True):
        retcode = p.poll() # returns None while subprocess is running
        line = p.stdout.readline().decode(encoding='windows-1252').strip('\n').strip('\r')
        if line != "":
            yield line
        if(retcode is not None):
            break

def main(argv):
	if len(argv) > 1:
		if argv[2] == 'get-command-definitions':
			printDefinitions()
			return
	for item in runProcess(["oi","build-source","update","/home/ack/bin/oi-packages/local.source"]):
		print(item)
	for item in runProcess(["oi","package","src","update","local"]):
		print(item)

if __name__ == "__main__":
	main(sys.argv)
