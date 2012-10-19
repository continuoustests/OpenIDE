@ECHO OFF
SET SCRIPTDIR=%~dp0

IF "%1" == "initialize" (
	oi process-start %SCRIPTDIR%\C#-plugin\C#.exe %* --process-hidden
	timeout 0.5
) ELSE (
	%SCRIPTDIR%\C#-plugin\C#.exe send %*
)