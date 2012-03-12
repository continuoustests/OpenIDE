@ECHO OFF
REM First parameter is the execution location of this script instance

if "%2" == "get-command-definitions" (
	REM Definition format usually represented as a single line:

	REM Script description|
	REM command1|"Command1 description"
	REM 	param|"Param description" end
	REM end
	REM command2|"Command2 description"
	REM 	param|"Param description" end
	REM end

	ECHO "Script description"
	GOTO end
)

:end
