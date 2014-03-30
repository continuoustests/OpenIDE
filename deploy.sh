#!/bin/bash

ROOT=$(cd $(dirname "$0"); pwd)
BINARYDIR=$(cd $(dirname "$0"); pwd)/build_output
DEPLOYDIR=$(cd $(dirname "$0"); pwd)/ReleaseBinaries
LIB=$(cd $(dirname "$0"); pwd)/lib

if [ -d $BINARYDIR ]; then
{
    rm -r $BINARYDIR/
}
fi
if [ -d $DEPLOYDIR ]; then
{
    rm -r $DEPLOYDIR/
}
fi

mkdir $BINARYDIR
mkdir $DEPLOYDIR
mkdir $PACKAGEDIR
mkdir $DEPLOYDIR/EditorEngine
mkdir $DEPLOYDIR/CodeEngine
mkdir $DEPLOYDIR/EventListener
mkdir $DEPLOYDIR/tests

mkdir $DEPLOYDIR/.OpenIDE

mkdir $DEPLOYDIR/.OpenIDE/scripts
mkdir $DEPLOYDIR/.OpenIDE/scripts/templates

mkdir $DEPLOYDIR/.OpenIDE/rscripts
mkdir $DEPLOYDIR/.OpenIDE/rscripts/templates

mkdir $DEPLOYDIR/.OpenIDE/test
mkdir $DEPLOYDIR/.OpenIDE/test/templates

xbuild OpenIDE.sln /target:rebuild /property:OutDir=$BINARYDIR/ /p:Configuration=Release;
xbuild OpenIDE.CodeEngine.sln /target:rebuild /property:OutDir=$BINARYDIR/ /p:Configuration=Release;
xbuild PackageManager/oipckmngr/oipckmngr.csproj /target:rebuild /property:OutDir=$BINARYDIR/ /p:Configuration=Release;

# oi
cp $ROOT/oi/oi $DEPLOYDIR/oi
cp $ROOT/oi/oi.bat $DEPLOYDIR/oi.bat
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

# Reactive scripts
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
