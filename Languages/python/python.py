#!/usr/bin/env python

import sys
import ast
import os

class AstVisitor(ast.NodeVisitor):
    def visit_ClassDef(self, node):
        print 'signature||' + node.name + '|' + node.name + \
              '|class|public|' + str(node.lineno) + '|' + \
              str(node.col_offset + len('class ') + 1) + '||typesearch'
        for item in node.body:
            if item.name.startswith('__') == False:
                print 'signature|' + node.name + '|' + node.name + '.' + item.name + '|' + item.name + \
                      '|classmethod|public|' + str(item.lineno) + '|' + \
                      str(item.col_offset + len('def ') + 1) + '|'

    def visit_FunctionDef(self, node):
        if node.name.startswith('__') == False:
            print 'signature||' + node.name + '|' + node.name + \
                  '|method|public|' + str(node.lineno) + '|' + \
                  str(node.col_offset + len('def ') + 1) + '||typesearch'
        else:
            print 'signature||' + node.name + '|' + node.name + \
                  '|method|private|' + str(node.lineno) + '|' + \
                  str(node.col_offset + len('def ') + 1) + '|'

def __handleDirectory(path):
    items = os.listdir(path);
    for dr in items:
        dir = os.path.join(path, dr)
        if (os.path.isdir(dir)):
            __handleDirectory(dir)
    
    for file in items:
        filename = os.path.join(path, file)
        if (os.path.isfile(filename)):
            extension = os.path.splitext(filename)[1]
            if extension == '.py':
                __handleFile(filename)

def __handleFile(file):
    try:
        print 'file|' + file + '|filesearch'
        node = ast.parse(open(file, 'r').read())
        node = ast.fix_missing_locations(node)
        v = AstVisitor()
        v.visit(node)
    except Exception,e:
        print 'error|Failed to parse ' + file
        print 'error|' + str(e)

def main(argv):
    if len(argv) > 1:
        if argv[1] == 'initialize':
            return
        if argv[1] == 'get-command-definitions':
            print ''
            return
        if argv[1] == 'crawl-file-types':
            print '.py'
            return
        if argv[1] == 'crawl-source':
            if os.path.isfile(argv[2]):
                with open(argv[2]) as file:
                    lines = file.readlines()
                    for ln in lines:
                        line = ln.replace(os.linesep, '')
                        if os.path.isdir(line):
                            __handleDirectory(line)
                        else:
                            __handleFile(line)
            else:
                __handleDirectory(argv[2])

if __name__ == "__main__":
    main(sys.argv)
