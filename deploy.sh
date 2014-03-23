#!/bin/bash

ROOT=$(cd $(dirname "$0"); pwd)
BINARYDIR=$(cd $(dirname "$0"); pwd)/build_output
DEPLOYDIR=$(cd $(dirname "$0"); pwd)/ReleaseBinaries
PACKAGEDIR=$(cd $(dirname "$0"); pwd)/Packages
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
if [ ! -d $PACKAGEDIR ]; then
{
    mkdir $PACKAGEDIR
}
fi

rm -r $BINARYDIR/*
rm -r $DEPLOYDIR/*
rm -r $PACKAGEDIR/*
mkdir $DEPLOYDIR/EditorEngine
mkdir $DEPLOYDIR/CodeEngine
mkdir $DEPLOYDIR/EventListener
mkdir $DEPLOYDIR/tests
mkdir $DEPLOYDIR/Packaging

mkdir $DEPLOYDIR/.OpenIDE

mkdir $PACKAGEDIR
mkdir $PACKAGEDIR/oipkg
mkdir $PACKAGEDIR/C#-files
mkdir $PACKAGEDIR/C#-files/bin
mkdir $PACKAGEDIR/C#-files/bin/AutoTest.Net
mkdir $PACKAGEDIR/C#-files/bin/ContinuousTests
mkdir $PACKAGEDIR/go-files
mkdir $PACKAGEDIR/go-files/rscripts
mkdir $PACKAGEDIR/go-files/graphics
mkdir $PACKAGEDIR/python-files
mkdir $PACKAGEDIR/python-files/rscripts
mkdir $PACKAGEDIR/python-files/graphics
mkdir $PACKAGEDIR/js-files
mkdir $PACKAGEDIR/js-files/lib
mkdir $PACKAGEDIR/php-files

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

# oi
cp $ROOT/oi/oi $DEPLOYDIR/oi
cp $BINARYDIR/oi.exe $DEPLOYDIR/
cp $BINARYDIR/OpenIDE.dll $DEPLOYDIR/
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/
cp $BINARYDIR/Newtonsoft.Json.dll $DEPLOYDIR/

# Code model engine
cp $BINARYDIR/OpenIDE.CodeEngine.exe $DEPLOYDIR/CodeEngine/OpenIDE.CodeEngine.exe
cp $BINARYDIR/OpenIDE.CodeEngine.Core.dll $DEPLOYDIR/CodeEngine/OpenIDE.CodeEngine.Core.dll
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/CodeEngine/
cp $BINARYDIR/Newtonsoft.Json.dll $DEPLOYDIR/CodeEngine/
cp $ROOT/lib/FSWatcher/FSWatcher.dll $DEPLOYDIR/CodeEngine/

# Editor engine
cp -r $LIB/EditorEngine/* $DEPLOYDIR/EditorEngine

# Event listener
cp $BINARYDIR/OpenIDE.EventListener.exe $DEPLOYDIR/EventListener/

# Tests
cp -r $ROOT/oi/tests/* $DEPLOYDIR/tests
cp -r $ROOT/oi/rscripts/* $DEPLOYDIR/.OpenIDE/rscripts

# Package manager
cp $BINARYDIR/oipckmngr.exe $DEPLOYDIR/Packaging
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/Packaging
cp $BINARYDIR/Newtonsoft.Json.dll $DEPLOYDIR/Packaging
cp $BINARYDIR/ICSharpCode.SharpZipLib.dll $DEPLOYDIR/Packaging

# Templates
cp -r $ROOT/oi/script-templates/* $DEPLOYDIR/.OpenIDE/scripts/templates
cp -r $ROOT/oi/rscript-templates/* $DEPLOYDIR/.OpenIDE/rscripts/templates
cp -r $ROOT/oi/test-templates/* $DEPLOYDIR/.OpenIDE/test/templates

# Packages
# C#
cp $ROOT/Languages/CSharp/C#.oilnk $PACKAGEDIR/C#.oilnk
cp $ROOT/Languages/CSharp/language.oicfgoptions $PACKAGEDIR/C#-files/language.oicfgoptions
cp $ROOT/Languages/CSharp/package.json $PACKAGEDIR/C#-files/package.json
cp $BINARYDIR/C#.exe $PACKAGEDIR/C#-files/C#.exe
cp $BINARYDIR/OpenIDE.Core.dll $PACKAGEDIR/C#-files/OpenIDE.Core.dll
cp $BINARYDIR/Newtonsoft.Json.dll $PACKAGEDIR/C#-files/
cp $BINARYDIR/ICSharpCode.NRefactory.CSharp.dll $PACKAGEDIR/C#-files/ICSharpCode.NRefactory.CSharp.dll
cp $BINARYDIR/ICSharpCode.NRefactory.dll $PACKAGEDIR/C#-files/ICSharpCode.NRefactory.dll
cp $BINARYDIR/Mono.Cecil.dll $PACKAGEDIR/C#-files/Mono.Cecil.dll
cp -r $ROOT/Languages/CSharp/templates/* $PACKAGEDIR/C#-files
cp $ROOT/Languages/CSharp/initialize.sh $PACKAGEDIR/C#-files
cp -r $CSHARP_BIN/AutoTest.Net/* $PACKAGEDIR/C#-files/bin/AutoTest.Net
cp -r $CSHARP_BIN/ContinuousTests/* $PACKAGEDIR/C#-files/bin/ContinuousTests

# go
cp $ROOT/Languages/go/bin/go $PACKAGEDIR/go
cp $ROOT/Languages/go/package.json $PACKAGEDIR/go-files/package.json
cp $ROOT/Languages/go/rscripts/go-build.sh $PACKAGEDIR/go-files/rscripts/go-build.sh
cp $ROOT/Languages/go/graphics/* $PACKAGEDIR/go-files/graphics/

# python
cp -r $ROOT/Languages/python/* $PACKAGEDIR

# Javascript
cp $ROOT/Languages/js/js.js $PACKAGEDIR/js
cp -r $ROOT/Languages/js/js-files/* $PACKAGEDIR/js-files

# Php
cp -r $ROOT/Languages/php/* $PACKAGEDIR

# Building packages
echo "Building packages.."
$DEPLOYDIR/oi package build Packages/C\# $PACKAGEDIR/oipkg
$DEPLOYDIR/oi package build Packages/go $PACKAGEDIR/oipkg
$DEPLOYDIR/oi package build Packages/python $PACKAGEDIR/oipkg
$DEPLOYDIR/oi package build Packages/js $PACKAGEDIR/oipkg
$DEPLOYDIR/oi package build Packages/php $PACKAGEDIR/oipkg