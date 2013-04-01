#!/bin/bash

ROOT=$(cd $(dirname "$0"); pwd)
BINARYDIR=$(cd $(dirname "$0"); pwd)/build_output
DEPLOYDIR=$(cd $(dirname "$0"); pwd)/ReleaseBinaries
LIB=$(cd $(dirname "$0"); pwd)/lib
CSHARP_BIN=$(cd $(dirname "$0"); pwd)/Languages/CSharp/lib

if [ ! -d $BINARYDIR ]; then
{
	mkdir $BINARYDIR
}
fi
if [ ! -d $DEPLOYDIR ]; then
{
	mkdir $DEPLOYDIR
}
fi

rm -r $BINARYDIR/*
rm -r $DEPLOYDIR/*
mkdir $DEPLOYDIR/EditorEngine
mkdir $DEPLOYDIR/CodeEngine
mkdir $DEPLOYDIR/EventListener
mkdir $DEPLOYDIR/tests
mkdir $DEPLOYDIR/Packaging

mkdir $DEPLOYDIR/.OpenIDE

touch $DEPLOYDIR/.OpenIDE/oi.config

mkdir $DEPLOYDIR/.OpenIDE/languages
mkdir $DEPLOYDIR/.OpenIDE/languages/C#-files
mkdir $DEPLOYDIR/.OpenIDE/languages/C#-files/bin
mkdir $DEPLOYDIR/.OpenIDE/languages/C#-files/bin/AutoTest.Net
mkdir $DEPLOYDIR/.OpenIDE/languages/C#-files/bin/ContinuousTests
mkdir $DEPLOYDIR/.OpenIDE/languages/go-files
mkdir $DEPLOYDIR/.OpenIDE/languages/go-files/rscripts
mkdir $DEPLOYDIR/.OpenIDE/languages/go-files/graphics
mkdir $DEPLOYDIR/.OpenIDE/languages/python-files
mkdir $DEPLOYDIR/.OpenIDE/languages/python-files/rscripts
mkdir $DEPLOYDIR/.OpenIDE/languages/python-files/graphics
mkdir $DEPLOYDIR/.OpenIDE/languages/js-files
mkdir $DEPLOYDIR/.OpenIDE/languages/js-files/lib
mkdir $DEPLOYDIR/.OpenIDE/languages/php-files

mkdir $DEPLOYDIR/.OpenIDE/scripts
mkdir $DEPLOYDIR/.OpenIDE/scripts/templates

mkdir $DEPLOYDIR/.OpenIDE/rscripts
mkdir $DEPLOYDIR/.OpenIDE/rscripts/templates

mkdir $DEPLOYDIR/.OpenIDE/test
mkdir $DEPLOYDIR/.OpenIDE/test/templates

chmod +x $CSHARP_BIN/ContinuousTests/AutoTest.*.exe
chmod +x $CSHARP_BIN/ContinuousTests/ContinuousTests.exe

echo $BINARYDIR

xbuild OpenIDE.sln /target:rebuild /property:OutDir=$BINARYDIR/ /p:Configuration=Release;
xbuild OpenIDE.CodeEngine.sln /target:rebuild /property:OutDir=$BINARYDIR/ /p:Configuration=Release;
xbuild PackageManager/oipckmngr/oipckmngr.csproj /target:rebuild /property:OutDir=$BINARYDIR/ /p:Configuration=Release;
#xbuild .OpenIDE/languages/CSharp/CSharp.sln /target:rebuild /property:OutDir=$BINARYDIR/ /p:Configuration=Release;

cp $BINARYDIR/CoreExtensions.dll $DEPLOYDIR/
cp $BINARYDIR/oi.exe $DEPLOYDIR/
cp $BINARYDIR/OpenIDE.dll $DEPLOYDIR/
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/
cp $BINARYDIR/Newtonsoft.Json.dll $DEPLOYDIR/
cp -r $LIB/EditorEngine/* $DEPLOYDIR/EditorEngine
cp $ROOT/oi/oi $DEPLOYDIR/oi
cp $BINARYDIR/OpenIDE.CodeEngine.exe $DEPLOYDIR/CodeEngine/OpenIDE.CodeEngine.exe
cp $BINARYDIR/OpenIDE.CodeEngine.Core.dll $DEPLOYDIR/CodeEngine/OpenIDE.CodeEngine.Core.dll
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/CodeEngine/
cp $BINARYDIR/CoreExtensions.dll $DEPLOYDIR/CodeEngine/
cp $ROOT/lib/FSWatcher/FSWatcher.dll $DEPLOYDIR/CodeEngine/
cp $BINARYDIR/OpenIDE.EventListener.exe $DEPLOYDIR/EventListener/
cp -r $ROOT/oi/tests/* $DEPLOYDIR/tests

cp $BINARYDIR/oipckmngr.exe $DEPLOYDIR/Packaging
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/Packaging
cp $BINARYDIR/Newtonsoft.Json.dll $DEPLOYDIR/Packaging
cp $BINARYDIR/SharpCompress.3.5.dll $DEPLOYDIR/Packaging

cp -r $ROOT/oi/script-templates/* $DEPLOYDIR/.OpenIDE/scripts/templates
cp -r $ROOT/oi/rscript-templates/* $DEPLOYDIR/.OpenIDE/rscripts/templates
cp -r $ROOT/oi/test-templates/* $DEPLOYDIR/.OpenIDE/test/templates

cp -r $ROOT/oi/rscripts/* $DEPLOYDIR/.OpenIDE/rscripts

cp $ROOT/Languages/CSharp/C# $DEPLOYDIR/.OpenIDE/languages/C#
cp $ROOT/Languages/CSharp/language.oicfgoptions $DEPLOYDIR/.OpenIDE/languages/C#-files/language.oicfgoptions
cp $BINARYDIR/C#.exe $DEPLOYDIR/.OpenIDE/languages/C#-files/C#.exe
cp $BINARYDIR/CoreExtensions.dll $DEPLOYDIR/.OpenIDE/languages/C#-files/CoreExtensions.dll
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/.OpenIDE/languages/C#-files/OpenIDE.Core.dll
cp $ROOT/lib/FSWatcher/FSWatcher.dll $DEPLOYDIR/CodeEngine/
cp $BINARYDIR/ICSharpCode.NRefactory.CSharp.dll $DEPLOYDIR/.OpenIDE/languages/C#-files/ICSharpCode.NRefactory.CSharp.dll
cp $BINARYDIR/ICSharpCode.NRefactory.dll $DEPLOYDIR/.OpenIDE/languages/C#-files/ICSharpCode.NRefactory.dll
cp $BINARYDIR/Mono.Cecil.dll $DEPLOYDIR/.OpenIDE/languages/C#-files/Mono.Cecil.dll
cp -r $ROOT/Languages/CSharp/templates/* $DEPLOYDIR/.OpenIDE/languages/C#-files
cp $ROOT/Languages/CSharp/initialize.sh $DEPLOYDIR/.OpenIDE/languages/C#-files
cp -r $CSHARP_BIN/AutoTest.Net/* $DEPLOYDIR/.OpenIDE/languages/C#-files/bin/AutoTest.Net
cp -r $CSHARP_BIN/ContinuousTests/* $DEPLOYDIR/.OpenIDE/languages/C#-files/bin/ContinuousTests

cp $ROOT/Languages/go/bin/go $DEPLOYDIR/.OpenIDE/languages/go
cp $ROOT/Languages/go/rscripts/go-build.sh $DEPLOYDIR/.OpenIDE/languages/go-files/rscripts/go-build.sh
cp $ROOT/Languages/go/graphics/* $DEPLOYDIR/.OpenIDE/languages/go-files/graphics/

cp $ROOT/Languages/python/python.py $DEPLOYDIR/.OpenIDE/languages/python

cp $ROOT/Languages/js/js.js $DEPLOYDIR/.OpenIDE/languages/js
cp $ROOT/Languages/js/js-files/lib/parse-js.js $DEPLOYDIR/.OpenIDE/languages/js-files/lib/parse-js.js
cp $ROOT/Languages/js/js-files/lib/parse-js.License $DEPLOYDIR/.OpenIDE/languages/js-files/lib/parse-js.License
cp $ROOT/Languages/js/js-files/lib/carrier.js $DEPLOYDIR/.OpenIDE/languages/js-files/lib/carrier.js
cp $ROOT/Languages/js/js-files/lib/carrier.License $DEPLOYDIR/.OpenIDE/languages/js-files/lib/carrier.License

cp $ROOT/Languages/php/php.php $DEPLOYDIR/.OpenIDE/languages/php
