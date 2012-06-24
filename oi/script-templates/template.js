#!/usr/bin/env node
// First parameter is the execution location of this script instance

if (process.argv.length > 2) {
	if (process.argv[2] === "get-command-definitions") {
		// Definition format usually represented as a single line:

		// Script description|
		// command1|"Command1 description"
		// 	param|"Param description" end
		// end
		// command2|"Command2 description"
		// 	param|"Param description" end
		// end

		console.log("Script description");
		process.exit();
	}
}
