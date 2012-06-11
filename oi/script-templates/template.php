#!/usr/bin/env php
<?php
	if (count($argv) > 1) {
		if ($argv[1] == "get-command-definitions") {
			# Definition format usually represented as a single line:

			# Script description|
			# command1|"Command1 description"
			# 	param|"Param description" end
			# end
			# command2|"Command2 description"
			# 	param|"Param description" end
			# end

			echo "Script description\n";
			return;
		}
	}
?>
