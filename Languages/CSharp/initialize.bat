@ECHO OFF

SET ROOT=%~d0%~p0%
SET WORKING_DIR=%1
SET DEFAULT_LANGUAGE=%2
SET ENABLED_LANGUAGES=%3

REM oi process-start %ROOT%\bin\AutoTest.Net\AutoTest.WinForms.exe "%WORKING_DIR%"
oi process-start %ROOT%\bin\ContinuousTests\ContinuousTests.exe "%WORKING_DIR%"
