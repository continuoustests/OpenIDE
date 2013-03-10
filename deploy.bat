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

mkdir %DEPLOYDIR%\.OpenIDE
"" > %DEPLOYDIR%\.OpenIDE\oi.config

mkdir %DEPLOYDIR%\.OpenIDE\Languages
mkdir %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin
mkdir %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\bin
mkdir %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\bin\AutoTest.Net
mkdir %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\bin\ContinuousTests
mkdir %DEPLOYDIR%\.OpenIDE\Languages\python-plugin
mkdir %DEPLOYDIR%\.OpenIDE\Languages\python-plugin\rscripts
mkdir %DEPLOYDIR%\.OpenIDE\Languages\python-plugin\graphics
mkdir %DEPLOYDIR%\.OpenIDE\Languages\js-plugin
mkdir %DEPLOYDIR%\.OpenIDE\Languages\js-plugin\lib
mkdir %DEPLOYDIR%\.OpenIDE\Languages\php-plugin
mkdir %DEPLOYDIR%\.OpenIDE\scripts
mkdir %DEPLOYDIR%\.OpenIDE\scripts\templates
mkdir %DEPLOYDIR%\.OpenIDE\rscripts
mkdir %DEPLOYDIR%\.OpenIDE\rscripts\templates
mkdir %DEPLOYDIR%\.OpenIDE\test
mkdir %DEPLOYDIR%\.OpenIDE\test\templates

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%OpenIDE.sln  /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%OpenIDE.CodeEngine.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
REM %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%.OpenIDE\Languages\CSharp\CSharp.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild

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

xcopy /S /I /E %ROOT%\oi\script-templates %DEPLOYDIR%\.OpenIDE\scripts\templates
xcopy /S /I /E %ROOT%\oi\rscript-templates %DEPLOYDIR%\.OpenIDE\rscripts\templates
xcopy /S /I /E %ROOT%\oi\test-templates %DEPLOYDIR%\.OpenIDE\test\templates

copy %ROOT%\Languages\CSharp\C#.bat %DEPLOYDIR%\.OpenIDE\Languages\C#.bat
copy %ROOT%\Languages\CSharp\language.oicfgoptions %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\language.oicfgoptions
copy %BINARYDIR%\C#.exe %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\C#.exe
copy %BINARYDIR%\CoreExtensions.dll %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\CoreExtensions.dll
copy %BINARYDIR%\OpenIDE.Core.dll %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\OpenIDE.Core.dll
copy %BINARYDIR%\ICSharpCode.NRefactory.CSharp.dll %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\ICSharpCode.NRefactory.CSharp.dll
copy %BINARYDIR%\ICSharpCode.NRefactory.dll %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\ICSharpCode.NRefactory.dll
copy %BINARYDIR%\Mono.Cecil.dll %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\Mono.Cecil.dll
xcopy /S /I /E %ROOT%\Languages\CSharp\templates %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin
copy %ROOT%\Languages\CSharp\initialize.bat %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin
xcopy /S /I /E %CSHARP_BIN%\AutoTest.Net %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\bin\AutoTest.Net
xcopy /S /I /E %CSHARP_BIN%\ContinuousTests %DEPLOYDIR%\.OpenIDE\Languages\C#-plugin\bin\ContinuousTests

rem To deploy the following .OpenIDE\languages uncomment these lines
rem xcopy %ROOT%\LANGUAGES\python\python.py %DEPLOYDIR%\.OpenIDE\Languages

rem xcopy %LANGUAGES%\js\js.js %DEPLOYDIR%\.OpenIDE\Languages
rem xcopy %LANGUAGES%\js\js-plugin\lib\parse-js.js %DEPLOYDIR%\.OpenIDE\Languages\js-plugin\lib
rem xcopy %LANGUAGES%\js\js-plugin\lib\parse-js.License %DEPLOYDIR%\.OpenIDE\Languages\js-plugin\lib
rem xcopy %LANGUAGES%\js\js-plugin\lib\carrier.js %DEPLOYDIR%\.OpenIDE\Languages\js-plugin\lib
rem xcopy %LANGUAGES%\js\js-plugin\lib\carrier.License %DEPLOYDIR%\.OpenIDE\Languages\js-plugin\lib

rem xcopy %LANGUAGES%\php\php.php %DEPLOYDIR%\.OpenIDE\Languages
