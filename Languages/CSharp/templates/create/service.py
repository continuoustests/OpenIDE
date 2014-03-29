#!/usr/bin/env python
import sys
from files.copydir import copy as copydir

if __name__ == "__main__":
    if sys.argv[1] == 'get_file':
        print("Service.cs")
    elif sys.argv[1] == 'get_position':
        print("22|10")
    elif sys.argv[1] == 'get_definition':
        print("Creates a new C# windows service")
    else:
        copydir("service", sys.argv[1])
