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



mkdir $DEPLOYDIR/.OpenIDE

touch $DEPLOYDIR/.OpenIDE/oi.config

mkdir $DEPLOYDIR/.OpenIDE/Languages
mkdir $DEPLOYDIR/.OpenIDE/Languages/C#-plugin
mkdir $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/bin
mkdir $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/bin/AutoTest.Net
mkdir $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/bin/ContinuousTests
mkdir $DEPLOYDIR/.OpenIDE/Languages/go-plugin
mkdir $DEPLOYDIR/.OpenIDE/Languages/go-plugin/rscripts
mkdir $DEPLOYDIR/.OpenIDE/Languages/go-plugin/graphics
mkdir $DEPLOYDIR/.OpenIDE/Languages/python-plugin
mkdir $DEPLOYDIR/.OpenIDE/Languages/python-plugin/rscripts
mkdir $DEPLOYDIR/.OpenIDE/Languages/python-plugin/graphics
mkdir $DEPLOYDIR/.OpenIDE/Languages/js-plugin
mkdir $DEPLOYDIR/.OpenIDE/Languages/js-plugin/lib
mkdir $DEPLOYDIR/.OpenIDE/Languages/php-plugin

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
#xbuild .OpenIDE/Languages/CSharp/CSharp.sln /target:rebuild /property:OutDir=$BINARYDIR/ /p:Configuration=Release;

cp $BINARYDIR/CoreExtensions.dll $DEPLOYDIR/
cp $BINARYDIR/oi.exe $DEPLOYDIR/
cp $BINARYDIR/OpenIDE.dll $DEPLOYDIR/
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/
cp -r $LIB/EditorEngine/* $DEPLOYDIR/EditorEngine
cp $ROOT/oi/oi $DEPLOYDIR/oi
cp $BINARYDIR/OpenIDE.CodeEngine.exe $DEPLOYDIR/CodeEngine/OpenIDE.CodeEngine.exe
cp $BINARYDIR/OpenIDE.CodeEngine.Core.dll $DEPLOYDIR/CodeEngine/OpenIDE.CodeEngine.Core.dll
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/CodeEngine/
cp $BINARYDIR/CoreExtensions.dll $DEPLOYDIR/CodeEngine/
cp $ROOT/lib/FSWatcher/FSWatcher.dll $DEPLOYDIR/CodeEngine/
cp $BINARYDIR/OpenIDE.EventListener.exe $DEPLOYDIR/EventListener/

cp -r $ROOT/oi/script-templates/* $DEPLOYDIR/.OpenIDE/scripts/templates
cp -r $ROOT/oi/rscript-templates/* $DEPLOYDIR/.OpenIDE/rscripts/templates
cp -r $ROOT/oi/test-templates/* $DEPLOYDIR/.OpenIDE/test/templates

cp -r $ROOT/oi/rscripts/* $DEPLOYDIR/.OpenIDE/rscripts

cp $ROOT/Languages/CSharp/C# $DEPLOYDIR/.OpenIDE/Languages/C#
cp $ROOT/Languages/CSharp/language.oicfgoptions $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/language.oicfgoptions
cp $BINARYDIR/C#.exe $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/C#.exe
cp $BINARYDIR/CoreExtensions.dll $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/CoreExtensions.dll
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/OpenIDE.Core.dll
cp $ROOT/lib/FSWatcher/FSWatcher.dll $DEPLOYDIR/CodeEngine/
cp $BINARYDIR/ICSharpCode.NRefactory.CSharp.dll $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/ICSharpCode.NRefactory.CSharp.dll
cp $BINARYDIR/ICSharpCode.NRefactory.dll $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/ICSharpCode.NRefactory.dll
cp $BINARYDIR/Mono.Cecil.dll $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/Mono.Cecil.dll
cp -r $ROOT/Languages/CSharp/templates/* $DEPLOYDIR/.OpenIDE/Languages/C#-plugin
cp $ROOT/Languages/CSharp/initialize.sh $DEPLOYDIR/.OpenIDE/Languages/C#-plugin
cp -r $CSHARP_BIN/AutoTest.Net/* $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/bin/AutoTest.Net
cp -r $CSHARP_BIN/ContinuousTests/* $DEPLOYDIR/.OpenIDE/Languages/C#-plugin/bin/ContinuousTests

cp $ROOT/Languages/go/bin/go $DEPLOYDIR/.OpenIDE/Languages/go
cp $ROOT/Languages/go/rscripts/go-build.sh $DEPLOYDIR/.OpenIDE/Languages/go-plugin/rscripts/go-build.sh
cp $ROOT/Languages/go/graphics/* $DEPLOYDIR/.OpenIDE/Languages/go-plugin/graphics/

cp $ROOT/Languages/python/python.py $DEPLOYDIR/.OpenIDE/Languages/python

cp $ROOT/Languages/js/js.js $DEPLOYDIR/.OpenIDE/Languages/js
cp $ROOT/Languages/js/js-plugin/lib/parse-js.js $DEPLOYDIR/.OpenIDE/Languages/js-plugin/lib/parse-js.js
cp $ROOT/Languages/js/js-plugin/lib/parse-js.License $DEPLOYDIR/.OpenIDE/Languages/js-plugin/lib/parse-js.License
cp $ROOT/Languages/js/js-plugin/lib/carrier.js $DEPLOYDIR/.OpenIDE/Languages/js-plugin/lib/carrier.js
cp $ROOT/Languages/js/js-plugin/lib/carrier.License $DEPLOYDIR/.OpenIDE/Languages/js-plugin/lib/carrier.License

cp $ROOT/Languages/php/php.php $DEPLOYDIR/.OpenIDE/Languages/php
