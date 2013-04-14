@ECHO OFF
REM First parameter is the execution location of this script instance

REM Script parameters
REM		Param 1: Script run location
REM		Param 2: global profile name
REM		Param 3: local profile name
REM		Param 4-: Any passed argument
REM
REM When calling oi use the --profile=PROFILE_NAME and 
REM --global-profile=PROFILE_NAME argument to ensure calling scripts
REM with the right profile.
REM
REM To post back oi commands print command prefixed by command| to standard output
REM To post a comment print to std output prefixed by comment|
REM To post an error print to std output prefixed by error|


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
