#!/usr/bin/env php
<?php namespace language_plugin;
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
		if (is_dir($file)) {
			handleItem($file);
			return;
		}
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
			if (endsWith($item, ".php")) {
				$handler = new FileReader();
				$handler->handleFile($item);
			}
		}
	}

	function endsWith($str, $match) {
		return (substr($str, strlen($str) - strlen($match)) === $match);
	}

	class FileReader
	{
		private $namespace;

		function __construct() {
			$namespace = "";
		}

		public function handleFile($file) {
			$lineNum = 0;
			$lines = file($file);
	        write("file|" . $file . "|filesearch");
			foreach ($lines as $line_str) {
	            $lineNum++;
				$l = str_replace("\n", "", $line_str);
	            $line = trim($l);
	            if ($this->getNamespace($line, $lineNum))
	            	continue;
	            if ($this->getClass($line, $lineNum))
	            	continue;
			}
		}

		function getNamespace($line, $lineNum) {
			if (strstr(strtolower($line), "namespace")) {
				$item = $this->getKeyword("namespace", $line);
				if ($item == NULL) {
					return FALSE;
				}
				$signature = "";
	            $column = $item->Column + 2;
	            write("signature||" .
					  $signature . "|" . 
					  $item->Name . 
					  "|namespace||" . 
					  $lineNum . "|" . 
					  $column . 
					  "||");
	            $this->namespace = $item->Name;
	            return TRUE;
			}
			return FALSE;
		}

		function getClass($line, $lineNum) {
			if (strstr(strtolower($line), "class") != FALSE) {
				$item = $this->getKeyword("class", $line);
				if ($item == NULL) {
					return FALSE;
				}
	            $signature = $item->Name;
	            if ($this->namespace != "") {
	            	$signature = $this->namespace . "." . $signature;
	            }
	            $column = $item->Column + 2;
	            write("signature||" .
					  $signature . "|" . 
					  $item->Name . 
					  "|class||" . 
					  $lineNum . "|" . 
					  $column . 
					  "||typesearch");
	            return TRUE;
	        }
	        return FALSE;
		}

		function getKeyword($type, $line) {
			$paramPos = strpos($line, ":");
            if ($paramPos === FALSE) {
                $paramPos = strpos($line, "{");
                if ($paramPos === FALSE) {
                	$paramPos = strpos($line, ";");
                	if ($paramPos === FALSE) {
                		$paramPos = strlen($line);
                	}
                }
            }
            $namePos = strpos(strtolower($line), $type) + strlen($type) + 1;
            if ($paramPos <= $namePos)
            	return NULL;
            $name = trim(substr($line, $namePos, $paramPos - $namePos));
            if (strstr($name, " ") || strstr($name, ";") || strstr($name, ")")) {
            	return NULL;
            }
			return new Item($name, $namePos);
		}
	}

	class Item {
		public $Name = "";
		public $Column = 0;

		function __construct($name, $column) {
			$this->Name = $name;
			$this->Column = $column;
		}
	}

	function write($s) {
		echo $s . "\n";
	}
?>
