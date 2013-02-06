package main

import (
	"os"
	"fmt"
	"flag"
	"bufio"
	"strings"
)

func main() {
	flag.Parse()
	args := flag.Args()
	if (len(args) == 0) {
		return
	}

	cmds := getCommands()
	prepareCommands(cmds)
	
	if (args[0] == "initialize") {
		reader := bufio.NewReader(os.Stdin)
		fmt.Println("initialized")
		for {
			line, err := reader.ReadString('\n')
			if (err == nil) {
				line = strings.TrimSpace(line)
				arguments := getArgs(line)
				if (line == "shutdown") {
					fmt.Println("end-of-conversation")
					break
				} else {
					cmd := getCommand(cmds, arguments)
					if (cmd != nil) {
						cmd.Run(arguments)
					}
				}
			}
			fmt.Println("end-of-conversation")
		}
	} else {
		cmd := getCommand(cmds, args)
		if (cmd != nil) {
			cmd.Run(args)
		}
	}
}

func getArgs(cmd string) []string {
	var args []string
	word := ""
	inQuotes := false
	for _,rune := range cmd {
		c := string(rune)
		if (c == "\"") {
			inQuotes = !inQuotes
		} else if (c != " " || inQuotes) {
			word += c
		} else {
			args = append(args, word)
			word = ""
		}
	}
	if (word != "") {
		args = append(args, word)
	}
	return args
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
}

func getCommand(cmds []Command, args []string) Command {
	for _, cmd := range cmds {
		if (cmd.HandlesScenario(args)) {
			return cmd
		}
	}
	return nil;
}

type Command interface {
	RegisterCommandDefinitions()
	HandlesScenario([]string) bool
	Run([]string)
}
