package main

import "fmt"

type InitializeCommand struct {
}

func (cmd InitializeCommand) RegisterCommandDefinitions() {
}

func (cmd InitializeCommand) HandlesScenario(args []string) bool {
	if (len(args) != 1) {
		return false
	}
	return args[0] == "initialize"
}

func (cmd InitializeCommand) Run(args []string) {
	fmt.Println("initialized")
}
