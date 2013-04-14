package main

import (
	"fmt"
	"os"
	"io"
	"path"
	"bufio"
    "bytes"
	"strconv"
	"path/filepath"
	"go/parser"
	"go/token"
	"go/ast"
)

type CrawlCommand struct {
}

func (cmd CrawlCommand) RegisterCommandDefinitions() {
}

func (cmd CrawlCommand) HandlesScenario(args []string) bool {
	if (len(args) != 2) {
		return false
	}
	return args[0] == "crawl-source"
}

func (cmd CrawlCommand) Run(args []string) {
	paramFile := args[1]
	lines, err := readLines(paramFile)
	if (err != nil) {
		fmt.Println(err)
		return;
	}
	for _, line := range lines {
		file, err := os.Stat(line)
		if (err == nil) {
			if (file.IsDir()) {
				crawlDir(line)
			} else {
				crawlFile(line)
			}
		}
	}
}

func crawlDir(name string) {
	filepath.Walk(name, handlePath)
}

func handlePath(name string, info os.FileInfo, err error) error {
	if (err != nil) {
		fmt.Println(err)
		return nil;
	}
	if (info.IsDir()) {
		return nil;
	}
	if (path.Ext(name) != ".go") {
		return nil;
	}
	crawlFile(name)
	return nil
}

func readLines(name string) (lines []string, err error) {
    var (
        file *os.File
        part []byte
        prefix bool
    )
    if file, err = os.Open(name); err != nil {
        return
    }
    reader := bufio.NewReader(file)
    buffer := bytes.NewBuffer(make([]byte, 1024))
	buffer.Reset()
    for {
        if part, prefix, err = reader.ReadLine(); err != nil {
            break
        }
        buffer.Write(part)
        if !prefix {
            lines = append(
				lines,
				buffer.String())
            buffer.Reset()
        }
    }
	if err == io.EOF {
        err = nil
    }
    return
}

func crawlFile(name string) {
	fset := token.NewFileSet() // positions are relative to fset

	f, err := parser.ParseFile(fset, name, nil, 0)
	if err != nil {
		fmt.Println(err)
		return
	}

	fmt.Printf("file|%s|filesearch", name)
	fmt.Println("")

	packageName := getPackageName(fset, f)

	// Print the imports from the file's AST.
	for _, s := range f.Decls {
		var parent, typename, typedefinition string
		var line, column int
		switch x := s.(type) {
			case *ast.FuncDecl:
				typename = x.Name.Name
				line = fset.Position(x.Name.NamePos).Line
				column = fset.Position(x.Name.NamePos).Column
				if (x.Name.Obj != nil) {
					typedefinition = x.Name.Obj.Kind.String()
					parent = ""
				} else {
					typedefinition = "func"
				}
				if (x.Recv != nil) {
					for _, t := range x.Recv.List {
						switch u := t.Type.(type) {
							case *ast.Ident:
								parent = "." + u.Obj.Decl.(*ast.TypeSpec).Name.Name
						}
					}
				}
			case *ast.GenDecl:
				for _, c := range x.Specs {
					switch d := c.(type) {
						case *ast.TypeSpec:
							typename = d.Name.Name
							line = fset.Position(d.Name.NamePos).Line
							column = fset.Position(d.Name.NamePos).Column
							switch d.Type.(type) {
								case *ast.InterfaceType:
									typedefinition = "interface"	
								case *ast.StructType:
									typedefinition = "struct"
							}
					}		
				}
		}
		if (len(typename) > 0) {
			fmt.Printf("signature|%s|%s|%s|%s||%s|%s||typesearch",
				packageName + parent,
				packageName + parent + "." + typename,
				typename,
				typedefinition,
				strconv.Itoa(line),
				strconv.Itoa(column))
			fmt.Println("")
		}
	}
}

func getPackageName(fset *token.FileSet, f *ast.File) string {
	name := f.Name.Name
	fmt.Printf("signature||%s|%s|package||%s|%s|",
		name,
		name,
		strconv.Itoa(fset.Position(f.Name.NamePos).Line),
		strconv.Itoa(fset.Position(f.Name.NamePos).Column))
	fmt.Println("")
	return name
}
