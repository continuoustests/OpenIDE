#!/usr/bin/env php
<?php
	if (count($argv) > 1) {
		if ($argv[1] == "get-command-definitions")
			handleDefinition();

		if ($argv[1] == "crawl-file-types")
			handleTypes();

		if (count($argv) > 2) {
			if ($argv[1] == "crawl-source")
				crawlSource($argv[2]);
		}
	}

	function handleDefinition() {		
	}

	function handleTypes() {
		write(".php");
	}

	function crawlSource($file) {
		$lines = file($file);
		foreach ($lines as $line) {
			handleItem(str_replace("\n", "", $line));
		}
	}

	function handleItem($item) {
		if (is_dir($item)) {			
			if ($handle = opendir($item)) {
				while (false !== ($newItem = readdir($handle))) {
					if ($newItem == "." || $newItem == "..")
						continue;
					handleItem($item . DIRECTORY_SEPARATOR . $newItem);
				}
			}
		} else {
			if (endsWith($item, ".php"))
				handleFile($item);
		}
	}

	function endsWith($str, $match) {
		return (substr($str, strlen($str) - strlen($match)) === $match);
	}

	function handleFile($file) {
		$first = TRUE;
		$lineNum = 0;
		$lines = file($file);
		foreach ($lines as $line_str) {
			$l = str_replace("\n", "", $line_str);
            $lineNum++;
            if (strstr(strtolower($l), "function") != FALSE)
            {
                $line = trim($l);
                $paramPos = strpos($line, "(");
                $namePos = strpos(strtolower($line), "function") + strlen("function") + 1;
                if ($paramPos <= $namePos)
                    continue;
                if ($first) {
                    write("file|" . $file . "|filesearch");
                    $first = FALSE;
                }
                $name = substr($line, $namePos, $paramPos - $namePos);
                $signature = $name;
                $column = $namePos + 2;
                write("signature|" .
					  $signature . "|" . 
					  $name . 
					  "|function||" . 
					  $lineNum . "|" . 
					  $column . 
					  "||typesearch");
            }
        }
	}

	function write($s) {
		echo $s . "\n";
	}
?>
