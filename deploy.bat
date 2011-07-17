@echo off

SET ROOT=%~d0%~p0%
SET BINARYDIR="%ROOT%build_output"
SET DEPLOYDIR="%ROOT%ReleaseBinaries"
SET LIB="%DIR%lib"

IF EXIST %BINARYDIR% (
  rmdir /Q /S %BINARYDIR%
)
mkdir %BINARYDIR%

IF EXIST %DEPLOYDIR% (
  rmdir /Q /S %DEPLOYDIR%
)
mkdir %DEPLOYDIR%

mkdir %DEPLOYDIR%\CodeEngine

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %SOURCEDIR%OpenIDENet.sln  /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %SOURCEDIR%OpenIDENet.CodeEngine.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild

xcopy %BINARYDIR%\Castle*.* %DEPLOYDIR%
copy %BINARYDIR%\oi.exe %DEPLOYDIR%\oi.exe
copy %BINARYDIR%\OpenIDENet.dll %DEPLOYDIR%\OpenIDENet.dll
copy %ROOT%\initialize.rb %DEPLOYDIR%\initialize.rb
xcopy /S /I %LIB%\EditorEngine %DEPLOYDIR%\EditorEngine
xcopy /S /I %LIB%\AutoTest.Net %DEPLOYDIR%\AutoTest.Net
xcopy /S /I %LIB%\ContinuousTests %DEPLOYDIR%\ContinuousTests
xcopy /S /I %ROOT%\templates %DEPLOYDIR%\templates
copy %ROOT%\oi\oi.bat %DEPLOYDIR%\oi.bat
copy %BINARYDIR%\OpenIDENet.CodeEngine.exe %DEPLOYDIR%\CodeEngine\OpenIDENet.CodeEngine.exe
copy %BINARYDIR%\OpenIDENet.CodeEngine.Core.dll %DEPLOYDIR%\CodeEngine\OpenIDENet.CodeEngine.Core.dll
