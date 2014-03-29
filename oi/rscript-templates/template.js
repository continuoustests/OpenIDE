#!/usr/bin/env node

if (process.argv.length > 2) {
    if (process.argv[2] === "reactive-script-reacts-to") {
        // Write one event pr line that this script will react to
        // console.log("goto*.js|*");
        process.exit();
    }
}

// Write scirpt code here.
//  Param 1: event
//  Param 2: global profile name
//  Param 3: local profile name
//
// When calling other commands use the --profile=PROFILE_NAME and 
// --global-profile=PROFILE_NAME argument to ensure calling scripts
// with the right profile.
