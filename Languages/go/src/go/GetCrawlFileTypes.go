package main

import "fmt"

type GetCrawlFileTypesCommand struct {
}

func (cmd GetCrawlFileTypesCommand) RegisterCommandDefinitions() {
}

func (cmd GetCrawlFileTypesCommand) HandlesScenario(args []string) bool {
	if (len(args) != 1) {
		return false
	}
	return args[0] == "crawl-file-types"
}

func (cmd GetCrawlFileTypesCommand) Run(args []string) {
	fmt.Println(".go")
}
