#!/usr/bin/env php

<?php
# Script parameters
#   Param 1: Script run location
#   Param 2: global profile name
#   Param 3: local profile name
#   Param 4-: Any passed argument
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
        #   param|"Param description" end
        # end
        # command2|"Command2 description"
        #   param|"Param description" end
        # end

        echo "Creates a new php file|\n";
        echo "file|\"Create empty php file\" \n";
        echo "  FILE|\"Path to file\" end \n";
        echo "end \n";
        echo "class|\"Create new class\" \n";
        echo "  FILE|\"Path to file\" end \n";
        echo "end \n";
        echo "interface|\"Create new interface\" \n";
        echo "  FILE|\"Path to file\" end \n";
        echo "end \n";
        echo "fixture|\"Create new phpunit test fixture\" \n";
        echo "  FILE|\"Path to file\" end \n";
        echo "end \n";
        exit();
    }
}

if ($argc == 6) {
    $type = $argv[4];
    $filechunk = $argv[5];
    $file = $argv[1] . DIRECTORY_SEPARATOR . $filechunk;
    if (!endsWith($file, ".php")) {
        $file = $file . ".php";
    }
    $filename = basename($file, ".php");
    $directory = dirname($file);
    if (!is_dir($directory)) {
        mkdir($directory, 0777, true);
    }
    $line = writeFile($type, $file, $filename);
    echo "command|editor goto \"" . $file . "|" . $line . "|2\"\n";
    waitForEndOfCommand();
}

function writeFile($type, $file, $filename) {
    $line = 2;
    $namespace = getNamespace(dirname($file));
    $fp = fopen($file, 'w');
    fwrite($fp, "<?php\n");
    if ($namespace !== null) {
        fwrite($fp, "namespace " . $namespace . ";\n");
        $line = 3;
    }
    fwrite($fp, "\n");
    if ($type == "class")
        $line = writeClass($fp, $filename, $line);
    if ($type == "interface")
        $line = writeInterface($fp, $filename, $line);
    if ($type == "fixture")
        $line = writeFixture($fp, $filename, $line);
    fclose($fp);
    return $line;
}

function writeClass($fp, $name, $line) {
    fwrite($fp, "class " . $name . "\n");
    fwrite($fp, "{\n");
    fwrite($fp, "    \n");
    fwrite($fp, "}\n");
    fwrite($fp, "\n");
    return $line + 3;
}

function writeInterface($fp, $name, $line) {
    fwrite($fp, "interface " . $name . "\n");
    fwrite($fp, "{\n");
    fwrite($fp, "    \n");
    fwrite($fp, "}\n");
    fwrite($fp, "\n");
    return $line + 3;
}

function writeFixture($fp, $name, $line) {
    fwrite($fp, "use PHPUnit_Framework_TestCase;\n");
    fwrite($fp, "\n");
    fwrite($fp, "class " . $name . " extends PHPUnit_Framework_TestCase\n");
    fwrite($fp, "{\n");
    fwrite($fp, "    \n");
    fwrite($fp, "}\n");
    fwrite($fp, "\n");
    return $line + 5;
}

function getNamespace($runPath) {
    $namespace = "";
    $dir = exec("oi conf read php.namespace.basepath");
    if ($dir != "") {
        $dir = realpath($dir);
        if (!file_exists($dir))
            $dir = getcwd();
    } else {
        $dir = getcwd();
    }
    if (file_exists($dir))
        $dir = $dir . DIRECTORY_SEPARATOR;
    if (strlen($runPath) > strlen($dir)) {
        $namespace = 
            substr(
                $runPath,
                strlen($dir),
                strlen($runPath) - strlen($dir));
        $namespace = str_replace('/', '\\', $namespace);
    }
    $basenamespace = exec("oi conf read php.namespace.base");
    if ($basenamespace != "") {
        if ($namespace != "")
            $namespace = $basenamespace . '\\' . $namespace;
        else
            $namespace = $basenamespace;
    }
    if ($namespace == "")
        return null;
    return $namespace;
}

function waitForEndOfCommand() {
    while (TRUE) {
        $line = readline();
        if ($line == "end-of-command")
            break;
    }
}

function endsWith($haystack, $needle) {
    return $needle === "" || substr($haystack, -strlen($needle)) === $needle;
}
