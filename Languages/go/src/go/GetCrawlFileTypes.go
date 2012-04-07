package main

import "fmt"
import "flag"

type GetCrawlFileTypesCommand struct {
}

func (cmd GetCrawlFileTypesCommand) RegisterCommandDefinitions() {
}

func (cmd GetCrawlFileTypesCommand) HandlesScenario() bool {
	args := flag.Args()
	if (len(args) != 1) {
		return false
	}
	return args[0] == "crawl-file-types"
}

func (cmd GetCrawlFileTypesCommand) Run() {
	fmt.Println(".go")
}
