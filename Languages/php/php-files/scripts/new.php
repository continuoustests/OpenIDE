#!/usr/bin/env php

<?php
# Script parameters
#	Param 1: Script run location
#	Param 2: global profile name
#	Param 3: local profile name
#	Param 4-: Any passed argument
#
# When calling oi use the --profile=PROFILE_NAME and 
# --global-profile=PROFILE_NAME argument to ensure calling scripts
# with the right profile.
#
# To post back oi commands print command prefixed by command| to standard output
# To post a comment print to std output prefixed by comment|
# To post an error print to std output prefixed by error|

	if (count($argv) == 3) {
		if ($argv[2] == "get-command-definitions") {
			# Definition format usually represented as a single line:

			# Script description|
			# command1|"Command1 description"
			# 	param|"Param description" end
			# end
			# command2|"Command2 description"
			# 	param|"Param description" end
			# end

			echo "Creates a new php file\n";
			exit();
		}
	}

	if (count($argv) == 5) {
		$file = $argv[1] . DIRECTORY_SEPARATOR . $argv[4];
		if (!endsWith($file, ".php")) {
			$file = $file . ".php";
		}
		$directory = dirname($file);
		if (!is_dir($directory)) {
			mkdir($directory, 0777, true);
		}
		$fp = fopen($file, 'w');
		fwrite($fp, "<?php\n\t\n?>");
		fclose($fp);
		echo "command|editor goto \"" . $file . "|2|2\"\n";
		waitForEndOfCommand();
	}

	function waitForEndOfCommand() {
		while (TRUE) {
			$line = readline();
			if ($line == "end-of-command")
				break;
			echo $line;
		}
	}

	function endsWith($haystack, $needle) {
	    return $needle === "" || substr($haystack, -strlen($needle)) === $needle;
	}
?>