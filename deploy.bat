@echo off

SET ROOT=%~d0%~p0%
SET BINARYDIR="%ROOT%build_output"
SET DEPLOYDIR="%ROOT%ReleaseBinaries"
SET LIB="%ROOT%lib"
SET CSHARP_BIN="%ROOT%Languages\CSharp\lib"

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
mkdir %DEPLOYDIR%\Languages
mkdir %DEPLOYDIR%\Languages\C#
mkdir %DEPLOYDIR%\Languages\C#\bin
mkdir %DEPLOYDIR%\Languages\C#\bin\AutoTest.Net
mkdir %DEPLOYDIR%\Languages\C#\bin\ContinuousTests
mkdir %DEPLOYDIR%\scripts
mkdir %DEPLOYDIR%\scripts\templates
mkdir %DEPLOYDIR%\rscripts
mkdir %DEPLOYDIR%\rscripts\templates

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %SOURCEDIR%OpenIDE.sln  /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %SOURCEDIR%OpenIDE.CodeEngine.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %SOURCEDIR%Languages\CSharp\CSharp.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild

copy %BINARYDIR%\CoreExtensions.dll %DEPLOYDIR%\CoreExtensions.dll
copy %BINARYDIR%\oi.exe %DEPLOYDIR%\oi.exe
copy %BINARYDIR%\OpenIDE.dll %DEPLOYDIR%\OpenIDE.dll
copy %BINARYDIR%\OpenIDE.Core.dll %DEPLOYDIR%\OpenIDE.Core.dll
xcopy /S /I /E %LIB%\EditorEngine %DEPLOYDIR%\EditorEngine
copy %ROOT%\oi\oi.bat %DEPLOYDIR%\oi.bat
copy %BINARYDIR%\OpenIDE.CodeEngine.exe %DEPLOYDIR%\CodeEngine\OpenIDE.CodeEngine.exe
copy %BINARYDIR%\OpenIDE.CodeEngine.Core.dll %DEPLOYDIR%\CodeEngine\OpenIDE.CodeEngine.Core.dll
copy %BINARYDIR%\OpenIDE.Core.dll %DEPLOYDIR%\CodeEngine\OpenIDE.Core.dll
copy %BINARYDIR%\CoreExtensions.dll %DEPLOYDIR%\CodeEngine\CoreExtensions.dll

xcopy /S /I /E %ROOT%\oi\script-templates %DEPLOYDIR%\scripts\templates
xcopy /S /I /E %ROOT%\oi\rscript-templates %DEPLOYDIR%\rscripts\templates

copy %BINARYDIR%\C#.exe %DEPLOYDIR%\Languages\C#.exe
xcopy /S /I /E %ROOT%\Languages\CSharp\templates %DEPLOYDIR%\Languages\C#
copy %ROOT%\Languages\CSharp\initialize.bat %DEPLOYDIR%\Languages\C#
xcopy /S /I /E %CSHARP_BIN%\AutoTest.Net %DEPLOYDIR%\Languages\C#\bin\AutoTest.Net
xcopy /S /I /E %CSHARP_BIN%\ContinuousTests %DEPLOYDIR%\Languages\C#\bin\ContinuousTests
