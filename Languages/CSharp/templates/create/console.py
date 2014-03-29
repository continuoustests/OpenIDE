#!/usr/bin/env python
import sys
from files.copydir import copy as copydir

if __name__ == "__main__":
    if sys.argv[1] == 'get_file':
        print("Program.cs")
    elif sys.argv[1] == 'get_position':
        print("8|3")
    elif sys.argv[1] == 'get_definition':
        print("Creates a new C# console application")
    else:
        copydir("console", sys.argv[1])
