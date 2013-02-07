#!/usr/bin/env node

var fs = require('fs');
var path = require('path');
var jsp = require("./js-plugin/lib/parse-js");
var carrier = require('./js-plugin/lib/carrier.js');

if (process.argv.length > 2) {
	var firstArg = process.argv[2];
	if (firstArg === "initialize") {
		return;
	}
	if (firstArg === "get-command-definitions")
		return;
	if (firstArg === "crawl-file-types") {
		console.log(".js");
		return;
	}
	if (firstArg === "crawl-source") {
		var inStream = fs.createReadStream(process.argv[3], {flags:'r'});
		carrier.carry(inStream).on('line', function(line) {
			handleItem(line);
		});
	}
}

function handleItem(item) {
	fs.stat(item, function(err, stats) {
		if (stats && stats.isDirectory())
			handleDirectory(item);
		else if (stats && stats.isFile() && item.substr(-3) === ".js")
			handleFile(item);
	});
}

function handleDirectory(dir) {
	fs.readdir(dir, function(err, files) {
		files.forEach(function(file) {
			handleItem(path.join(dir, file));
		});
	});
}

function handleFile(file) {
	fs.readFile(file, function(err,data){
		if(err) {
			console.error("Could not open file: %s", err);
			process.exit(1);
		}

		console.log('file|' + file + '|filesearch');
		var tokenizer = jsp.tokenizer(data.toString().replace("#!/usr/bin/env node", ""));
		readDefinitions(tokenizer);
	});
}

function readDefinitions(tokenizer) {

	var name = null;
	var assigned = false;
	var nextTokenProvided = false;
	var getToken = function(tokenizer){
		try {
			var token = tokenizer();
		} catch(err) {
			console.log("error|tokenize returned " + err);
			return null;
		}
		if (token.type == "eof")
			return null;
		return token;
	}
	var token = null;
	while (true) {
		if (nextTokenProvided == false)
			token = getToken(tokenizer);
		nextTokenProvided = false;
		if (token == null)
			break;
		if (token.type == "keyword" && token.value == "function" && token.nlb == true) {
			var func = tokenizer();
			var line = func.line + 1;
			var col = func.col + 1;
			console.log('signature||' + func.value + '|' + func.value + 
						'|function||' + line + '|' + col + '||typesearch');
		} else if (token.type == "name" && token.nlb == true) {
			name = token.value;
			while (true) {
				token = getToken(tokenizer);
				if (token == null)
					break;
				else if (token.type == "name" || (token.type == "punc" && token.value == "."))
					name = name + token.value
				else
					break;
			}
			nextTokenProvided = true;
		} else if (token.type == "operator" && token.value == "=") {
			assigned = true;
			if (name != null) {
				var line = token.line + 1;
				var col = token.col - name.length;
				if (name.substr(-10) === ".prototype") {
					console.log('signature||' + name + '|' + name + 
								'|prototype||' + line + '|' + col + '||typesearch');
				}
			}
		} else if (token.type == "keyword" && token.value == "function" && name != null && assigned == true) {
			var func = tokenizer();
			//console.log("Public object: " + name + " as " + func.value);
		} else {
			name = null;
			assigned = false;
		}
		//console.log(JSON.stringify(token, "", "  "));
	}
}
