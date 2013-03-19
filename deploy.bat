@echo off

SET ROOT=%~d0%~p0%
SET BINARYDIR="%ROOT%build_output"
SET DEPLOYDIR="%ROOT%ReleaseBinaries"
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

mkdir %DEPLOYDIR%\CodeEngine
mkdir %DEPLOYDIR%\EditorEngine
mkdir %DEPLOYDIR%\EventListener
mkdir %DEPLOYDIR%\tests

mkdir %DEPLOYDIR%\.OpenIDE
"" > %DEPLOYDIR%\.OpenIDE\oi.config

mkdir %DEPLOYDIR%\.OpenIDE\languages
mkdir %DEPLOYDIR%\.OpenIDE\languages\C#-files
mkdir %DEPLOYDIR%\.OpenIDE\languages\C#-files\bin
mkdir %DEPLOYDIR%\.OpenIDE\languages\C#-files\bin\AutoTest.Net
mkdir %DEPLOYDIR%\.OpenIDE\languages\C#-files\bin\ContinuousTests
mkdir %DEPLOYDIR%\.OpenIDE\languages\python-files
mkdir %DEPLOYDIR%\.OpenIDE\languages\python-files\rscripts
mkdir %DEPLOYDIR%\.OpenIDE\languages\python-files\graphics
mkdir %DEPLOYDIR%\.OpenIDE\languages\js-files
mkdir %DEPLOYDIR%\.OpenIDE\languages\js-files\lib
mkdir %DEPLOYDIR%\.OpenIDE\languages\php-files
mkdir %DEPLOYDIR%\.OpenIDE\scripts
mkdir %DEPLOYDIR%\.OpenIDE\scripts\templates
mkdir %DEPLOYDIR%\.OpenIDE\rscripts
mkdir %DEPLOYDIR%\.OpenIDE\rscripts\templates
mkdir %DEPLOYDIR%\.OpenIDE\test
mkdir %DEPLOYDIR%\.OpenIDE\test\templates

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%OpenIDE.sln  /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%OpenIDE.CodeEngine.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
REM %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%.OpenIDE\languages\CSharp\CSharp.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild

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
copy %ROOT%\lib\FSWatcher\FSWatcher.dll %DEPLOYDIR%\CodeEngine\FSWatcher.dll
copy %BINARYDIR%\OpenIDE.EventListener.exe %DEPLOYDIR%\EventListener\OpenIDE.EventListener.exe
xcopy /S /I /E %ROOT%\oi\tests %DEPLOYDIR%\tests

xcopy /S /I /E %ROOT%\oi\script-templates %DEPLOYDIR%\.OpenIDE\scripts\templates
xcopy /S /I /E %ROOT%\oi\rscript-templates %DEPLOYDIR%\.OpenIDE\rscripts\templates
xcopy /S /I /E %ROOT%\oi\test-templates %DEPLOYDIR%\.OpenIDE\test\templates

copy %ROOT%\Languages\CSharp\C#.bat %DEPLOYDIR%\.OpenIDE\languages\C#.bat
copy %ROOT%\Languages\CSharp\language.oicfgoptions %DEPLOYDIR%\.OpenIDE\languages\C#-files\language.oicfgoptions
copy %BINARYDIR%\C#.exe %DEPLOYDIR%\.OpenIDE\languages\C#-files\C#.exe
copy %BINARYDIR%\CoreExtensions.dll %DEPLOYDIR%\.OpenIDE\languages\C#-files\CoreExtensions.dll
copy %BINARYDIR%\OpenIDE.Core.dll %DEPLOYDIR%\.OpenIDE\languages\C#-files\OpenIDE.Core.dll
copy %BINARYDIR%\ICSharpCode.NRefactory.CSharp.dll %DEPLOYDIR%\.OpenIDE\languages\C#-files\ICSharpCode.NRefactory.CSharp.dll
copy %BINARYDIR%\ICSharpCode.NRefactory.dll %DEPLOYDIR%\.OpenIDE\languages\C#-files\ICSharpCode.NRefactory.dll
copy %BINARYDIR%\Mono.Cecil.dll %DEPLOYDIR%\.OpenIDE\languages\C#-files\Mono.Cecil.dll
xcopy /S /I /E %ROOT%\Languages\CSharp\templates %DEPLOYDIR%\.OpenIDE\languages\C#-files
copy %ROOT%\Languages\CSharp\initialize.bat %DEPLOYDIR%\.OpenIDE\languages\C#-files
xcopy /S /I /E %CSHARP_BIN%\AutoTest.Net %DEPLOYDIR%\.OpenIDE\languages\C#-files\bin\AutoTest.Net
xcopy /S /I /E %CSHARP_BIN%\ContinuousTests %DEPLOYDIR%\.OpenIDE\languages\C#-files\bin\ContinuousTests

rem To deploy the following .OpenIDE\languages uncomment these lines
rem xcopy %ROOT%\lANGUAGES\python\python.py %DEPLOYDIR%\.OpenIDE\languages

rem xcopy %LANGUAGES%\js\js.js %DEPLOYDIR%\.OpenIDE\languages
rem xcopy %LANGUAGES%\js\js-files\lib\parse-js.js %DEPLOYDIR%\.OpenIDE\languages\js-files\lib
rem xcopy %LANGUAGES%\js\js-files\lib\parse-js.License %DEPLOYDIR%\.OpenIDE\languages\js-files\lib
rem xcopy %LANGUAGES%\js\js-files\lib\carrier.js %DEPLOYDIR%\.OpenIDE\languages\js-files\lib
rem xcopy %LANGUAGES%\js\js-files\lib\carrier.License %DEPLOYDIR%\.OpenIDE\languages\js-files\lib

rem xcopy %LANGUAGES%\php\php.php %DEPLOYDIR%\.OpenIDE\languages
