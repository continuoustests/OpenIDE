package main

type GetCommandDefinitionsCommand struct {
}

func (cmd GetCommandDefinitionsCommand) RegisterCommandDefinitions() {
}

func (cmd GetCommandDefinitionsCommand) HandlesScenario(args []string) bool {
	if (len(args) != 1) {
		return false
	}
	return args[0] == "get-command-definitions"
}

func (cmd GetCommandDefinitionsCommand) Run(args []string) {
}
