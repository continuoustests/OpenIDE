@ECHO OFF
SET SCRIPTDIR=%~dp0

IF "%1" == "initialize" (
	oi process-start %SCRIPTDIR%\C#-plugin\C#.exe %* --process-hidden
) ELSE (
	%SCRIPTDIR%\C#-plugin\C#.exe send %*
)