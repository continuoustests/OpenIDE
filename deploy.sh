#!/bin/bash

ROOT=$(cd $(dirname "$0"); pwd)
BINARYDIR=$(cd $(dirname "$0"); pwd)/build_output
DEPLOYDIR=$(cd $(dirname "$0"); pwd)/ReleaseBinaries
LIB=$(cd $(dirname "$0"); pwd)/lib

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
mkdir $DEPLOYDIR/AutoTest.Net
mkdir $DEPLOYDIR/ContinuousTests
chmod +x $LIB/ContinuousTests/AutoTest.*.exe
chmod +x $LIB/ContinuousTests/ContinuousTests.exe

mkdir $DEPLOYDIR/Languages
mkdir $DEPLOYDIR/Languages/C#

echo $BINARYDIR

xbuild OpenIDE.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;
xbuild OpenIDE.CodeEngine.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;
xbuild Languages/CSharp/CSharp.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;

cp $BINARYDIR/oi.exe $DEPLOYDIR/
cp $BINARYDIR/OpenIDE.dll $DEPLOYDIR/
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/
cp $ROOT/initialize.rb $DEPLOYDIR/initialize.rb
cp -r $LIB/EditorEngine/* $DEPLOYDIR/EditorEngine
cp -r $LIB/AutoTest.Net/* $DEPLOYDIR/AutoTest.Net
cp -r $LIB/ContinuousTests/* $DEPLOYDIR/ContinuousTests
cp $ROOT/oi/oi $DEPLOYDIR/oi
cp $BINARYDIR/OpenIDE.CodeEngine.exe $DEPLOYDIR/CodeEngine/OpenIDE.CodeEngine.exe
cp $BINARYDIR/OpenIDE.CodeEngine.Core.dll $DEPLOYDIR/CodeEngine/OpenIDE.CodeEngine.Core.dll
cp $BINARYDIR/OpenIDE.Core.dll $DEPLOYDIR/CodeEngine/

cp $BINARYDIR/C#.exe $DEPLOYDIR/Languages/C#.exe
cp -r $ROOT/Languages/CSharp/templates/* $DEPLOYDIR/Languages/C#
