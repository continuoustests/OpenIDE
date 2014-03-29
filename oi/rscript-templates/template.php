#!/usr/bin/env php
<?php
if (count($argv) == 2) {
    if ($argv[1] == "reactive-script-reacts-to") {
        # Write one event pr line that this script will react to
        # echo "goto*.php|*";
        exit(); 
    }
}

# Write scirpt code here.
#   Param 1: event
#   Param 2: global profile name
#   Param 3: local profile name
#
# When calling other commands use the --profile=PROFILE_NAME and 
# --global-profile=PROFILE_NAME argument to ensure calling scripts
# with the right profile.
