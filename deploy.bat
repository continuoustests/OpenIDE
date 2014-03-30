@echo off

SET ROOT=%~d0%~p0%
SET BINARYDIR="%ROOT%build_output"
SET DEPLOYDIR="%ROOT%ReleaseBinaries"
SET LIB="%ROOT%lib"

IF EXIST %BINARYDIR% (
  rmdir /Q /S %BINARYDIR%
)
mkdir %BINARYDIR%

IF EXIST %DEPLOYDIR% (
  rmdir /Q /S %DEPLOYDIR%
)
mkdir %DEPLOYDIR%

mkdir %DEPLOYDIR%\CodeEngine
mkdir %DEPLOYDIR%\EditorEngine
mkdir %DEPLOYDIR%\EventListener
mkdir %DEPLOYDIR%\tests
mkdir %DEPLOYDIR%\Packaging

mkdir %DEPLOYDIR%\.OpenIDE

mkdir %DEPLOYDIR%\.OpenIDE\scripts
mkdir %DEPLOYDIR%\.OpenIDE\scripts\templates

mkdir %DEPLOYDIR%\.OpenIDE\rscripts
mkdir %DEPLOYDIR%\.OpenIDE\rscripts\templates

mkdir %DEPLOYDIR%\.OpenIDE\test
mkdir %DEPLOYDIR%\.OpenIDE\test\templates

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%OpenIDE.sln  /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%OpenIDE.CodeEngine.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%PackageManager\oipckmngr\oipckmngr.csproj /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild

REM oi
copy %ROOT%\oi\oi %DEPLOYDIR%\oi
copy %ROOT%\oi\oi.bat %DEPLOYDIR%\oi.bat
copy %BINARYDIR%\oi.exe %DEPLOYDIR%\oi.exe
copy %BINARYDIR%\OpenIDE.dll %DEPLOYDIR%\OpenIDE.dll
copy %BINARYDIR%\OpenIDE.Core.dll %DEPLOYDIR%\OpenIDE.Core.dll
copy %BINARYDIR%\Newtonsoft.Json.dll %DEPLOYDIR%\Newtonsoft.Json.dll

REM Code model engine
copy %BINARYDIR%\OpenIDE.CodeEngine.exe %DEPLOYDIR%\CodeEngine\OpenIDE.CodeEngine.exe
copy %BINARYDIR%\OpenIDE.CodeEngine.Core.dll %DEPLOYDIR%\CodeEngine\OpenIDE.CodeEngine.Core.dll
copy %BINARYDIR%\OpenIDE.Core.dll %DEPLOYDIR%\CodeEngine\OpenIDE.Core.dll
copy %BINARYDIR%\Newtonsoft.Json.dll %DEPLOYDIR%\CodeEngine\Newtonsoft.Json.dll
copy %ROOT%\lib\FSWatcher\FSWatcher.dll %DEPLOYDIR%\CodeEngine\FSWatcher.dll

REM Editor engine
xcopy /S /I /E %LIB%\EditorEngine %DEPLOYDIR%\EditorEngine

REM Event listener
copy %BINARYDIR%\OpenIDE.EventListener.exe %DEPLOYDIR%\EventListener\OpenIDE.EventListener.exe

REM Tests
xcopy /S /I /E %ROOT%\oi\tests %DEPLOYDIR%\tests

REM Package manager
copy %BINARYDIR%\oipckmngr.exe %DEPLOYDIR%\Packaging
copy %BINARYDIR%\OpenIDE.Core.dll %DEPLOYDIR%\Packaging
copy %BINARYDIR%\Newtonsoft.Json.dll %DEPLOYDIR%\Packaging
copy %BINARYDIR%\ICSharpCode.SharpZipLib.dll %DEPLOYDIR%\Packaging

REM Templates
xcopy /S /I /E %ROOT%\oi\script-templates %DEPLOYDIR%\.OpenIDE\scripts\templates
xcopy /S /I /E %ROOT%\oi\rscript-templates %DEPLOYDIR%\.OpenIDE\rscripts\templates
xcopy /S /I /E %ROOT%\oi\test-templates %DEPLOYDIR%\.OpenIDE\test\templates
