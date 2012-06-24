#!/usr/bin/env node

if (process.argv.length > 2) {
	if (process.argv[2] === "reactive-script-reacts-to") {
		// Write one event pr line that this script will react to
		// console.log("goto*.cs|*");
		process.exit();
	}
}

// Write scirpt code here. First parameter contains the event
