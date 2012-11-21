package main

import "fmt"
import "flag"

type InitializeCommand struct {
}

func (cmd InitializeCommand) RegisterCommandDefinitions() {
}

func (cmd InitializeCommand) HandlesScenario() bool {
	args := flag.Args()
	if (len(args) != 1) {
		return false
	}
	return args[0] == "initialize"
}

func (cmd InitializeCommand) Run() {
	fmt.Println("initialized")
}
