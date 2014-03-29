#!/usr/bin/env python
import sys
from files.copydir import copy as copydir

if __name__ == "__main__":
    if sys.argv[1] == 'get_file':
        print("")
    elif sys.argv[1] == 'get_position':
        print("")
    elif sys.argv[1] == 'get_definition':
        print("Creates a new C# library project")
    else:
        copydir("library", sys.argv[1])
