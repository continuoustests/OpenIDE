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
mkdir %DEPLOYDIR%\Languages
mkdir %DEPLOYDIR%\Languages\C#-plugin
mkdir %DEPLOYDIR%\Languages\C#-plugin\bin
mkdir %DEPLOYDIR%\Languages\C#-plugin\bin\AutoTest.Net
mkdir %DEPLOYDIR%\Languages\C#-plugin\bin\ContinuousTests
mkdir %DEPLOYDIR%\Languages\python-plugin
mkdir %DEPLOYDIR%\Languages\python-plugin\rscripts
mkdir %DEPLOYDIR%\Languages\python-plugin\graphics
mkdir %DEPLOYDIR%\Languages\js-plugin
mkdir %DEPLOYDIR%\Languages\js-plugin\lib
mkdir %DEPLOYDIR%\Languages\php-plugin
mkdir %DEPLOYDIR%\scripts
mkdir %DEPLOYDIR%\scripts\templates
mkdir %DEPLOYDIR%\rscripts
mkdir %DEPLOYDIR%\rscripts\templates

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%OpenIDE.sln  /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%OpenIDE.CodeEngine.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild
REM %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %ROOT%Languages\CSharp\CSharp.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild

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
copy %BINARYDIR%\OpenIDE.EventListener.dll %DEPLOYDIR%\EventListener\OpenIDE.EventListener.dll

xcopy /S /I /E %ROOT%\oi\script-templates %DEPLOYDIR%\scripts\templates
xcopy /S /I /E %ROOT%\oi\rscript-templates %DEPLOYDIR%\rscripts\templates

copy %ROOT%\Languages\CSharp\C#.bat %DEPLOYDIR%\Languages\C#.bat
copy %ROOT%\Languages\CSharp\language.oicfgoptions %DEPLOYDIR%\Languages\C#-plugin\language.oicfgoptions
copy %BINARYDIR%\C#.exe %DEPLOYDIR%\Languages\C#-plugin\C#.exe
copy %BINARYDIR%\CoreExtensions.dll %DEPLOYDIR%\Languages\C#-plugin\CoreExtensions.dll
copy %BINARYDIR%\OpenIDE.Core.dll %DEPLOYDIR%\Languages\C#-plugin\OpenIDE.Core.dll
copy %BINARYDIR%\ICSharpCode.NRefactory.CSharp.dll %DEPLOYDIR%\Languages\C#-plugin\ICSharpCode.NRefactory.CSharp.dll
copy %BINARYDIR%\ICSharpCode.NRefactory.dll %DEPLOYDIR%\Languages\C#-plugin\ICSharpCode.NRefactory.dll
copy %BINARYDIR%\Mono.Cecil.dll %DEPLOYDIR%\Languages\C#-plugin\Mono.Cecil.dll
xcopy /S /I /E %ROOT%\Languages\CSharp\templates %DEPLOYDIR%\Languages\C#-plugin
copy %ROOT%\Languages\CSharp\initialize.bat %DEPLOYDIR%\Languages\C#-plugin
xcopy /S /I /E %CSHARP_BIN%\AutoTest.Net %DEPLOYDIR%\Languages\C#-plugin\bin\AutoTest.Net
xcopy /S /I /E %CSHARP_BIN%\ContinuousTests %DEPLOYDIR%\Languages\C#-plugin\bin\ContinuousTests

rem To deploy the following languages uncomment these lines
rem xcopy %LANGUAGES%python\python.py %DEPLOYDIR%\Languages

rem xcopy %LANGUAGES%js\js.js %DEPLOYDIR%\Languages
rem xcopy %LANGUAGES%js\js-plugin\lib\parse-js.js %DEPLOYDIR%\Languages\js-plugin\lib
rem xcopy %LANGUAGES%js\js-plugin\lib\parse-js.License %DEPLOYDIR%\Languages\js-plugin\lib
rem xcopy %LANGUAGES%js\js-plugin\lib\carrier.js %DEPLOYDIR%\Languages\js-plugin\lib
rem xcopy %LANGUAGES%js\js-plugin\lib\carrier.License %DEPLOYDIR%\Languages\js-plugin\lib

rem xcopy %LANGUAGES%php\php.php %DEPLOYDIR%\Languages
