@ECHO OFF

SET ROOT=%~d0%~p0%
SET WORKING_DIR=%1
SET DEFAULT_LANGUAGE=%2
SET ENABLED_LANGUAGES=%3
SET WATCH_PATH=%WORKING_DIR%

SET ROOTPOINT="null"
SET SOLUTION="null"
cd "%WORKING_DIR%"

for /f "" %%i IN ('oi conf read rootpoint') do (
	SET ROOTPOINT=%%i
	for /l %%a in (1,1,31) do if "!ROOTPOINT:~-1!"==" " set ROOTPOINT=!ROOTPOINT:~0,-1!
)
for /f "" %%i IN ('oi conf read autotest.solution') do (
	SET SOLUTION=%%i
	for /l %%a in (1,1,31) do if "!SOLUTION:~-1!"==" " set SOLUTION=!SOLUTION:~0,-1!
)

if %SOLUTION% neq "null" (
	SET WATCH_PATH=%ROOTPOINT%\%SOLUTION%
)

REM oi process start %ROOT%\bin\AutoTest.Net\AutoTest.WinForms.exe "%WATCH_PATH%"
REM oi process start %ROOT%\bin\ContinuousTests\AutoTest.GtkSharp.exe "%WATCH_PATH%"
oi process start %ROOT%\bin\ContinuousTests\ContinuousTests.exe "%WATCH_PATH%"