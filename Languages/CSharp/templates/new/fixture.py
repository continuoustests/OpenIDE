#!/usr/bin/env python
import sys

if __name__ == "__main__":
    if sys.argv[1] == 'get_file_extension':
        print(".cs")
    elif sys.argv[1] == 'get_position':
        print("11|4")
    elif sys.argv[1] == 'get_definition':
        print("Creates an empty NUnit test fixture for C#")
    else:
        classname = sys.argv[1]
        namespace = sys.argv[2]
        parameterfile = sys.argv[3]
        print("using System;")
        print("using NUnit.Framework;")
        print("")
        print("namespace " + namespace)
        print("{")
        print(" [TestFixture]")
        print(" public class " + classname)
        print(" {")
        print("     [Test]")
        print("     public void Test()")
        print("     {")
        print("     }")
        print(" }")
        print("}")
