package main

import "flag"

func main() {
	cmds := getCommands()
	prepareCommands(cmds)
	cmd := getCommand(cmds)
	if (cmd != nil) {
		cmd.Run()
	}
}

func getCommands() []Command {
	return []Command{
		InitializeCommand{},
		GetCommandDefinitionsCommand{},
		GetCrawlFileTypesCommand{},
		CrawlCommand{}}
}

func prepareCommands(cmds []Command) {
	for _, cmd := range cmds {
		cmd.RegisterCommandDefinitions()
	}
	flag.Parse()
}

func getCommand(cmds []Command) Command {
	for _, cmd := range cmds {
		if (cmd.HandlesScenario()) {
			return cmd
		}
	}
	return nil;
}

type Command interface {
	RegisterCommandDefinitions()
	HandlesScenario() bool
	Run()
}
