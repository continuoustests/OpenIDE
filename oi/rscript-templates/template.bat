@ECHO OFF

if "%1" == "reactive-script-reacts-to" (
	rem Write one event pr line that this script will react to
	rem echo "goto*.cs|*"
	GOTO end
)

rem Write scirpt code here.
rem	Param 1: event
rem	Param 2: global profile name
rem	Param 3: local profile name

rem When calling other commands use the --profile=PROFILE_NAME and 
rem --global-profile=PROFILE_NAME argument to ensure calling scripts
rem with the right profile.
:end
