#!/usr/bin/env python
import sys

if __name__ == "__main__":
    if sys.argv[1] == 'get_file_extension':
        print(".cs")
    elif sys.argv[1] == 'get_position':
        print("7|6")
    elif sys.argv[1] == 'get_definition':
        print("Creates an empty C# class")
    else:
        classname = sys.argv[1]
        namespace = sys.argv[2]
        parameterfile = sys.argv[3]
        print("using System;")
        print("")
        print("namespace " + namespace)
        print("{")
        print("    class " + classname)
        print("    {")
        print("    }")
        print("}")