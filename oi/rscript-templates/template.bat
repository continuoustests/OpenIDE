@ECHO OFF

if "%1" == "reactive-script-reacts-to" (
	rem Write one event pr line that this script will react to
	rem echo "goto*.cs|*"
	GOTO end
)

rem Write scirpt code here. First parameter contains the event %~1
:end
