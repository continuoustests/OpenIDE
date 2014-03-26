@echo off

SET ROOT=%~d0%~p0%
SET BINARYDIR="%ROOT%build_output"
SET DEPLOYDIR="%ROOT%ReleaseBinaries"
SET PACKAGEDIR="%ROOT%Packages"
SET LIB="%ROOT%lib"
SET CSHARP_BIN="%ROOT%Languages\CSharp\lib"
SET LANGUAGES="%ROOT%Languages\"

IF EXIST %BINARYDIR% (
  rmdir /Q /S %BINARYDIR%
)
mkdir %BINARYDIR%

IF EXIST %DEPLOYDIR% (
  rmdir /Q /S %DEPLOYDIR%
)
mkdir %DEPLOYDIR%

IF EXIST %PACKAGEDIR% (
  rmdir /Q /S %PACKAGEDIR%
)
mkdir %PACKAGEDIR%

mkdir %DEPLOYDIR%\CodeEngine
mkdir %DEPLOYDIR%\EditorEngine
mkdir %DEPLOYDIR%\EventListener
mkdir %DEPLOYDIR%\tests
mkdir %DEPLOYDIR%\Packaging

mkdir %DEPLOYDIR%\.OpenIDE

mkdir %PACKAGEDIR%\oipkg
mkdir %PACKAGEDIR%\C#-files
mkdir %PACKAGEDIR%\C#-files\bin
mkdir %PACKAGEDIR%\C#-files\bin\AutoTest.Net
mkdir %PACKAGEDIR%\C#-files\bin\ContinuousTests
mkdir %PACKAGEDIR%\python-files
mkdir %PACKAGEDIR%\python-files\rscripts
mkdir %PACKAGEDIR%\python-files\graphics
mkdir %PACKAGEDIR%\js-files
mkdir %PACKAGEDIR%\js-files\lib
mkdir %PACKAGEDIR%\php-files

mkdir %DEPLOYDIR%\.OpenIDE\scripts
mkdir %DEPLOYDIR%\.OpenIDE\scripts\templates

mkdir %DEPLOYDIR%\.OpenIDE\rscripts
mkdir %DEPLOYDIR%\.OpenIDE\rscripts\templates

mkdir %DEPLOYDIR%\.OpenIDE\test
mkdir %DEPLOYDIR%\.OpenIDE\test\templates

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%OpenIDE.sln  /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%OpenIDE.CodeEngine.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%PackageManager\oipckmngr\oipckmngr.csproj /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
REM %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%.OpenIDE\languages\CSharp\CSharp.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild

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

REM Packages
REM C#
copy %ROOT%\Languages\CSharp\C#.oilnk %PACKAGEDIR%\C#.oilnk
copy %ROOT%\Languages\CSharp\language.oicfgoptions %PACKAGEDIR%\C#-files\language.oicfgoptions
copy %ROOT%\Languages\CSharp\package.json.CT %PACKAGEDIR%\C#-files\package.json
copy %BINARYDIR%\C#.exe %PACKAGEDIR%\C#-files\C#.exe
copy %BINARYDIR%\OpenIDE.Core.dll %PACKAGEDIR%\C#-files\OpenIDE.Core.dll
copy %BINARYDIR%\Newtonsoft.Json.dll %PACKAGEDIR%\C#-files\Newtonsoft.Json.dll
copy %BINARYDIR%\ICSharpCode.NRefactory.CSharp.dll %PACKAGEDIR%\C#-files\ICSharpCode.NRefactory.CSharp.dll
copy %BINARYDIR%\ICSharpCode.NRefactory.dll %PACKAGEDIR%\C#-files\ICSharpCode.NRefactory.dll
copy %BINARYDIR%\Mono.Cecil.dll %PACKAGEDIR%\C#-files\Mono.Cecil.dll
xcopy /S /I /E %ROOT%\Languages\CSharp\templates %PACKAGEDIR%\C#-files
copy %ROOT%\Languages\CSharp\initialize.bat %PACKAGEDIR%\C#-files
copy %ROOT%\Languages\CSharp\initialize.sh %PACKAGEDIR%\C#-files
xcopy /S /I /E %CSHARP_BIN%\AutoTest.Net %PACKAGEDIR%\C#-files\bin\AutoTest.Net
xcopy /S /I /E %CSHARP_BIN%\ContinuousTests %PACKAGEDIR%\C#-files\bin\ContinuousTests

REM php
xcopy /S /I /E %LANGUAGES%\php %DEPLOYDIR%\.OpenIDE\languages

REM Building packages
ECHO Building packages

%DEPLOYDIR%\oi package build "Packages\C#" %PACKAGEDIR%/oipkg
rmdir /Q /S %PACKAGEDIR%\C#-files\bin
del %PACKAGEDIR%\C#-files\initialize.*
del %PACKAGEDIR%\C#-files\package.json
copy %ROOT%\Languages\CSharp\package.json %PACKAGEDIR%\C#-files\package.json
%DEPLOYDIR%\oi package build "Packages\C#" %PACKAGEDIR%/oipkg

del %DEPLOYDIR%\.OpenIDE\oi-definitions.json
del %DEPLOYDIR%\oi-definitions.json