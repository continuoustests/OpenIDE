#!/bin/bash

ROOT=$(cd $(dirname "$0"); pwd)
BINARYDIR=$(cd $(dirname "$0"); pwd)/build_output
DEPLOYDIR=$(cd $(dirname "$0"); pwd)/ReleaseBinaries
LIB=$(cd $(dirname "$0"); pwd)/lib
CSHARP_BIN=$(cd $(dirname "$0"); pwd)/Languages/CSharp/lib
LANGUAGES=$(cd $(dirname "$0"); pwd)/Languages

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

mkdir $DEPLOYDIR/Languages
mkdir $DEPLOYDIR/Languages/C#
mkdir $DEPLOYDIR/Languages/C#/bin
mkdir $DEPLOYDIR/Languages/C#/bin/AutoTest.Net
mkdir $DEPLOYDIR/Languages/C#/bin/ContinuousTests

mkdir $DEPLOYDIR/scripts
mkdir $DEPLOYDIR/scripts/templates

mkdir $DEPLOYDIR/rscripts
mkdir $DEPLOYDIR/rscripts/templates

chmod +x $CSHARP_BIN/ContinuousTests/AutoTest.*.exe
chmod +x $CSHARP_BIN/ContinuousTests/ContinuousTests.exe

echo $BINARYDIR

xbuild OpenIDE.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;
xbuild OpenIDE.CodeEngine.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;
xbuild Languages/CSharp/CSharp.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;

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

cp -r $ROOT/oi/script-templates/* $DEPLOYDIR/scripts/templates
cp -r $ROOT/oi/rscript-templates/* $DEPLOYDIR/rscripts/templates

cp $BINARYDIR/C#.exe $DEPLOYDIR/Languages/C#.exe
cp -r $ROOT/Languages/CSharp/templates/* $DEPLOYDIR/Languages/C#
cp $ROOT/Languages/CSharp/initialize.sh $DEPLOYDIR/Languages/C#
cp -r $CSHARP_BIN/AutoTest.Net/* $DEPLOYDIR/Languages/C#/bin/AutoTest.Net
cp -r $CSHARP_BIN/ContinuousTests/* $DEPLOYDIR/Languages/C#/bin/ContinuousTests

cp $LANGUAGES/go/bin/go $DEPLOYDIR/Languages/go
