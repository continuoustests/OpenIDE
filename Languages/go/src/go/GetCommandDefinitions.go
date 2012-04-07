package main

import "flag"

type GetCommandDefinitionsCommand struct {
}

func (cmd GetCommandDefinitionsCommand) RegisterCommandDefinitions() {
}

func (cmd GetCommandDefinitionsCommand) HandlesScenario() bool {
	args := flag.Args()
	if (len(args) != 1) {
		return false
	}
	return args[0] == "get-command-definitions"
}

func (cmd GetCommandDefinitionsCommand) Run() {
}
