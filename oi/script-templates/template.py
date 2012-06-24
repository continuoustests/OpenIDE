#!/usr/bin/env python

import sys

def main(argv):
	if len(argv) > 1:
		if argv[1] == 'get-command-definitions':
			# Definition format usually represented as a single line:

			# Script description|
			# command1|"Command1 description"
			# 	param|"Param description" end
			# end
			# command2|"Command2 description"
			# 	param|"Param description" end
			# end

			print "Script description"
			return

if __name__ == "__main__":
	main(sys.argv)
