package main

import (
	"os"
	"fmt"
	"bufio"
	"strings"
)

func main() {
	cmds := getCommands()
	prepareCommands(cmds)
	reader := bufio.NewReader(os.Stdin)
	for {
		line, err := reader.ReadString('\n')
		if (err == nil) {
			line = strings.TrimSpace(line)
			args := getArgs(line)
			if (line == "shutdown") {
				break;
			} else {
				cmd := getCommand(cmds, args)
				if (cmd != nil) {
					cmd.Run(args)
				}
				fmt.Println("end-of-conversation")
			}
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
