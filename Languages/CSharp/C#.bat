@ECHO OFF
SET SCRIPTDIR=%~dp0

IF "%1" == "initialize" (
	%SCRIPTDIR%\C#-plugin\C#.exe %*
) ELSE (
	%SCRIPTDIR%\C#-plugin\C#.exe send %*
)